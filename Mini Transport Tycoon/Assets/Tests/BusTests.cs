using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MiniTransportTycoon
{
    public class BusTests
    {
        private GameObject _busGo;
        private Bus _bus;
        private GameObject _gameDataGo;
        private GameData _gameData;
        private List<Object> _trackedObjects;

        private FieldInfo GetFieldInfo(System.Type type, string name)
        {
            if (type == null)
                return null;

            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
                return field;

            return GetFieldInfo(type.BaseType, name);
        }

        private T GetPrivateField<T>(object obj, string name)
        {
            var field = GetFieldInfo(obj.GetType(), name);
            return (T)field?.GetValue(obj);
        }

        private void SetPrivateField(object obj, string name, object value)
        {
            var field = GetFieldInfo(obj.GetType(), name);
            field?.SetValue(obj, value);
        }

        private object InvokePrivateMethod(object obj, string name, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return method?.Invoke(obj, parameters);
        }

        [SetUp]
        public void SetUp()
        {
            _trackedObjects = new List<Object>();

            _busGo = new GameObject("Bus");
            _bus = _busGo.AddComponent<Bus>();
            _trackedObjects.Add(_busGo);

            _gameDataGo = new GameObject("GameData");
            _gameData = _gameDataGo.AddComponent<GameData>();
            _trackedObjects.Add(_gameDataGo);
            
            // Manually invoke Start to ensure gameData is found, simulating Unity's lifecycle
            var startMethod = typeof(Bus).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod?.Invoke(_bus, null);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = _trackedObjects.Count - 1; i >= 0; i--)
            {
                if (_trackedObjects[i] != null)
                {
                    Object.DestroyImmediate(_trackedObjects[i]);
                }
            }
            _trackedObjects.Clear();
        }

        [Test]
        public void Awake_SetsCarTypeToBus()
        {
            var carType = GetPrivateField<CarType>(_bus, "type");
            Assert.AreEqual(CarType.Bus, carType);
        }

        [Test]
        public void SetMaxCarryingAmount_PositiveValue_SetsValue()
        {
            _bus.SetMaxCarryingAmount(100);
            Assert.AreEqual(100, _bus.MaxCarryingAmount);
        }

        [Test]
        public void SetMaxCarryingAmount_NegativeValue_ClampsToZero()
        {
            _bus.SetMaxCarryingAmount(-50);
            Assert.AreEqual(0, _bus.MaxCarryingAmount);
        }
        
        [Test]
        public void SetMaxCarryingAmount_ClampsCurrentCarryingAmount()
        {
            SetPrivateField(_bus, "carryingAmount", 150);
            _bus.SetMaxCarryingAmount(100);
            Assert.AreEqual(100, _bus.CarryingAmount);
        }

        [Test]
        public void SetCost_PositiveValue_SetsValue()
        {
            _bus.SetCost(2000);
            var cost = GetPrivateField<int>(_bus, "cost");
            Assert.AreEqual(2000, cost);
        }

        [Test]
        public void SetCost_NegativeValue_ClampsToZero()
        {
            _bus.SetCost(-100);
            var cost = GetPrivateField<int>(_bus, "cost");
            Assert.AreEqual(0, cost);
        }

        [Test]
        public void SetRoute_ClearsLoopRouteProperties()
        {
            SetPrivateField(_bus, "useLoopRoute", true);
            GetPrivateField<List<List<Vector3Int>>>(_bus, "loopRouteLegs").Add(new List<Vector3Int>());
            SetPrivateField(_bus, "nextLoopLegIndex", 1);

            _bus.SetRoute(new List<Vector3Int> { Vector3Int.zero });

            Assert.IsFalse(GetPrivateField<bool>(_bus, "useLoopRoute"));
            Assert.AreEqual(0, GetPrivateField<List<List<Vector3Int>>>(_bus, "loopRouteLegs").Count);
            Assert.AreEqual(0, GetPrivateField<int>(_bus, "nextLoopLegIndex"));
        }

        [Test]
        public void SetShuttleRoute_WithFullPath_CreatesTwoTrimmedLegs()
        {
            var path = new List<Vector3Int> { Vector3Int.zero, Vector3Int.one, Vector3Int.up };

            _bus.SetShuttleRoute(path);

            var legs = GetPrivateField<List<List<Vector3Int>>>(_bus, "loopRouteLegs");
            Assert.IsTrue(GetPrivateField<bool>(_bus, "useLoopRoute"));
            Assert.AreEqual(2, legs.Count);
            
            CollectionAssert.AreEqual(new[] { Vector3Int.one, Vector3Int.up }, legs[0]);
            CollectionAssert.AreEqual(new[] { Vector3Int.one, Vector3Int.zero }, legs[1]);
        }
        
        [Test]
        public void SetLoopRoute_WithNull_DisablesLooping()
        {
            _bus.SetLoopRoute(null);
            Assert.IsFalse(GetPrivateField<bool>(_bus, "useLoopRoute"));
        }

        [Test]
        public void SetLoopRoute_WithEmptyOrShortLegs_IsIgnored()
        {
            var newLoopLegs = new List<List<Vector3Int>>
            {
                new List<Vector3Int>(),
                new List<Vector3Int> { Vector3Int.zero }
            };

            _bus.SetLoopRoute(newLoopLegs);

            Assert.AreEqual(0, GetPrivateField<List<List<Vector3Int>>>(_bus, "loopRouteLegs").Count);
            Assert.IsFalse(GetPrivateField<bool>(_bus, "useLoopRoute"));
        }

        [Test]
        public void SetLoopRoute_WithValidLegs_EnablesLoopingAndStartsFirstLeg()
        {
            var newLoopLegs = new List<List<Vector3Int>>
            {
                new List<Vector3Int> { Vector3Int.zero, Vector3Int.one },
                new List<Vector3Int> { Vector3Int.one, Vector3Int.up, Vector3Int.down }
            };

            _bus.SetLoopRoute(newLoopLegs);

            var legs = GetPrivateField<List<List<Vector3Int>>>(_bus, "loopRouteLegs");
            Assert.IsTrue(GetPrivateField<bool>(_bus, "useLoopRoute"));
            Assert.AreEqual(2, legs.Count);
            Assert.IsTrue(GetPrivateField<bool>(_bus, "hasStartedLoopLeg"));
            Assert.AreEqual(1, GetPrivateField<int>(_bus, "nextLoopLegIndex"));
        }
        
        [Test]
        public void HandleStopArrival_WhenConditionsMet_AddsMoney()
        {
            _gameData.Money = 100;
            _bus.SetMaxCarryingAmount(50);
            SetPrivateField(_bus, "hasStartedLoopLeg", true);
            SetPrivateField(_bus, "stopRoute", new List<Vector3Int> { Vector3Int.zero, Vector3Int.one });
            var tilemapGo = new GameObject("Tilemap");
            _trackedObjects.Add(tilemapGo);
            SetPrivateField(_bus, "garageTilemap", tilemapGo.AddComponent<Tilemap>());

            InvokePrivateMethod(_bus, "HandleStopArrival");

            Assert.AreEqual(150, _gameData.Money);
        }

        [Test]
        public void HandleStopArrival_WhenDependenciesAreNull_DoesNothing()
        {
            _gameData.Money = 100;
            SetPrivateField(_bus, "hasStartedLoopLeg", true);
            SetPrivateField(_bus, "stopRoute", null);

            InvokePrivateMethod(_bus, "HandleStopArrival");

            Assert.AreEqual(100, _gameData.Money);
        }

        [Test]
        public void StartNextLoopLeg_CyclesThroughLegs()
        {
            var newLoopLegs = new List<List<Vector3Int>>
            {
                new List<Vector3Int> { Vector3Int.zero, Vector3Int.one },
                new List<Vector3Int> { Vector3Int.one, Vector3Int.up }
            };
            _bus.SetLoopRoute(newLoopLegs);
            Assert.AreEqual(1, GetPrivateField<int>(_bus, "nextLoopLegIndex"));

            InvokePrivateMethod(_bus, "StartNextLoopLeg");

            Assert.AreEqual(0, GetPrivateField<int>(_bus, "nextLoopLegIndex"));
        }
    }
}