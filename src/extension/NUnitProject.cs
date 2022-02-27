// ***********************************************************************
// Copyright (c) 2011-2021 Charlie Poole
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

using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Common;
using NUnit.Engine.Extensibility;

namespace NUnit.Engine.Services.ProjectLoaders
{
    public class NUnitProject : IProject
    {
        private const string ROOT_NODE = "NUnitProject";
        private const string SETTINGS_NODE = "Settings";
        private const string CONFIG_NODE = "Config";
        private const string ASSEMBLY_NODE = "assembly";

        private const string ACTIVECONFIG_ATTR = "activeconfig";
        private const string NAME_ATTR = "name";
        private const string PATH_ATTR = "path";
        private const string APPBASE_ATTR = "appbase";
        private const string CONFIGFILE_ATTR = "configfile";
        private const string BINPATH_ATTR = "binpath";
        private const string BINPATHTYPE_ATTR = "binpathtype";
        private const string RUNTIME_ATTR = "runtimeFramework";
        private const string PROCESS_ATTR = "processModel";
        private const string DOMAIN_ATTR = "domainUsage";

        private const string BINPATH_AUTO = "auto";

        #region IProject Members

        /// <summary>
        /// Gets the path to the file storing this project, if any.
        /// If the project has not been saved, this is null.
        /// </summary>
        public string ProjectPath { get; private set; }

        /// <summary>
        /// Gets the active configuration, as defined
        /// by the particular project. For an NUnit
        /// project, we use the activeConfig attribute
        /// if present and otherwise return the first
        /// config found.
        /// </summary>
        public string ActiveConfigName { get; private set; }

        public IList<string> ConfigNames
        {
            get 
            {
                var result = new List<string>();
                foreach (XmlNode node in ConfigNodes)
                    result.Add(node.GetAttribute(NAME_ATTR));
                return result; 
            }
        }

        public TestPackage GetTestPackage()
        {
            return GetTestPackage(null);
        }

        public TestPackage GetTestPackage(string configName)
        {
            var dummy = new TestPackage();
            var package = dummy.AddSubPackage(ProjectPath);
            if (configName == null)
                configName = ActiveConfigName;

            if (configName != null)
            {
                XmlNode configNode = GetConfigNode(configName);
                
                string basePath = GetBasePathForConfig(configNode);

                foreach (XmlNode node in configNode.SelectNodes(ASSEMBLY_NODE))
                {
                    string assembly = node.GetAttribute(PATH_ATTR);
                    if (basePath != null)
                        assembly = Path.Combine(basePath, assembly);
                    package.AddSubPackage(assembly);
                }

                var settings = GetSettingsForConfig(configNode);
                foreach (var key in settings.Keys)
                    package.Settings.Add(key, settings[key]);
            }

            return package;
        }

        #endregion

        #region Public Methods

        public void Load(string filename)
        {
            ProjectPath = NormalizePath(Path.GetFullPath(filename));

            var doc = new XmlDocument();
            doc.Load(ProjectPath);
            Initialize(doc);
        }

        public void LoadXml(string xmlText)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlText);
            Initialize(doc);
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// The top-level (NUnitProject) node
        /// </summary>
        internal XmlNode RootNode { get; private set; }

        /// <summary>
        /// The Settings node if present, otherwise null
        /// </summary>
        internal XmlNode SettingsNode { get; private set; }

        /// <summary>
        /// The collection of Config nodes - may be empty
        /// </summary>
        internal XmlNodeList ConfigNodes { get; private set; }

        /// <summary>
        /// The specified Runtime, or null
        /// </summary>
        internal string RuntimeFramework { get; private set; }

        /// <summary>
        /// The specified process model, or null
        /// </summary>
        internal string ProcessModel { get; private set; }

        /// <summary>
        /// The specified domain usage, or null
        /// </summary>
        internal string DomainUsage { get; private set; }

        /// <summary>
        /// The project base path (Appbase)
        /// </summary>
        internal string ProjectBase { get; private set; }

        #endregion

        #region Helper Methods

