using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace KnockerTest
{
    [TestFixture]
    public class DestinationPointTests
    {
        [Test]
        public void TestMethod_DestinationPointAddressCannotBeNull()
        {
            Assert.Catch(delegate
            {
                KnockerLib.DestinationPoint dp = new KnockerLib.DestinationPoint(null);
            });
        }
        
        [Test]
        public void TestMethod_DestinationPointMustHaveValidUri()
        {
            Uri vaild_uri = new Uri("http://localhost");

            Assert.DoesNotThrow(delegate
            {
                KnockerLib.DestinationPoint dp = new KnockerLib.DestinationPoint(vaild_uri);
            });
        }

        [Test]
        public void TestMethod_DestinationPointUnreachableWithIncorrectUrlAddress()
        {
            Uri vaild_uri = new Uri("http://holocaust");
            KnockerLib.DestinationPoint dp = new KnockerLib.DestinationPoint(vaild_uri);

            /// act
            KnockerLib.PointTester.PointPingCheckAction(dp);

            /// assert
            Assert.AreEqual(dp.State, KnockerLib.PointState.Unknown);
        }

        [Test]
        public void TestMethod_LocalDestinationPointPingPassedSuccessfully()
        {
            /// arrange
            Uri vaild_uri = new Uri("http://localhost");
            KnockerLib.DestinationPoint dp = new KnockerLib.DestinationPoint(vaild_uri);

            /// act
            KnockerLib.PointTester.PointPingCheckAction(dp);

            /// assert
            Assert.AreEqual(dp.State, KnockerLib.PointState.Open);
        }

        [Test]
        public void TestMethod_LocalDestinationPointTracePassedSuccessfully()
        {
            /// arrange
            Uri vaild_uri = new Uri("http://localhost");
            KnockerLib.DestinationPoint dp = new KnockerLib.DestinationPoint(vaild_uri);

            Assert.DoesNotThrow(delegate { KnockerLib.PointTester.PointTraceCheckAction(dp); });
        }

        [Test(TestOf = typeof(KnockerLib.DestinationPoint))]
        public void TestMethod_LocalDestinationPointDefaultPortClosed()
        {
            /// arrange
            Uri vaild_uri = new Uri("http://127.0.0.1");
            KnockerLib.DestinationPoint dp = new KnockerLib.DestinationPoint(vaild_uri);

            /// act
            KnockerLib.PointTester.PointPortCheckAction(dp);

            /// assert
            Assert.AreEqual(dp.State, KnockerLib.PointState.Closed);
        }
    }
}
