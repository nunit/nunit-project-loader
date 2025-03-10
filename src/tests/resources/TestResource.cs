// Copyright (c) Charlie Poole and Contributors. MIT License - see LICENSE.txt

using System;

namespace NUnit.Engine.Tests.resources
{
    public class TestResource : TempResourceFile
    {
        public TestResource(string name)
            : base(typeof(TestResource), name)
        {
        }

        public TestResource(string name, string filePath)
            : base(typeof(TestResource), name, filePath)
        {
        }
    }
}