        private void Initialize(XmlDocument doc)
        {
            RootNode = doc.DocumentElement;
            SettingsNode = RootNode.SelectSingleNode(SETTINGS_NODE);
            ConfigNodes = RootNode.SelectNodes(CONFIG_NODE);

            if (SettingsNode != null)
            {
                ActiveConfigName = SettingsNode.GetAttribute(ACTIVECONFIG_ATTR);
                ProcessModel = SettingsNode.GetAttribute(PROCESS_ATTR);
                DomainUsage = SettingsNode.GetAttribute(DOMAIN_ATTR);
                ProjectBase = SettingsNode.GetAttribute(APPBASE_ATTR);
            }

            if (ActiveConfigName == null && ConfigNodes.Count > 0)
                ActiveConfigName = ConfigNodes[0].GetAttribute(NAME_ATTR);

            if (ProjectBase == null)
                ProjectBase = Path.GetDirectoryName(ProjectPath);
            else if (ProjectPath != null)
                ProjectBase = Path.Combine(Path.GetDirectoryName(ProjectPath), ProjectBase);
        }

        private IEnumerable<string> GetConfigNodeNames()
        {
            foreach (XmlNode node in ConfigNodes)
            {
                yield return node.GetAttribute(NAME_ATTR);            
            }
        }

        private XmlNode GetConfigNode(string name)
        {
            foreach (XmlNode node in ConfigNodes)
                if (node.GetAttribute(NAME_ATTR) == name)
                    return node;

            var configNodeNames = new List<string>(GetConfigNodeNames()).ToArray();
            var errorMessage = "Unable to find a configuration named " + name.Enquote() + ".";
            var tip =
                configNodeNames.Length == 0 ? " There are no configuration nodes defined in " + ProjectPath + "." :
                configNodeNames.Length == 1 ? " The only configuration defined is " + name.Enquote() :
                                              " Available configuration names are " + configNodeNames.Enquote().Join(", ") + ".";
            throw new NUnitEngineException(errorMessage + tip);
        }

        private string GetBasePathForConfig(XmlNode configNode)
        {
            string configBasePath = NormalizePath(configNode.GetAttribute(APPBASE_ATTR));
            if (configBasePath == null)
                configBasePath = ProjectBase;
            else if (ProjectBase != null)
                configBasePath = Path.Combine(ProjectBase, configBasePath);

            return configBasePath;
        }

        private IDictionary<string, object> GetSettingsForConfig(XmlNode configNode)
        {
            var settings = new Dictionary<string, object>();

            string basePath = GetBasePathForConfig(configNode);
            if (basePath != ProjectPath)
                settings[RunnerSettings.BasePath] = basePath;

            settings[RunnerSettings.ConfigurationFile] = 
                configNode.GetAttribute(CONFIGFILE_ATTR) ??
                Path.ChangeExtension(ProjectPath, ".config");

            string binpath = configNode.GetAttribute(BINPATH_ATTR);
            if (binpath != null)
                settings[RunnerSettings.PrivateBinPath] = binpath;

            string binpathtype = configNode.GetAttribute(BINPATHTYPE_ATTR);
            if (binpathtype != null && binpathtype.ToLower() == BINPATH_AUTO)
                settings[RunnerSettings.AutoBinPath] = true;

            string runtime = configNode.GetAttribute(RUNTIME_ATTR);
            if (runtime != null)
                settings[RunnerSettings.RuntimeFramework] = runtime;

            if (ProcessModel != null)
                settings[RunnerSettings.ProcessModel] = ProcessModel;

            if (DomainUsage != null)
                settings[RunnerSettings.DomainUsage] = DomainUsage;

            return settings;
        }

        static readonly char[] PATH_SEPARATORS = new char[] { '/', '\\' };

        private string NormalizePath(string path)
        {
            char sep = Path.DirectorySeparatorChar;

            if (path != null)
                foreach (char alt in PATH_SEPARATORS)
                    if (alt != sep)
                        path = path.Replace(alt, sep);

            return path;
        }

        #endregion
    }
}
