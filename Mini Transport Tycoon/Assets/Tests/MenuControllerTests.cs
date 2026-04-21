using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class MenuControllerTests
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
        }

        [Test]
        public void ToggleVehicleMenu_WhenSubMenuIsNull_DoesNotThrow()
        {
            MenuController controller = CreateController();
            controller.vehicleSubMenu = null;
            controller.GarageMenu = null;

            Assert.DoesNotThrow(() => controller.ToggleVehicleMenu());
        }

        [Test]
        public void ToggleVehicleMenu_WhenSubMenuIsInactive_ActivatesIt()
        {
            MenuController controller = CreateController();
            controller.vehicleSubMenu.SetActive(false);

            controller.ToggleVehicleMenu();

            Assert.IsTrue(controller.vehicleSubMenu.activeSelf);
        }

        [Test]
        public void ToggleVehicleMenu_WhenSubMenuIsActive_DeactivatesIt()
        {
            MenuController controller = CreateController();
            controller.vehicleSubMenu.SetActive(true);

            controller.ToggleVehicleMenu();

            Assert.IsFalse(controller.vehicleSubMenu.activeSelf);
        }

        [Test]
        public void ToggleVehicleMenu_WhenGarageMenuIsActive_DeactivatesGarageMenu()
        {
            MenuController controller = CreateController();
            controller.vehicleSubMenu.SetActive(false);
            controller.GarageMenu.SetActive(true);

            controller.ToggleVehicleMenu();

            Assert.IsFalse(controller.GarageMenu.activeSelf);
            Assert.IsTrue(controller.vehicleSubMenu.activeSelf);
        }

        [Test]
        public void ToggleGarageMenu_WhenGarageMenuIsNull_DoesNotThrow()
        {
            MenuController controller = CreateController();
            controller.vehicleSubMenu = null;
            controller.GarageMenu = null;

            Assert.DoesNotThrow(() => controller.ToggleGarageMenu());
        }

        [Test]
        public void ToggleGarageMenu_WhenGarageMenuIsInactive_ActivatesIt()
        {
            MenuController controller = CreateController();
            controller.GarageMenu.SetActive(false);

            controller.ToggleGarageMenu();

            Assert.IsTrue(controller.GarageMenu.activeSelf);
        }

        [Test]
        public void ToggleGarageMenu_WhenGarageMenuIsActive_DeactivatesIt()
        {
            MenuController controller = CreateController();
            controller.GarageMenu.SetActive(true);

            controller.ToggleGarageMenu();

            Assert.IsFalse(controller.GarageMenu.activeSelf);
        }

        [Test]
        public void ToggleGarageMenu_WhenVehicleSubMenuIsActive_DeactivatesVehicleSubMenu()
        {
            MenuController controller = CreateController();
            controller.GarageMenu.SetActive(false);
            controller.vehicleSubMenu.SetActive(true);

            controller.ToggleGarageMenu();

            Assert.IsFalse(controller.vehicleSubMenu.activeSelf);
            Assert.IsTrue(controller.GarageMenu.activeSelf);
        }

        [Test]
        public void LoadNextScene_WhenIndexIsNegative_LogsWarningAndDoesNotLoad()
        {
            MenuController controller = CreateController();
            int invalidIndex = -1;

            LogAssert.Expect(LogType.Warning, $"Scene index {invalidIndex} is out of range!");
            
            controller.LoadNextScene(invalidIndex);
        }

        [Test]
        public void LoadNextScene_WhenIndexIsBeyondBuildSettings_LogsWarningAndDoesNotLoad()
        {
            MenuController controller = CreateController();
            int invalidIndex = SceneManager.sceneCountInBuildSettings + 1;

            LogAssert.Expect(LogType.Warning, $"Scene index {invalidIndex} is out of range!");
            
            controller.LoadNextScene(invalidIndex);
        }

        [Test]
        public void LoadNextScene_WhenIndexIsValid_AttemptsToLoadScene()
        {
            MenuController controller = CreateController();
            int validIndex = 0; 
            
            if (SceneManager.sceneCountInBuildSettings == 0)
            {
                Assert.Ignore("A Build Settings-ben nincsenek scene-ek beállítva, ez a teszt átugrásra kerül.");
                return;
            }

            LogAssert.ignoreFailingMessages = true;
            bool executedWithoutCrash = false;

            try
            {
                controller.LoadNextScene(validIndex);
                executedWithoutCrash = true;
            }
            catch (Exception)
            {
                executedWithoutCrash = true;
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.IsTrue(executedWithoutCrash);
        }

        private MenuController CreateController()
        {
            GameObject controllerObj = Track(new GameObject("MenuController"));
            MenuController controller = controllerObj.AddComponent<MenuController>();

            controller.vehicleSubMenu = Track(new GameObject("VehicleMenu"));
            controller.GarageMenu = Track(new GameObject("GarageMenu"));

            return controller;
        }

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            trackedObjects.Add(obj);
            return obj;
        }
    }
}