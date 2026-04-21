using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TMPro;
using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class UIManagerTests
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
        public void ToggleEscapeMenu_WhenInactive_ActivatesMenuAndTogglesPause()
        {
            UIContext context = CreateContext();
            context.EscapeMenu.SetActive(false);
            context.Data.IsPaused = false;
            Time.timeScale = 1f;

            context.Manager.ToggleEscapeMenu();

            Assert.IsTrue(context.EscapeMenu.activeSelf);
            Assert.IsTrue(context.Data.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void ToggleEscapeMenu_WhenActive_DeactivatesMenuAndTogglesPause()
        {
            UIContext context = CreateContext();
            context.EscapeMenu.SetActive(true);
            context.Data.IsPaused = true;
            Time.timeScale = 0f;

            context.Manager.ToggleEscapeMenu();

            Assert.IsFalse(context.EscapeMenu.activeSelf);
            Assert.IsFalse(context.Data.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void UpdateUI_SetsTextValuesCorrectly()
        {
            UIContext context = CreateContext();
            
            context.Data.Money = 9999;
            context.Data.CityName = "TestCity";
            SetField(context.Data, "currentDate", new DateTime(2026, 5, 10, 14, 30, 0));

            Invoke(context.Manager, "UpdateUI");

            Assert.AreEqual("9999$", context.MoneyText.text);
            Assert.AreEqual("TestCity", context.CityText.text);
            Assert.AreEqual("2026. 05. 10. 14:30", context.DateText.text);
        }

        [Test]
        public void OnEnable_SubscribesToGameDataEvent_AndUpdatesUI()
        {
            UIContext context = CreateContext();
            
            Invoke(context.Manager, "OnEnable");
            
            context.Data.Money = 12345; 

            Assert.AreEqual("12345$", context.MoneyText.text);
        }

        [Test]
        public void OnDisable_UnsubscribesFromGameDataEvent()
        {
            UIContext context = CreateContext();
            
            Invoke(context.Manager, "OnEnable");
            Invoke(context.Manager, "OnDisable");
            
            context.Data.Money = 54321; 

            Assert.AreNotEqual("54321$", context.MoneyText.text);
        }

        [Test]
        public void IncreaseTimeMultiplier_MultipliesTimeScale_CapsAtFour()
        {
            UIContext context = CreateContext();
            Time.timeScale = 1f;

            context.Manager.IncreaseTimeMultiplier();
            Assert.AreEqual(2f, Time.timeScale);

            context.Manager.IncreaseTimeMultiplier();
            Assert.AreEqual(4f, Time.timeScale);

            context.Manager.IncreaseTimeMultiplier();
            Assert.AreEqual(4f, Time.timeScale);
        }

        [Test]
        public void DecreaseTimeMultiplier_DividesTimeScale_CapsAtQuarter()
        {
            UIContext context = CreateContext();
            Time.timeScale = 1f;

            context.Manager.DecreaseTimeMultiplier();
            Assert.AreEqual(0.5f, Time.timeScale);

            context.Manager.DecreaseTimeMultiplier();
            Assert.AreEqual(0.25f, Time.timeScale);

            context.Manager.DecreaseTimeMultiplier();
            Assert.AreEqual(0.25f, Time.timeScale);
        }

        [Test]
        public void TogglePause_InvertsGameStateAndTimeScale()
        {
            UIContext context = CreateContext();
            context.Data.IsPaused = false;
            Time.timeScale = 1f;

            context.Manager.TogglePause();

            Assert.IsTrue(context.Data.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);

            context.Manager.TogglePause();

            Assert.IsFalse(context.Data.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        private UIContext CreateContext()
        {
            GameObject managerObj = Track(new GameObject("UIManager"));
            UIManager manager = managerObj.AddComponent<UIManager>();

            GameObject dataObj = Track(new GameObject("GameData"));
            GameData data = dataObj.AddComponent<GameData>();

            GameObject menuObj = Track(new GameObject("EscapeMenu"));
            
            UIContext context = new UIContext
            {
                Manager = manager,
                Data = data,
                EscapeMenu = menuObj,
                MoneyText = Track(new GameObject("MoneyText")).AddComponent<TextMeshProUGUI>(),
                CityText = Track(new GameObject("CityText")).AddComponent<TextMeshProUGUI>(),
                DateText = Track(new GameObject("DateText")).AddComponent<TextMeshProUGUI>()
            };

            SetField(manager, "gameData", context.Data);
            SetField(manager, "escapeMenu", context.EscapeMenu);
            SetField(manager, "moneyText", context.MoneyText);
            SetField(manager, "cityText", context.CityText);
            SetField(manager, "dateText", context.DateText);

            return context;
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

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = FindField(target.GetType(), name);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static FieldInfo FindField(Type type, string name)
        {
            FieldInfo f = type.GetField(name, InstanceFlags);
            return f ?? (type.BaseType != null ? FindField(type.BaseType, name) : null);
        }

        private class UIContext
        {
            public UIManager Manager;
            public GameData Data;
            public GameObject EscapeMenu;
            public TextMeshProUGUI MoneyText;
            public TextMeshProUGUI CityText;
            public TextMeshProUGUI DateText;
        }
    }
}