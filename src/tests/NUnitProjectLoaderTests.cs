﻿// ***********************************************************************
// Copyright (c) 2014-2021 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Tests.resources;
using NUnit.Framework;

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

                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual("Debug", project.ActiveConfigName);
                Assert.AreEqual(new string[] { "Debug", "Release" }, project.ConfigNames);

                TestPackage package1 = project.GetTestPackage("Debug");
                Assert.AreEqual(true, package1.Settings["AutoBinPath"], "AutoBinPath");
                Assert.AreEqual(projectDir, package1.Settings["BasePath"], "BasePath");

                TestPackage package2 = project.GetTestPackage("Release");
                Assert.AreEqual(true, package2.Settings["AutoBinPath"], "AutoBinPath");
                Assert.AreEqual(projectDir, package1.Settings["BasePath"], "BasePath");
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

                Assert.AreEqual(2, _project.ConfigNames.Count);
                Assert.AreEqual("Debug", _project.ActiveConfigName);
                Assert.AreEqual(new string[] { "Debug", "Release" }, _project.ConfigNames);

                TestPackage package1 = _project.GetTestPackage("Debug");
                Assert.AreEqual(2, package1.SubPackages.Count);
                Assert.AreEqual(
                    Path.Combine(debugDir, "assembly1.dll"),
                    package1.SubPackages[0].FullName);
                Assert.AreEqual(
                    Path.Combine(debugDir, "assembly2.dll"),
                    package1.SubPackages[1].FullName);

                Assert.AreEqual(debugDir, package1.Settings["BasePath"], "BasePath");
                Assert.AreEqual(true, package1.Settings["AutoBinPath"], "AutoBinPath");

                TestPackage package2 = _project.GetTestPackage("Release");
                Assert.AreEqual(
                    Path.Combine(releaseDir, "assembly1.dll"),
                    package2.SubPackages[0].FullName);
                Assert.AreEqual(
                    Path.Combine(releaseDir, "assembly2.dll"),
                    package2.SubPackages[1].FullName);

                Assert.AreEqual(releaseDir, package2.Settings["BasePath"]);
                Assert.AreEqual(true, package2.Settings["AutoBinPath"]);

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
                Assert.AreEqual("bin_path_value", package1.Settings["PrivateBinPath"], "PrivateBinPath");
                Assert.AreEqual(projectDir, package1.Settings["BasePath"], "BasePath");
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

                Assert.AreEqual(2, project.ConfigNames.Count);

                TestPackage package1 = project.GetTestPackage("Debug");
                Assert.AreEqual(debugDir, package1.Settings["BasePath"]);
                Assert.AreEqual(true, package1.Settings["AutoBinPath"]);
                Assert.AreEqual("Separate", package1.Settings["ProcessModel"]);
                Assert.AreEqual("Multiple", package1.Settings["DomainUsage"]);
                Assert.AreEqual("v2.0", package1.Settings["RuntimeFramework"]);

                Assert.AreEqual(2, package1.SubPackages.Count);
                Assert.AreEqual(
                    Path.Combine(debugDir, "assembly1.dll"),
                    package1.SubPackages[0].FullName);
                Assert.AreEqual(
                    Path.Combine(debugDir, "assembly2.dll"),
                    package1.SubPackages[1].FullName);

                TestPackage package2 = project.GetTestPackage("Release");
                Assert.AreEqual(releaseDir, package2.Settings["BasePath"]);
                Assert.AreEqual(true, package2.Settings["AutoBinPath"]);
                Assert.AreEqual("Separate", package2.Settings["ProcessModel"]);
                Assert.AreEqual("Multiple", package2.Settings["DomainUsage"]);
                Assert.AreEqual("v4.0", package2.Settings["RuntimeFramework"]);

                Assert.AreEqual(2, package2.SubPackages.Count);
                Assert.AreEqual(
                    Path.Combine(releaseDir, "assembly1.dll"),
                    package2.SubPackages[0].FullName);
                Assert.AreEqual(
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
