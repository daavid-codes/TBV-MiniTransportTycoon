using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class CarTests
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly List<UnityEngine.Object> trackedObjects = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDownTrackedObjects()
        {
            for (int i = trackedObjects.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object trackedObject = trackedObjects[i];
                if (trackedObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(trackedObject);
                }
            }

            trackedObjects.Clear();
        }

        [Test]
        public void Awake_SetsCarTypeToCar()
        {
            Car car = CreateCar();
            Invoke(car, "Awake");

            Assert.AreEqual(CarType.Car, GetField<CarType>(car, "type"));
        }

        [Test]
        public void Reset_SetsCarTypeToCar()
        {
            Car car = CreateCar();
            SetField(car, "type", CarType.Bus); 
            
            Invoke(car, "Reset");

            Assert.AreEqual(CarType.Car, GetField<CarType>(car, "type"));
        }

        [Test]
        public void OnValidate_SetsCarTypeToCar()
        {
            Car car = CreateCar();
            SetField(car, "type", CarType.Bus); 

            Invoke(car, "OnValidate");

            Assert.AreEqual(CarType.Car, GetField<CarType>(car, "type"));
        }

        [Test]
        public void SetRoute_ClearsShuttleRoutesAndFlags()
        {
            Car car = CreateCar();
            List<Vector3Int> newRoute = new List<Vector3Int> { Vector3Int.zero };
            
            car.SetRoute(newRoute);

            Assert.IsFalse(GetField<bool>(car, "useShuttleRoute"));
            Assert.AreEqual(0, GetField<List<Vector3Int>>(car, "shuttleRouteForward").Count);
            Assert.AreEqual(0, GetField<List<Vector3Int>>(car, "shuttleRouteBackward").Count);
        }

        [Test]
        public void SetShuttleRoute_WithInvalidPath_DisablesShuttleRoute()
        {
            Car car = CreateCar();
            List<Vector3Int> path = new List<Vector3Int>();
            
            car.SetShuttleRoute(path);

            Assert.IsFalse(GetField<bool>(car, "useShuttleRoute"));
        }

        [Test]
        public void SetShuttleRoute_WithValidPath_SetsLegsCorrectly()
        {
            Car car = CreateCar();
            List<Vector3Int> path = new List<Vector3Int> { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(2, 0, 0) };
            
            car.SetShuttleRoute(path);

            Assert.IsTrue(GetField<bool>(car, "useShuttleRoute"));
            
            List<Vector3Int> forwardRoute = GetField<List<Vector3Int>>(car, "shuttleRouteForward");
            List<Vector3Int> backwardRoute = GetField<List<Vector3Int>>(car, "shuttleRouteBackward");

            Assert.AreEqual(2, forwardRoute.Count);
            Assert.AreEqual(2, backwardRoute.Count);
            Assert.AreEqual(new Vector3Int(1, 0, 0), forwardRoute[0]);
            Assert.AreEqual(new Vector3Int(1, 0, 0), backwardRoute[0]);
        }

        [Test]
        public void BuildShuttleLeg_RemovesFirstElementAndReversesCorrectly()
        {
            Car car = CreateCar();
            List<Vector3Int> path = new List<Vector3Int> { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(2, 0, 0) };

            List<Vector3Int> forwardResult = Invoke<List<Vector3Int>>(car, "BuildShuttleLeg", path, false);
            Assert.AreEqual(2, forwardResult.Count);
            Assert.AreEqual(new Vector3Int(1, 0, 0), forwardResult[0]);
            Assert.AreEqual(new Vector3Int(2, 0, 0), forwardResult[1]);

            List<Vector3Int> backwardResult = Invoke<List<Vector3Int>>(car, "BuildShuttleLeg", path, true);
            Assert.AreEqual(2, backwardResult.Count);
            Assert.AreEqual(new Vector3Int(1, 0, 0), backwardResult[0]);
            Assert.AreEqual(new Vector3Int(0, 0, 0), backwardResult[1]);
        }

        [Test]
        public void Update_WhenShuttleEnabledAndNotMoving_StartsNextLeg()
        {
            Car car = CreateCar();
            List<Vector3Int> path = new List<Vector3Int> { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(2, 0, 0) };
            car.SetShuttleRoute(path);

            SetPropertyOrField(car, "IsMoving", false);
            SetPropertyOrField(car, "Route", new List<Vector3Int> { new Vector3Int(0, 0, 0) });

            bool initialForwardState = GetField<bool>(car, "nextShuttleLegIsForward");

            Invoke(car, "Update");

            bool newForwardState = GetField<bool>(car, "nextShuttleLegIsForward");
            Assert.AreNotEqual(initialForwardState, newForwardState);
            Assert.AreEqual(2, GetPropertyOrField<List<Vector3Int>>(car, "Route").Count);
        }

        private Car CreateCar()
        {
            GameObject carObject = Track(new GameObject("Car"));
            return carObject.AddComponent<Car>();
        }

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            if (obj != null)
            {
                trackedObjects.Add(obj);
            }
            return obj;
        }

        private static T GetField<T>(object target, string fieldName)
        {
            return (T)FindField(target.GetType(), fieldName).GetValue(target);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FindField(target.GetType(), fieldName).SetValue(target, value);
        }
        
        private static T GetPropertyOrField<T>(object target, string name)
        {
            Type type = target.GetType();
            PropertyInfo prop = type.GetProperty(name, InstanceFlags);
            if (prop != null)
            {
                return (T)prop.GetValue(target);
            }
            FieldInfo field = FindField(type, name);
            return (T)field.GetValue(target);
        }

        private static void SetPropertyOrField(object target, string name, object value)
        {
            Type type = target.GetType();
            PropertyInfo prop = type.GetProperty(name, InstanceFlags);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value);
                return;
            }
            FieldInfo field = FindField(type, name);
            field.SetValue(target, value);
        }

        private static T Invoke<T>(object target, string methodName, params object[] args)
        {
            return (T)Invoke(target, methodName, args);
        }

        private static object Invoke(object target, string methodName, params object[] args)
        {
            MethodInfo method = FindMethod(target.GetType(), methodName, args ?? Array.Empty<object>());
            return method.Invoke(target, args);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            Type currentType = type;
            while (currentType != null)
            {
                FieldInfo field = currentType.GetField(fieldName, InstanceFlags);
                if (field != null)
                {
                    return field;
                }
                currentType = currentType.BaseType;
            }
            throw new MissingFieldException(type.FullName, fieldName);
        }

        private static MethodInfo FindMethod(Type type, string methodName, object[] args)
        {
            Type currentType = type;
            while (currentType != null)
            {
                MethodInfo[] methods = currentType.GetMethods(InstanceFlags);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo method = methods[i];
                    if (method.Name != methodName)
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length != args.Length)
                    {
                        continue;
                    }

                    bool matches = true;
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        object arg = args[j];
                        Type parameterType = parameters[j].ParameterType;

                        if (arg == null)
                        {
                            if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                            {
                                matches = false;
                                break;
                            }
                            continue;
                        }

                        if (!parameterType.IsInstanceOfType(arg))
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                    {
                        return method;
                    }
                }
                currentType = currentType.BaseType;
            }
            throw new MissingMethodException(type.FullName, methodName);
        }
    }
}