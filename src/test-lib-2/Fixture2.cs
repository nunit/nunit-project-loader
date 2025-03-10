// Copyright (c) Charlie Poole and Contributors. MIT License - see LICENSE.txt

using NUnit.Framework;

namespace test_lib_2
{
    public class Fixture2
    {
        [Test]
        public void TestMethod1() { }

        [Test]
        public void TestMethod2() { }

        [Test]
        public void TestMethod3() { }

        [Test]
        public void TestMethod4() { }

        [Test]
        public void TestMethod5()
        {
            Assert.Fail();
        }

        [Test]
        public void TestMethod6()
        {
            Assert.Fail();
        }

        [Test]
        public void TestMethod7()
        {
            Assert.Inconclusive();
        }

        [Test, Ignore("Because")]
        public void TestMethod8() { }

        [Test, Ignore("Reason")]
        public void TestMethod9() { }
    }
}
