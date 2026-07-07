// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using NUnit.Common;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Tests.resources;
using NUnit.Extensibility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                Has.Attribute<ExtensionAttribute>());
        }

        [Test]
        public void CheckExtensionPropertyAttribute()
        {
            Assert.That(typeof(NUnitProjectLoader),
                Has.Attribute<ExtensionPropertyAttribute>()
                    .With.Property("Name").EqualTo("FileExtension")
                    .And.Property("Value").EqualTo(".nunit"));
        }

        [Test]
        public void CheckExtension()
        {
            Assert.That(_loader.CanLoadFrom("dummy.nunit"));
            Assert.False(_loader.CanLoadFrom("dummy.junk"));
        }

        [Test]
        public void CanLoadEmptyProject()
        {
            using (TestResource file = new TestResource("NUnitProject_EmptyProject.nunit"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.That(project.ProjectPath, Is.EqualTo(file.Path));
                Assert.That(project.ConfigNames.Count, Is.EqualTo(0));
                Assert.Null(project.ActiveConfigName);
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
                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.BasePath), Is.EqualTo(projectDir));

                TestPackage package2 = project.GetTestPackage("Release");
                Assert.That(package2.Settings.GetValueOrDefault(SettingDefinitions.AutoBinPath), Is.True);
                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.BasePath), Is.EqualTo(projectDir));
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

                Assert.That(_project.ActiveConfigName, Is.EqualTo("Debug"));
                Assert.That(_project.ConfigNames, Is.EqualTo(new string[] { "Debug", "Release" }));

                TestPackage package1 = _project.GetTestPackage("Debug");
                Assert.That(package1.SubPackages[0].FullName, Is.EqualTo(Path.Combine(debugDir, "assembly1.dll")));
                Assert.That(package1.SubPackages[1].FullName, Is.EqualTo(Path.Combine(debugDir, "assembly2.dll")));

                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.BasePath), Is.EqualTo(debugDir));
                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.AutoBinPath), Is.EqualTo(true));

                TestPackage package2 = _project.GetTestPackage("Release");
                Assert.That(package2.SubPackages.Count, Is.EqualTo(2));
                Assert.That(package2.SubPackages[0].FullName, Is.EqualTo(Path.Combine(releaseDir, "assembly1.dll")));
                Assert.That(package2.SubPackages[1].FullName, Is.EqualTo(Path.Combine(releaseDir, "assembly2.dll")));

                Assert.That(package2.Settings.GetValueOrDefault(SettingDefinitions.BasePath), Is.EqualTo(releaseDir));
                Assert.That(package2.Settings.GetValueOrDefault(SettingDefinitions.AutoBinPath), Is.EqualTo(true));

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

                Assert.AreEqual(1, project.ConfigNames.Count);

                TestPackage package1 = project.GetTestPackage("Debug");
                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.PrivateBinPath), Is.EqualTo("bin_path_value"));
                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.BasePath), Is.EqualTo(projectDir));
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

                Assert.That(project.ConfigNames.Count, Is.EqualTo(2));

                TestPackage package1 = project.GetTestPackage("Debug");
                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.BasePath), Is.EqualTo(debugDir));
                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.AutoBinPath), Is.EqualTo(true));
                Assert.That(package1.Settings.GetValueOrDefault(SettingDefinitions.RequestedRuntimeFramework), Is.EqualTo("v2.0"));

                Assert.AreEqual(
                    Path.Combine(debugDir, "assembly1.dll"),
                    package1.SubPackages[0].FullName);
                Assert.AreEqual(
                    Path.Combine(debugDir, "assembly2.dll"),
                    package1.SubPackages[1].FullName);

                TestPackage package2 = project.GetTestPackage("Release");
                Assert.That(package2.Settings.GetValueOrDefault(SettingDefinitions.BasePath), Is.EqualTo(releaseDir));
                Assert.That(package2.Settings.GetValueOrDefault(SettingDefinitions.AutoBinPath), Is.EqualTo(true));
                Assert.That(package2.Settings.GetValueOrDefault(SettingDefinitions.RequestedRuntimeFramework), Is.EqualTo("v4.0"));

                Assert.That(package2.SubPackages.Count, Is.EqualTo(2));
                Assert.AreEqual(
                    Path.Combine(releaseDir, "assembly1.dll"),
                    package2.SubPackages[0].FullName);
                Assert.AreEqual(
                    Path.Combine(releaseDir, "assembly2.dll"),
                    package2.SubPackages[1].FullName);
            }
        }
    }
}
