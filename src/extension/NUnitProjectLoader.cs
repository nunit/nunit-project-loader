// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.IO;
using NUnit.Common;
using NUnit.Engine.Extensibility;

namespace NUnit.Engine.Services.ProjectLoaders
{
    [Extension(Path="/NUnit/Engine/TypeExtensions/IProjectLoader")]
    [ExtensionProperty("FileExtension", ".nunit")]
    public class NUnitProjectLoader : IProjectLoader
    {
        #region IProjectLoader Members

        public bool CanLoadFrom(string path)
        {
            return Path.GetExtension(path) == ".nunit";
        }

        public IProject LoadFrom(string path)
        {
            NUnitProject project = new NUnitProject();
            project.Load(path);
            return project;
        }

        public TestPackage GetTestPackage(string path)
        {
            return GetTestPackage(path, null);
        }

        public TestPackage GetTestPackage(string path, string configName)
        {
            NUnitProject project = new NUnitProject();
            project.Load(path);
            return project.GetTestPackage(configName);
        }

        #endregion
    }
}
