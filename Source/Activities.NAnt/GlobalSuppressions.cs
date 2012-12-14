//-----------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "We can add a strong-name key later if we find we need one for example if the assemblies need to be GAC'd.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubType", Scope = "member", Target = "TfsBuildExtensions.Activities.Virtualization.Utilities.Utility.#GetResourceAllocationsettingData(System.Management.ManagementObject,System.UInt16,System.String,System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tfs")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tfs", Scope = "namespace", Target = "TfsBuildExtensions.Activities.NAnt")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "TfsBuildExtensions.Activities.NAnt")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "TfsBuildExtensions.Activities.NAnt.InvokeNAnt.#CreateInternalBody()")]
