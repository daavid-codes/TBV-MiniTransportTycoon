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
    }
}