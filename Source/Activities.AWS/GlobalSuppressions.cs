﻿//-----------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "We can add a strong-name key later if we find we need one for example if the assemblies need to be GAC'd.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.DescribeAddresses.#Addresses")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.DescribeImages.#Images")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.DescribeInstances.#InstanceIds")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.DescribeInstances.#Reservations")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.DescribeSpotInstanceRequests.#SpotRequests")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.DescribeSpotInstanceRequests.#UpdatedSpotRequests")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.DescribeSpotPriceHistory.#PriceHistory")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.RequestSpotInstances.#SpotRequests")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.TerminateInstances.#InstanceChanges")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.TerminateInstances.#InstanceIds")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "TfsBuildExtensions.Activities.AWS")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Scope = "type", Target = "TfsBuildExtensions.Activities.AWS.EC2.ProductionDescriptionType")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Scope = "type", Target = "TfsBuildExtensions.Activities.AWS.EC2.SpotRequestType")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.RequestSpotInstances.#AmazonExecute()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OneTime", Scope = "member", Target = "TfsBuildExtensions.Activities.AWS.EC2.SpotRequestType.#OneTime")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "TfsBuildExtensions.Activities.AWS.Extended")]
