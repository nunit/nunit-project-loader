// Copyright (c) Charlie Poole and Contributors. MIT License - see LICENSE.txt

using NUnitLite;

namespace NUnit.Engine.Tests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return new AutoRun().Execute(args);
        }
    }
}
