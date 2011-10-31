//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Community TFS Build Extensions")]
[assembly: AssemblyDescription("Community TFS Build Extensions")]
[assembly: CLSCompliant(false)]

// NOTE: When updating the namespaces in the project please add new or update existing the XmlnsDefinitionAttribute
// You can add additional attributes in order to map any additional namespaces you have in the project
// [assembly: System.Workflow.ComponentModel.Serialization.XmlnsDefinition("http://schemas.com/Workflow", "Workflow")]
[assembly: InternalsVisibleTo("TfsBuildExtensions.Activities.Tests")]
