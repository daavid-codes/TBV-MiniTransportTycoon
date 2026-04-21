using NUnit.Framework;
using System;
using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class CarTypeTests
    {
        [Test]
        public void CarType_Enum_ContainsAllRequiredValues()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(CarType), CarType.Bus));
            Assert.IsTrue(Enum.IsDefined(typeof(CarType), CarType.Taxi));
            Assert.IsTrue(Enum.IsDefined(typeof(CarType), CarType.Truck));
            Assert.IsTrue(Enum.IsDefined(typeof(CarType), CarType.Car));
        }

        [Test]
        public void CarType_Enum_CountIsCorrect()
        {
            int enumCount = Enum.GetNames(typeof(CarType)).Length;
            Assert.AreEqual(4, enumCount);
        }

        [TestCase(CarType.Bus, 0)]
        [TestCase(CarType.Taxi, 1)]
        [TestCase(CarType.Truck, 2)]
        [TestCase(CarType.Car, 3)]
        public void CarType_Enum_ValuesAreInExpectedOrder(CarType type, int expectedValue)
        {
            Assert.AreEqual(expectedValue, (int)type);
        }
    }
}