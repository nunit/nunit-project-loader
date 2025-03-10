// Copyright (c) Charlie Poole and Contributors. MIT License - see LICENSE.txt

using System.IO;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Tests.resources;
using NUnit.Extensibility;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    public class NUnitProjectLoaderTests
    {
        NUnitProjectLoader _loader;

        [SetUp]
        public void CreateLoader()
        {
            _loader = new NUnitProjectLoader();
        }

        [Test]
        public void CheckExtensionAttribute()
        {
            Assert.That(typeof(NUnitProjectLoader),
                Has.Attribute<ExtensionAttribute>()
                    .With.Property("EngineVersion").EqualTo("4.0"));
        }

        [Test]
        public void CheckExtensionPropertyAttribute()
        {
            Assert.That(typeof(NUnitProjectLoader),
                Has.Attribute<ExtensionPropertyAttribute>()
                    .With.Property("Name").EqualTo("FileExtension")
                    .And.Property("Value").EqualTo(".nunit"));
        }

        [TestCase("dummy.nunit", ExpectedResult = true)]
        [TestCase("dummy.dll", ExpectedResult = false)]
        [TestCase("dummy.junk", ExpectedResult = false)]
        public bool CheckExtension(string fileName)
        {
            return _loader.CanLoadFrom(fileName);
        }

        [Test]
        public void CanLoadEmptyProject()
        {
            using (TestResource file = new TestResource("NUnitProject_EmptyProject.nunit"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.That(project.ProjectPath, Is.EqualTo(file.Path));
                Assert.That(project.ConfigNames.Count, Is.EqualTo(0));
                Assert.That(project.ActiveConfigName, Is.Null);
            }
        }

        [Test]
        public void LoadEmptyConfigs()
        {
            using (TestResource file = new TestResource("NUnitProject_EmptyConfigs.nunit"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                var projectDir = Path.GetDirectoryName(file.Path);

                Assert.That(project.ConfigNames.Count, Is.EqualTo(2));
                Assert.That(project.ActiveConfigName, Is.EqualTo("Debug"));
                Assert.That(project.ConfigNames, Is.EqualTo(new string[] { "Debug", "Release" }));

                TestPackage package1 = project.GetTestPackage("Debug");
                ClassicAssert.AreEqual(projectDir, package1.Settings["BasePath"], "BasePath");

                TestPackage package2 = project.GetTestPackage("Release");
                ClassicAssert.AreEqual(true, package2.Settings["AutoBinPath"], "AutoBinPath");
                ClassicAssert.AreEqual(projectDir, package1.Settings["BasePath"], "BasePath");
            }
        }

        [TestCase("NUnitProject.nunit")]
        [TestCase("NUnitProject_XmlDecl.nunit")]
        public void LoadNormalProject(string resourceName)
        {
            using (TestResource file = new TestResource(resourceName))
            {
                IProject _project = _loader.LoadFrom(file.Path);

                var projectDir = Path.GetDirectoryName(file.Path);
                var binDir = Path.Combine(projectDir, "bin");
                var debugDir = Path.Combine(binDir, "debug");
                var releaseDir = Path.Combine(binDir, "release");

                ClassicAssert.AreEqual(2, _project.ConfigNames.Count);
                ClassicAssert.AreEqual("Debug", _project.ActiveConfigName);
                ClassicAssert.AreEqual(new string[] { "Debug", "Release" }, _project.ConfigNames);

                TestPackage package1 = _project.GetTestPackage("Debug");
                ClassicAssert.AreEqual(2, package1.SubPackages.Count);
                ClassicAssert.AreEqual(
                    Path.Combine(debugDir, "assembly1.dll"),
                    package1.SubPackages[0].FullName);
                ClassicAssert.AreEqual(
                    Path.Combine(debugDir, "assembly2.dll"),
                    package1.SubPackages[1].FullName);

                ClassicAssert.AreEqual(debugDir, package1.Settings["BasePath"], "BasePath");
                ClassicAssert.AreEqual(true, package1.Settings["AutoBinPath"], "AutoBinPath");

                TestPackage package2 = _project.GetTestPackage("Release");
                ClassicAssert.AreEqual(
                    Path.Combine(releaseDir, "assembly1.dll"),
                    package2.SubPackages[0].FullName);
                ClassicAssert.AreEqual(
                    Path.Combine(releaseDir, "assembly2.dll"),
                    package2.SubPackages[1].FullName);

                ClassicAssert.AreEqual(releaseDir, package2.Settings["BasePath"]);
                ClassicAssert.AreEqual(true, package2.Settings["AutoBinPath"]);

                Assert.Throws<NUnitEngineException>(() => _project.GetTestPackage("MissingConfiguration"), 
                "Project loader should throw exception when attempting to use missing configuration");
            }
        }

        [Test]
        public void LoadProjectWithManualBinPath()
        {
            using (TestResource file = new TestResource("NUnitProject_ManualBinPath.nunit"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                var projectDir = Path.GetDirectoryName(file.Path);

                ClassicAssert.AreEqual(1, project.ConfigNames.Count);

                TestPackage package1 = project.GetTestPackage("Debug");
                ClassicAssert.AreEqual("bin_path_value", package1.Settings["PrivateBinPath"], "PrivateBinPath");
                ClassicAssert.AreEqual(projectDir, package1.Settings["BasePath"], "BasePath");
            }
        }

        [Test]
        public void LoadProjectWithComplexSettings()
        {
            using (TestResource file = new TestResource("NUnitProject_ComplexSettings.nunit"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                var projectDir = Path.GetDirectoryName(file.Path);
                var binDir = Path.Combine(projectDir, "bin");
                var debugDir = Path.Combine(binDir, "debug");
                var releaseDir = Path.Combine(binDir, "release");

                ClassicAssert.AreEqual(2, project.ConfigNames.Count);

                TestPackage package1 = project.GetTestPackage("Debug");
                ClassicAssert.AreEqual(debugDir, package1.Settings["BasePath"]);
                ClassicAssert.AreEqual(true, package1.Settings["AutoBinPath"]);
                ClassicAssert.AreEqual("v2.0", package1.Settings["RuntimeFramework"]);

                ClassicAssert.AreEqual(2, package1.SubPackages.Count);
                ClassicAssert.AreEqual(
                    Path.Combine(debugDir, "assembly1.dll"),
                    package1.SubPackages[0].FullName);
                ClassicAssert.AreEqual(
                    Path.Combine(debugDir, "assembly2.dll"),
                    package1.SubPackages[1].FullName);

                TestPackage package2 = project.GetTestPackage("Release");
                ClassicAssert.AreEqual(releaseDir, package2.Settings["BasePath"]);
                ClassicAssert.AreEqual(true, package2.Settings["AutoBinPath"]);
                ClassicAssert.AreEqual("v4.0", package2.Settings["RuntimeFramework"]);

                ClassicAssert.AreEqual(2, package2.SubPackages.Count);
                ClassicAssert.AreEqual(
                    Path.Combine(releaseDir, "assembly1.dll"),
                    package2.SubPackages[0].FullName);
                ClassicAssert.AreEqual(
                    Path.Combine(releaseDir, "assembly2.dll"),
                    package2.SubPackages[1].FullName);
            }
        }

        [Test]
        public void DefaultConfigFile()
        {
            using (TestResource file = new TestResource("NUnitProject.nunit"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                TestPackage package = project.GetTestPackage("Debug");
                Assert.That(package.Settings.ContainsKey("ConfigurationFile"), "No ConfigurationFile setting found");
                Assert.That(package.Settings["ConfigurationFile"], Is.EqualTo(Path.ChangeExtension(file.Path, ".config")));
            }
        }

        [TestCase("NUnitProject_ConfigFilePerConfig.nunit", "Debug", "D.config")]
        [TestCase("NUnitProject_ConfigFilePerConfig.nunit", "Release", "R.config")]
        public void CanSpecifyConfigFilePerConfig(string projectName, string config, string expectedFile)
        {
            using (TestResource file = new TestResource(projectName))
            {
                IProject project = _loader.LoadFrom(file.Path);
                TestPackage package = project.GetTestPackage(config);
                Assert.That(package.Settings.ContainsKey("ConfigurationFile"), "No ConfigurationFile setting found");
                Assert.That(package.Settings["ConfigurationFile"], Is.EqualTo(expectedFile));
            }
        }
    }
}
