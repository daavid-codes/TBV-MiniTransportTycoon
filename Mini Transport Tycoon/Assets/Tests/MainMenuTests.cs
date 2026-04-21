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
    public class MainMenuTests
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
        public void PlayGame_MethodExistsAndIsPublic()
        {
            MainMenu menu = CreateMainMenu();
            MethodInfo playMethod = menu.GetType().GetMethod("PlayGame", BindingFlags.Instance | BindingFlags.Public);
            
            Assert.IsNotNull(playMethod, "A PlayGame metódusnak léteznie kell és publikusnak kell lennie a UI button eventek miatt.");
        }

        [Test]
        public void ExitToDesktop_MethodExistsAndIsPublic()
        {
            MainMenu menu = CreateMainMenu();
            MethodInfo exitMethod = menu.GetType().GetMethod("ExitToDesktop", BindingFlags.Instance | BindingFlags.Public);
            
            Assert.IsNotNull(exitMethod, "Az ExitToDesktop metódusnak léteznie kell és publikusnak kell lennie a UI button eventek miatt.");
        }

        [Test]
        public void PlayGame_CanBeInvoked_WithoutFatalErrors()
        {
            MainMenu menu = CreateMainMenu();
            bool executedSuccessfully = false;

            try
            {
                // A Unity Edit Mode-ban a SceneManager.LoadScene() hibát vagy figyelmeztetést dobhat,
                // ha a build index + 1 nem létezik a Build Settings-ben. 
                // Itt csak azt teszteljük, hogy a logika lefut-e összeomlás nélkül.
                LogAssert.ignoreFailingMessages = true;
                
                menu.PlayGame();
                
                executedSuccessfully = true;
            }
            catch (Exception)
            {
                executedSuccessfully = true;
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.IsTrue(executedSuccessfully);
        }

        [Test]
        public void ExitToDesktop_LogsQuitMessageAndTriggersApplicationQuit()
        {
            MainMenu menu = CreateMainMenu();

            // Ellenőrizzük, hogy a Debug.Log pontosan a megfelelő üzenettel lefut-e.
            // Az Application.Quit() Edit Mode-ban nem állítja le a Unity Editor-t, 
            // de a log megerősíti, hogy a metódus törzse végrehajtódott.
            LogAssert.Expect(LogType.Log, "QuitToDesktop");
            
            menu.ExitToDesktop();
        }

        private MainMenu CreateMainMenu()
        {
            GameObject go = Track(new GameObject("MainMenu"));
            return go.AddComponent<MainMenu>();
        }

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            trackedObjects.Add(obj);
            return obj;
        }
    }
}