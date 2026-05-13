using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;
using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class TruckTests
    {
        private GameObject _truckGo;
        private Truck _truck;
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
            if (field != null)
            {
                field.SetValue(obj, value);
            }
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

            _truckGo = new GameObject("Truck");
            _truck = _truckGo.AddComponent<Truck>();
            _trackedObjects.Add(_truckGo);

            _gameDataGo = new GameObject("GameData");
            _gameData = _gameDataGo.AddComponent<GameData>();
            _trackedObjects.Add(_gameDataGo);
            
            var startMethod = typeof(Truck).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod?.Invoke(_truck, null);
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
        public void Awake_SetsCarTypeToTruck()
        {
            InvokePrivateMethod(_truck, "Awake");
            var carType = GetPrivateField<CarType>(_truck, "type");
            Assert.AreEqual(CarType.Truck, carType);
        }

        [Test]
        public void Reset_SetsCarTypeToTruck()
        {
            SetPrivateField(_truck, "type", CarType.Bus);
            InvokePrivateMethod(_truck, "Reset");
            var carType = GetPrivateField<CarType>(_truck, "type");
            Assert.AreEqual(CarType.Truck, carType);
        }

        [Test]
        public void OnValidate_SetsCarTypeToTruckAndClampsValues()
        {
            SetPrivateField(_truck, "type", CarType.Bus);
            SetPrivateField(_truck, "maxCarryingAmount", -10);
            SetPrivateField(_truck, "carryingAmount", 50);
            
            InvokePrivateMethod(_truck, "OnValidate");
            
            Assert.AreEqual(CarType.Truck, GetPrivateField<CarType>(_truck, "type"));
            Assert.AreEqual(0, _truck.MaxCarryingAmount);
            Assert.AreEqual(0, _truck.CarryingAmount);
        }

        [Test]
        public void SetMaterialType_SetsCorrectMaterial()
        {
            _truck.SetMaterialType(Materials.Steel);
            Assert.AreEqual(Materials.Steel, _truck.MaterialType);
        }

        [Test]
        public void SetCost_SetsCostValue()
        {
            _truck.SetCost(1500);
            Assert.AreEqual(1500, GetPrivateField<int>(_truck, "cost"));
        }

        [Test]
        public void SetMaxCarryingAmount_PositiveValue_SetsValue()
        {
            _truck.SetMaxCarryingAmount(1000);
            Assert.AreEqual(1000, _truck.MaxCarryingAmount);
        }

        [Test]
        public void SetMaxCarryingAmount_NegativeValue_ClampsToZero()
        {
            _truck.SetMaxCarryingAmount(-50);
            Assert.AreEqual(0, _truck.MaxCarryingAmount);
        }

        [Test]
        public void SetMaxCarryingAmount_ClampsCurrentCarryingAmount()
        {
            _truck.SetMaxCarryingAmount(500);
            _truck.LoadMaterial(400);
            
            _truck.SetMaxCarryingAmount(200);
            
            Assert.AreEqual(200, _truck.MaxCarryingAmount);
            Assert.AreEqual(200, _truck.CarryingAmount);
        }

        [Test]
        public void LoadMaterial_PositiveAmount_LoadsCorrectly()
        {
            _truck.SetMaxCarryingAmount(500);
            int loaded = _truck.LoadMaterial(100);
            
            Assert.AreEqual(100, loaded);
            Assert.AreEqual(100, _truck.CarryingAmount);
        }

        [Test]
        public void LoadMaterial_NegativeAmount_ReturnsZeroAndDoesNotLoad()
        {
            _truck.SetMaxCarryingAmount(500);
            int loaded = _truck.LoadMaterial(-50);
            
            Assert.AreEqual(0, loaded);
            Assert.AreEqual(0, _truck.CarryingAmount);
        }

        [Test]
        public void LoadMaterial_ExceedsCapacity_LoadsUpToMax()
        {
            _truck.SetMaxCarryingAmount(300);
            _truck.LoadMaterial(200);
            
            int loaded = _truck.LoadMaterial(200); // Only 100 space left
            
            Assert.AreEqual(100, loaded);
            Assert.AreEqual(300, _truck.CarryingAmount);
        }

        [Test]
        public void UnloadMaterial_PositiveAmount_UnloadsCorrectly()
        {
            _truck.SetMaxCarryingAmount(500);
            _truck.LoadMaterial(300);
            
            int unloaded = _truck.UnloadMaterial(100);
            
            Assert.AreEqual(100, unloaded);
            Assert.AreEqual(200, _truck.CarryingAmount);
        }

        [Test]
        public void UnloadMaterial_NegativeAmount_ReturnsZeroAndDoesNotUnload()
        {
            _truck.SetMaxCarryingAmount(500);
            _truck.LoadMaterial(300);
            
            int unloaded = _truck.UnloadMaterial(-50);
            
            Assert.AreEqual(0, unloaded);
            Assert.AreEqual(300, _truck.CarryingAmount);
        }

        [Test]
        public void UnloadMaterial_ExceedsCarryingAmount_UnloadsAvailableAmount()
        {
            _truck.SetMaxCarryingAmount(500);
            _truck.LoadMaterial(150);
            
            int unloaded = _truck.UnloadMaterial(300); // Tries to unload more than it has
            
            Assert.AreEqual(150, unloaded);
            Assert.AreEqual(0, _truck.CarryingAmount);
        }

        [Test]
        public void SetRoute_ClearsLoopRouteProperties()
        {
            SetPrivateField(_truck, "useLoopRoute", true);
            GetPrivateField<List<List<Vector3Int>>>(_truck, "loopRouteLegs").Add(new List<Vector3Int>());
            SetPrivateField(_truck, "nextLoopLegIndex", 1);

            _truck.SetRoute(new List<Vector3Int> { Vector3Int.zero });

            Assert.IsFalse(GetPrivateField<bool>(_truck, "useLoopRoute"));
            Assert.AreEqual(0, GetPrivateField<List<List<Vector3Int>>>(_truck, "loopRouteLegs").Count);
            Assert.AreEqual(0, GetPrivateField<int>(_truck, "nextLoopLegIndex"));
        }

        [Test]
        public void SetLoopRoute_WithNull_DisablesLooping()
        {
            _truck.SetLoopRoute(null);
            Assert.IsFalse(GetPrivateField<bool>(_truck, "useLoopRoute"));
        }

        [Test]
        public void SetLoopRoute_WithEmptyOrShortLegs_IsIgnored()
        {
            var newLoopLegs = new List<List<Vector3Int>>
            {
                new List<Vector3Int>(),
                new List<Vector3Int> { Vector3Int.zero } // Becomes empty after TrimLegStart
            };

            _truck.SetLoopRoute(newLoopLegs);

            Assert.AreEqual(0, GetPrivateField<List<List<Vector3Int>>>(_truck, "loopRouteLegs").Count);
            Assert.IsFalse(GetPrivateField<bool>(_truck, "useLoopRoute"));
        }

        [Test]
        public void Update_WhenNotUsingLoopRoute_DoesNothing()
        {
            SetPrivateField(_truck, "useLoopRoute", false);
            SetPrivateField(_truck, "isMoving", false);

            InvokePrivateMethod(_truck, "Update");

            // Ha továbbmegy és belemegy az if (loopRouteLegs.Count == 0) ágba, nextLoopLegIndex változhatna,
            // de early return miatt nem teszi.
            Assert.AreEqual(0, GetPrivateField<int>(_truck, "nextLoopLegIndex"));
        }

        [Test]
        public void HandleStopArrival_WithoutStartedLoop_DoesNothing()
        {
            SetPrivateField(_truck, "hasStartedLoopLeg", false);
            Assert.DoesNotThrow(() => InvokePrivateMethod(_truck, "HandleStopArrival"));
        }

        [Test]
        public void HandleStopArrival_WithoutDependencies_DoesNothing()
        {
            SetPrivateField(_truck, "hasStartedLoopLeg", true);
            SetPrivateField(_truck, "stopRoute", null);
            Assert.DoesNotThrow(() => InvokePrivateMethod(_truck, "HandleStopArrival"));
        }

        [Test]
        public void HandleMaterialTransferAtStop_WithNullGameController_DoesNothing()
        {
            SetPrivateField(_truck, "gameController", null);
            _truck.SetMaxCarryingAmount(500);
            _truck.LoadMaterial(100);

            InvokePrivateMethod(_truck, "HandleMaterialTransferAtStop", Vector3Int.zero);
            
            Assert.AreEqual(100, _truck.CarryingAmount);
        }

        [Test]
        public void StartNextLoopLeg_CyclesThroughLegs()
        {
            var path = new List<Vector3Int> { Vector3Int.zero, Vector3Int.one, Vector3Int.up };
            _truck.SetShuttleRoute(path); // 2 legs created, next index becomes 1

            Assert.AreEqual(1, GetPrivateField<int>(_truck, "nextLoopLegIndex"));
            
            InvokePrivateMethod(_truck, "StartNextLoopLeg");
            
            Assert.AreEqual(0, GetPrivateField<int>(_truck, "nextLoopLegIndex"));
        }
    }
}