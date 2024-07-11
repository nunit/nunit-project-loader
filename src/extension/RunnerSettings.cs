// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

namespace NUnit.Engine
{
    /// <summary>
    /// RunnerSettings is a static class containing constant values that
    /// are used as keys in the TestPackage created by the project loader.
    /// </summary>
    /// <remarks>
    /// This class is used only in the NUnitProjectLoader extension and
    /// represents knowledge the extension has of the internals of the
    /// engine - that is, the meaning of various settings. Only setting
    /// keys actually used by the loader are included.
    /// </remarks>
    public static class RunnerSettings
    {
        /// <summary>
        /// If true, the engine should determine the private bin
        /// path by examining the paths to all the tests.
        /// </summary>
        public const string AutoBinPath = "AutoBinPath";

        /// <summary>
        /// The ApplicationBase to use in loading the tests.
        /// </summary>
        public const string BasePath = "BasePath";

        /// <summary>
        /// The config file to use in running the tests 
        /// </summary>
        public const string ConfigurationFile = "ConfigurationFile";

        /// <summary>
        /// Indicates how to load tests across AppDomains
        /// </summary>
        public const string DomainUsage = "DomainUsage";

        /// <summary>
        /// The private binpath used to locate assemblies
        /// </summary>
        public const string PrivateBinPath = "PrivateBinPath";

        /// <summary>
        /// Indicates how to allocate assemblies to processes
        /// </summary>
        public const string ProcessModel = "ProcessModel";

        /// <summary>
        /// Indicates the desired runtime to use for the tests.
        /// </summary>
        public const string RuntimeFramework = "RuntimeFramework";
    }
}
