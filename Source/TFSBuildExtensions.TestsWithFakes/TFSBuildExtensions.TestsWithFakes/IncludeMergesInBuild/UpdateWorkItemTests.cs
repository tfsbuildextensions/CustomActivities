//-----------------------------------------------------------------------
// <copyright file="UpdateWorkItemTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
	using System;
	using System.Activities;
	using System.Collections;
	using System.Collections.Generic;
	using Microsoft.QualityTools.Testing.Fakes;
	using Microsoft.TeamFoundation.WorkItemTracking.Client;
	using Microsoft.TeamFoundation.WorkItemTracking.Client.Fakes;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TfsBuildExtensions.Activities.TeamFoundationServer.IncludeMergesInBuild;

	[TestClass]
	public class UpdateWorkItemTests
	{
		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_UpdateIsFalse_When_TypeDoesNotHaveIntegratedInField()
		{
			using (var context = ShimsContext.Create())
			{
				var args = GetWorkflowArgs(false, true);

				var activity = new UpdateWorkItem();
				IDictionary<string, object> outputs;
				var result = WorkflowInvoker.Invoke(activity, args, out outputs, TimeSpan.FromMinutes(1));
				Assert.IsFalse(result);
				Assert.IsTrue(outputs["WarningMessage"].ToString().Contains("TF42093"));
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_UpdateIsTrue_When_TypeDoesHaveIntegratedInField_AndWI_IsValid()
		{
			using (var context = ShimsContext.Create())
			{
				var args = GetWorkflowArgs(true, true);

				var result = WorkflowInvoker.Invoke(new UpdateWorkItem(), args);
				Assert.IsTrue(result);
			}
		}

		[TestMethod]
		[TestCategory("IncludeMergesInBuild")]
		public void Test_UpdateIsFalse_When_TypeDoesHaveIntegratedInField_AndWI_IsInvalid()
		{
			using (var context = ShimsContext.Create())
			{
				var args = GetWorkflowArgs(true, false);

				var activity = new UpdateWorkItem();
				IDictionary<string, object> outputs;
				var result = WorkflowInvoker.Invoke(activity, args, out outputs, TimeSpan.FromMinutes(1));
				Assert.IsFalse(result);
				Assert.IsTrue(outputs["WarningMessage"].ToString().Contains("TF42097"));
				Assert.IsTrue(outputs["WarningMessage"].ToString().Contains("Field1"));
				Assert.IsTrue(outputs["WarningMessage"].ToString().Contains("Field2"));
				Assert.IsTrue(outputs["WarningMessage"].ToString().Contains("Err1"));
				Assert.IsTrue(outputs["WarningMessage"].ToString().Contains("Err2"));
			}
		}

		private Dictionary<string, object> GetWorkflowArgs(bool containsIntegratedIn, bool isValid)
		{
			var workItem = new ShimWorkItem()
			{
				TypeGet = () => new ShimWorkItemType()
				{
					NameGet = () => "Task",
					FieldDefinitionsGet = () => new ShimFieldDefinitionCollection()
					{
						ContainsString = (s) => containsIntegratedIn
					}
				},
				PartialOpen = () => { },
				ItemSetStringObject = (s, o) => { if (!containsIntegratedIn) throw new ApplicationException("Should not be calling set IntegratedIn"); },
				HistorySetString = (s) => { if (!containsIntegratedIn) throw new ApplicationException("Should not be calling set History"); },
				IsValid = () => isValid,
				Validate = () => isValid ? new ArrayList() : new ArrayList(new List<Field>()
					{
						new ShimField() { NameGet = () => "Field1", ValueGet = () => "Err1" },
						new ShimField() { NameGet = () => "Field2", ValueGet = () => "Err2" } 
					})
			};
			var args = new Dictionary<string, object>()	{
						{ "WorkItem", (WorkItem)workItem },
						{ "BuildNumber", "123" },
						{ "WorkItemTypeFieldCache", new Dictionary<string, bool>() }
					};
			return args;
		}
	}
}
