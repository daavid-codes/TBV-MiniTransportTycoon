using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using MiniTransportTycoon;

namespace MiniTransportTycoon.Tests
{
    public class FacilityTests
    {
        // Egy teszteléshez használt konkrét implementációja a Facility-nek
        private class TestFacility : Facility
        {
            public override void produce(GameData game)
            {
                ProduceOwnMaterial(game);
            }

            public void SetTypes(Materials resource, Materials produced)
            {
                SetResourceType(resource);
                SetProducedMaterialType(produced);
            }

            public void SetReqs(params Materials[] requiredMaterials)
            {
                SetInputRequirements(requiredMaterials);
            }
            
            public int GetProducedAmountPublic()
            {
                return GetProducedAmount();
            }
            
            public void UpdateProductivityPublic()
            {
                UpdateProductivity();
            }
        }

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

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            trackedObjects.Add(obj);
            return obj;
        }

        [Test]
        public void Initialize_SetsIdAndResetsStoredAmount()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            facility.Initialize(42);

            Assert.AreEqual(42, facility.Id);
            Assert.AreEqual(0, facility.StoredProducedAmount);
        }

        [Test]
        public void AddInputMaterial_IncreasesAmount()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            facility.AddInputMaterial(Materials.Wood, 100);

            Assert.AreEqual(100, facility.GetInputMaterialAmount(Materials.Wood));
        }

        [Test]
        public void AddInputMaterial_WithNegativeAmount_DoesNothing()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            facility.AddInputMaterial(Materials.Wood, -50);

            Assert.AreEqual(0, facility.GetInputMaterialAmount(Materials.Wood));
        }
        
        [Test]
        public void AddInputMaterial_ExistingMaterial_AddsToExistingAmount()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            facility.AddInputMaterial(Materials.Coal, 50);
            facility.AddInputMaterial(Materials.Coal, 30);

            Assert.AreEqual(80, facility.GetInputMaterialAmount(Materials.Coal));
        }

        [Test]
        public void TakeProducedMaterial_ReducesStoredAmount()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            SetField(facility, "storedProducedAmount", 500);

            int taken = facility.TakeProducedMaterial(200);

            Assert.AreEqual(200, taken);
            Assert.AreEqual(300, facility.StoredProducedAmount);
        }

        [Test]
        public void TakeProducedMaterial_MoreThanStored_ReturnsStoredAmount()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            SetField(facility, "storedProducedAmount", 150);

            int taken = facility.TakeProducedMaterial(500);

            Assert.AreEqual(150, taken);
            Assert.AreEqual(0, facility.StoredProducedAmount);
        }
        
        [Test]
        public void TakeProducedMaterial_NegativeAmount_ReturnsZero()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            SetField(facility, "storedProducedAmount", 150);

            int taken = facility.TakeProducedMaterial(-50);

            Assert.AreEqual(0, taken);
            Assert.AreEqual(150, facility.StoredProducedAmount);
        }

        [Test]
        public void RequiresInputMaterial_ReturnsTrueIfRequired()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            facility.SetReqs(Materials.Wood, Materials.Coal);

            Assert.IsTrue(facility.RequiresInputMaterial(Materials.Wood));
            Assert.IsTrue(facility.RequiresInputMaterial(Materials.Coal));
            Assert.IsFalse(facility.RequiresInputMaterial(Materials.Iron));
        }

        [Test]
        public void ProduceOwnMaterial_WithoutInputs_ConsumesResourceAndIncreasesStored()
        {
            var facility = Track(new GameObject("Facility")).AddComponent<TestFacility>();
            facility.SetTypes(Materials.Iron, Materials.Steel);
            SetField(facility, "resourceAmount", 5000);

            facility.produce(null); 

            Assert.IsTrue(facility.StoredProducedAmount > 0);
            Assert.IsTrue(facility.RemainingResourceAmount < 5000);
        }
        

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null && target.GetType().BaseType != null)
                field = target.GetType().BaseType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            field?.SetValue(target, value);
        }
        
        private static object GetField(object target, string name)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null && target.GetType().BaseType != null)
                field = target.GetType().BaseType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            return field?.GetValue(target);
        }
    }
}