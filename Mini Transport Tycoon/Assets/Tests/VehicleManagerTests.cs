using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class VehicleManagerTests
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
            
            // Reset singletons
            var gameDataField = typeof(GameData).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            if (gameDataField != null)
                gameDataField.SetValue(null, null);
                
            var vmField = typeof(VehicleManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            if (vmField != null)
                vmField.SetValue(null, null);
        }

        [Test]
        public void Awake_SetsInstance()
        {
            VehicleManager vm = CreateVehicleManager();
            Invoke(vm, "Awake");
            
            Assert.AreEqual(vm, VehicleManager.Instance);
        }

        [Test]
        public void RegisterVehicle_AddsToListAndTriggersEvent()
        {
            VehicleManager vm = CreateVehicleManager();
            Bus mockVehicle = CreateMockVehicle(false);
            
            bool eventTriggered = false;
            vm.OnVehiclesChanged += () => eventTriggered = true;

            int id = vm.RegisterVehicle(mockVehicle);

            Assert.AreEqual(1, id);
            Assert.AreEqual(1, vm.GetAllVehicles().Count);
            Assert.AreEqual(mockVehicle, vm.GetAllVehicles()[0]);
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void UnregisterVehicle_RemovesFromListAndTriggersEvent()
        {
            VehicleManager vm = CreateVehicleManager();
            Bus mockVehicle = CreateMockVehicle(false);
            vm.RegisterVehicle(mockVehicle);
            
            bool eventTriggered = false;
            vm.OnVehiclesChanged += () => eventTriggered = true;

            vm.UnregisterVehicle(mockVehicle);

            Assert.AreEqual(0, vm.GetAllVehicles().Count);
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void RemoveVehicleById_RemovesCorrectVehicleAndTriggersEvent()
        {
            VehicleManager vm = CreateVehicleManager();
            Bus mockVehicle1 = CreateMockVehicle(false);
            Bus mockVehicle2 = CreateMockVehicle(false);
            
            int id1 = vm.RegisterVehicle(mockVehicle1);
            vm.RegisterVehicle(mockVehicle2);
            
            SetField(mockVehicle1, "id", id1); // Simulate ID setting

            bool eventTriggered = false;
            vm.OnVehiclesChanged += () => eventTriggered = true;

            vm.RemoveVehicleById(id1);

            Assert.AreEqual(1, vm.GetAllVehicles().Count);
            Assert.AreEqual(mockVehicle2, vm.GetAllVehicles()[0]);
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void RepairVehicle_ResetsDurabilityAndCostAndTriggersEvent()
        {
            VehicleManager vm = CreateVehicleManager();
            Bus mockVehicle = CreateMockVehicle(false);
            mockVehicle.SetDurability(50);
            mockVehicle.SetMaintenanceCost(500);
            
            bool eventTriggered = false;
            vm.OnVehiclesChanged += () => eventTriggered = true;

            vm.RepairVehicle(mockVehicle);

            Assert.AreEqual(100, mockVehicle.GetDurability());
            Assert.AreEqual(0, mockVehicle.MaintenanceCost);
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void DecraseVehicleDurability_DecreasesDurabilityUpdatesCostAndTriggersEvent()
        {
            VehicleManager vm = CreateVehicleManager();
            Bus mockVehicle = CreateMockVehicle(false);
            mockVehicle.SetDurability(100);
            vm.RegisterVehicle(mockVehicle);
            
            bool eventTriggered = false;
            vm.OnVehiclesChanged += () => eventTriggered = true;

            Invoke(vm, "DecraseVehicleDurability");

            Assert.AreEqual(99, mockVehicle.GetDurability());
            // (100 - 99) * 10 = 10
            Assert.AreEqual(10, mockVehicle.MaintenanceCost);
            Assert.IsTrue(eventTriggered);
        }

        private VehicleManager CreateVehicleManager()
        {
            GameObject go = Track(new GameObject("VehicleManager"));
            return go.AddComponent<VehicleManager>();
        }

        private Bus CreateMockVehicle(bool hasGarage)
        {
            GameObject go = Track(new GameObject("MockVehicle"));
            Bus bus = go.AddComponent<Bus>();
            bus.SetDurability(100);

            if (hasGarage)
            {
                GameObject tmGo = Track(new GameObject("GarageTM"));
                tmGo.AddComponent<Grid>();
                Tilemap tm = tmGo.AddComponent<Tilemap>();
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tm.SetTile(Vector3Int.zero, tile);

                bus.SetGarageTilemap(tm);
                bus.SetStopRoute(new List<Vector3Int> { Vector3Int.zero });
            }

            return bus;
        }

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            trackedObjects.Add(obj);
            return obj;
        }

        private static void Invoke(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, InstanceFlags);
            method?.Invoke(target, args);
        }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, InstanceFlags);
            field?.SetValue(target, value);
        }
    }
}
