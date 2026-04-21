using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TMPro;
using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class TopBarManagerTests
    {
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
        }

        [Test]
        public void UpdateTopBar_SetsCityNameCorrectly()
        {
            UIContext context = CreateContext();

            context.Manager.UpdateTopBar("Budapest", 1000, "2026. 01. 01.");

            Assert.AreEqual("Budapest", context.CityNameText.text);
        }

        [Test]
        public void UpdateTopBar_FormatsMoneyCorrectly()
        {
            UIContext context = CreateContext();
            int testMoney = 1234567;
            string expectedMoneyString = $"${testMoney:N0}";

            context.Manager.UpdateTopBar("TestCity", testMoney, "2026. 01. 01.");

            Assert.AreEqual(expectedMoneyString, context.MoneyText.text);
        }

        [Test]
        public void UpdateTopBar_SetsDateCorrectly()
        {
            UIContext context = CreateContext();
            string expectedDate = "2026. 04. 21. 20:00";

            context.Manager.UpdateTopBar("TestCity", 0, expectedDate);

            Assert.AreEqual(expectedDate, context.DateText.text);
        }

        [Test]
        public void UpdateTopBar_UpdatesAllFieldsSimultaneously()
        {
            UIContext context = CreateContext();
            string expectedCity = "Kaposvár";
            int testMoney = 4242;
            string expectedDate = "2026. 01. 01. 08:00";
            string expectedMoneyString = $"${testMoney:N0}";

            context.Manager.UpdateTopBar(expectedCity, testMoney, expectedDate);

            Assert.AreEqual(expectedCity, context.CityNameText.text);
            Assert.AreEqual(expectedMoneyString, context.MoneyText.text);
            Assert.AreEqual(expectedDate, context.DateText.text);
        }

        private UIContext CreateContext()
        {
            GameObject managerObj = Track(new GameObject("TopBarManager"));
            TopBarManager manager = managerObj.AddComponent<TopBarManager>();

            UIContext context = new UIContext
            {
                Manager = manager,
                CityNameText = Track(new GameObject("CityNameText")).AddComponent<TextMeshProUGUI>(),
                MoneyText = Track(new GameObject("MoneyText")).AddComponent<TextMeshProUGUI>(),
                DateText = Track(new GameObject("DateText")).AddComponent<TextMeshProUGUI>()
            };

            manager.cityNameText = context.CityNameText;
            manager.moneyText = context.MoneyText;
            manager.dateText = context.DateText;

            return context;
        }

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            trackedObjects.Add(obj);
            return obj;
        }

        private class UIContext
        {
            public TopBarManager Manager;
            public TextMeshProUGUI CityNameText;
            public TextMeshProUGUI MoneyText;
            public TextMeshProUGUI DateText;
        }
    }
}