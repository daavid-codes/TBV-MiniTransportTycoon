using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class GameDataTests
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly List<UnityEngine.Object> trackedObjects = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDownTrackedObjects()
        {
            for (int i = trackedObjects.Count - 1; i >= 0; i--)
            {
                if (trackedObjects[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(trackedObjects[i]);
                }
            }
            trackedObjects.Clear();
            Time.timeScale = 1f;

            var instanceField = typeof(GameData).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            if (instanceField != null)
            {
                instanceField.SetValue(null, null);
            }
        }

        [Test]
        public void Start_SetsInitialDateAndTriggersEvent()
        {
            GameData data = CreateGameData();
            bool eventTriggered = false;
            data.OnDataChanged += () => eventTriggered = true;

            Invoke(data, "Start");

            Assert.AreEqual(new DateTime(2026, 1, 1, 8, 0, 0), data.CurrentDate);
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void Money_Property_UpdatesValueAndTriggersEvent()
        {
            GameData data = CreateGameData();
            int eventCount = 0;
            data.OnDataChanged += () => eventCount++;

            data.Money = 5000;

            Assert.AreEqual(5000, data.Money);
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void CityName_Property_UpdatesValueAndTriggersEvent()
        {
            GameData data = CreateGameData();
            int eventCount = 0;
            data.OnDataChanged += () => eventCount++;

            data.CityName = "Budapest";

            Assert.AreEqual("Budapest", data.CityName);
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void TimeMultiplier_Property_UpdatesValueAndTriggersEvent()
        {
            GameData data = CreateGameData();
            int eventCount = 0;
            data.OnDataChanged += () => eventCount++;

            data.TimeMultiplier = 1.5f;

            Assert.AreEqual(1.5f, data.TimeMultiplier);
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void IsPaused_WhenSetToTrue_UpdatesTimeScaleAndTriggersEvent()
        {
            GameData data = CreateGameData();
            Time.timeScale = 1f;
            int eventCount = 0;
            data.OnDataChanged += () => eventCount++;

            data.IsPaused = true;

            Assert.IsTrue(data.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void IsPaused_WhenSetToFalse_UpdatesTimeScaleAndTriggersEvent()
        {
            GameData data = CreateGameData();
            Time.timeScale = 0f;
            int eventCount = 0;
            data.OnDataChanged += () => eventCount++;

            data.IsPaused = false;

            Assert.IsFalse(data.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void Money_WhenZeroOrLess_TriggersGameOver()
        {
            GameData data = CreateGameData();
            bool gameOverTriggered = false;
            data.OnGameOver += () => gameOverTriggered = true;

            data.Money = 0;

            Assert.IsTrue(data.IsGameOver);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(gameOverTriggered);
        }

        [Test]
        public void TrySpendMoney_WithZeroAmount_ReturnsTrue()
        {
            GameData data = CreateGameData();
            data.Money = 100;
            
            bool result = data.TrySpendMoney(0);

            Assert.IsTrue(result);
            Assert.AreEqual(100, data.Money);
        }

        [Test]
        public void TrySpendMoney_WithSufficientFunds_ReturnsTrueAndDeductsMoney()
        {
            GameData data = CreateGameData();
            data.Money = 500;
            
            bool result = data.TrySpendMoney(200);

            Assert.IsTrue(result);
            Assert.AreEqual(300, data.Money);
        }

        [Test]
        public void TrySpendMoney_WhenGameOver_ReturnsFalse()
        {
            GameData data = CreateGameData();
            data.Money = 0; // Game Over trigger
            
            bool result = data.TrySpendMoney(10);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void ReportError_EmptyMessage_DoesNothing()
        {
            GameData data = CreateGameData();
            data.ClearLastError();
            
            data.ReportError("");
            
            Assert.AreEqual(string.Empty, data.LastErrorMessage);
        }

        [TestCase(Materials.Wood, 50)]
        [TestCase(Materials.Steel, 25)]
        [TestCase(Materials.Paper, 10)]
        [TestCase(Materials.Iron, 5)]
        [TestCase(Materials.Coal, 100)]
        public void AddAndGetMaterial_UpdatesCorrectResource(Materials material, int amount)
        {
            GameData data = CreateGameData();
            int eventCount = 0;
            data.OnDataChanged += () => eventCount++;

            data.AddMaterial(material, amount);

            Assert.AreEqual(amount, data.GetMaterialAmount(material));
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void AddMaterial_WithZeroAmount_DoesNothing()
        {
            GameData data = CreateGameData();
            int eventCount = 0;
            data.OnDataChanged += () => eventCount++;

            data.AddMaterial(Materials.Wood, 0);

            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void ResourceProperties_UpdateValuesAndTriggerEvent()
        {
            GameData data = CreateGameData();
            int eventCount = 0;
            data.OnDataChanged += () => eventCount++;

            data.Iron = 10;
            data.Steel = 20;
            data.Wood = 30;
            data.Paper = 40;
            data.Coal = 50;

            Assert.AreEqual(10, data.Iron);
            Assert.AreEqual(20, data.Steel);
            Assert.AreEqual(30, data.Wood);
            Assert.AreEqual(40, data.Paper);
            Assert.AreEqual(50, data.Coal);
            Assert.AreEqual(5, eventCount);
        }

        [Test]
        public void TimeRoutine_UpdatesDateAndTriggersEvents()
        {
            GameData data = CreateGameData();
            Invoke(data, "Start");
            
            bool hourChanged = false;
            bool dayChanged = false;
            data.OnHourChanged += () => hourChanged = true;
            data.OnDayChanged += () => dayChanged = true;

            System.Collections.IEnumerator routine = (System.Collections.IEnumerator)InvokeWithReturn(data, "TimeRoutine");
            
            // Manuálisan leptetjük a coroutine-t
            for (int i = 0; i < 65; i++)
            {
                routine.MoveNext();
            }
            
            Assert.IsTrue(hourChanged);
            
            for (int i = 0; i < 24 * 60; i++)
            {
                routine.MoveNext();
            }
            
            Assert.IsTrue(dayChanged);
        }

        [Test]
        public void ProduceAllFacilities_DoesNotThrowWhenEmpty()
        {
            GameData data = CreateGameData();
            
            Assert.DoesNotThrow(() => data.ProduceAllFacilities());
        }

        private GameData CreateGameData()
        {
            GameObject go = Track(new GameObject("GameData"));
            return go.AddComponent<GameData>();
        }

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            trackedObjects.Add(obj);
            return obj;
        }

        private static void Invoke(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, InstanceFlags);
            if (method != null)
            {
                method.Invoke(target, args);
            }
        }

        private static object InvokeWithReturn(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, InstanceFlags);
            if (method != null)
            {
                return method.Invoke(target, args);
            }
            return null;
        }
    }
}