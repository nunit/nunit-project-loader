// Copyright (c) Charlie Poole and Contributors. MIT License - see LICENSE.txt

using System;
using System.Xml;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    sealed class ExtensionAttribute : Attribute { }
}

namespace NUnit.Engine.Services.ProjectLoaders
{
    /// <summary>
    /// SafeAttributeAccess provides an extension method for accessing XML attributes.
    /// </summary>
    public static class SafeAttributeAccess
    {
        /// <summary>
        /// Gets the value of the given attribute.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string GetAttribute(this XmlNode result, string name)
        {
            XmlAttribute attr = result.Attributes[name];

            return attr == null ? null : attr.Value;
        }
    }
}
