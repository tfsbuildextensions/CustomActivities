//-----------------------------------------------------------------------
// <copyright file="Wmi.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Management
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// WmiAction
    /// </summary>
    public enum WmiAction
    {
        /// <summary>
        /// Execute
        /// </summary>
        Execute
    }

    /// <summary>
    /// <b>Valid Action values are:</b>
    /// <para><i>Execute</i> (<b>Required: </b> Class, Namespace, Method <b> Optional: </b>Instance, MethodParameters <b>Output: </b>ReturnValue)</para>
    /// <para><b>Remote Execution Support:</b> Yes</para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class Wmi : BaseRemoteCodeActivity
    {
        // Set a Default action
        private WmiAction action = WmiAction.Execute;

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public WmiAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Sets the namespace.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Namespace { get; set; }

        /// <summary>
        /// Sets the WMI class.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Class { get; set; }

        /// <summary>
        /// Gets the ReturnValue for Execute
        /// </summary>
        public OutArgument<string> ReturnValue { get; set; }

        /// <summary>
        /// Sets the Method used in Execute
        /// </summary>
        public InArgument<string> Method { get; set; }

        /// <summary>
        /// Sets the MethodParameters. Use #~# separate name and value.
        /// </summary>
        public InArgument<IEnumerable<string>> MethodParameters { get; set; }

        /// <summary>
        /// Sets the Wmi Instance used in Execute
        /// </summary>
        public InArgument<string> Instance { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.Action)
            {
                case WmiAction.Execute:
                    this.ExecuteWmi();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private void ExecuteWmi()
        {
            this.GetManagementScope(this.Namespace.Get(this.ActivityContext));
            string managementPath = this.Class.Get(this.ActivityContext);
            if (!string.IsNullOrEmpty(this.Instance.Get(this.ActivityContext)))
            {
                managementPath += "." + this.Instance.Get(this.ActivityContext);

                using (var classInstance = new ManagementObject(this.Scope, new ManagementPath(managementPath), null))
                {
                    // Obtain in-parameters for the method
                    ManagementBaseObject inParams = classInstance.GetMethodParameters(this.Method.Get(this.ActivityContext));
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Method: {0}", this.Method.Get(this.ActivityContext)), BuildMessageImportance.Low);

                    if (this.MethodParameters != null)
                    {
                        // Add the input parameters.
                        foreach (string[] data in this.MethodParameters.Get(this.ActivityContext).Select(param => param.Split(new[] { "#~#" }, StringSplitOptions.RemoveEmptyEntries)))
                        {
                            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Param: {0}. Value: {1}", data[0], data[1]), BuildMessageImportance.Low);
                            inParams[data[0]] = data[1];
                        }
                    }

                    // Execute the method and obtain the return values.
                    ManagementBaseObject outParams = classInstance.InvokeMethod(this.Method.Get(this.ActivityContext), inParams, null);
                    if (outParams != null)
                    {
                        this.ReturnValue.Set(this.ActivityContext, outParams["ReturnValue"].ToString());
                    }
                }
            }
            else
            {
                using (ManagementClass mgmtClass = new ManagementClass(this.Scope, new ManagementPath(managementPath), null))
                {
                    // Obtain in-parameters for the method
                    ManagementBaseObject inParams = mgmtClass.GetMethodParameters(this.Method.Get(this.ActivityContext));
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Method: {0}", this.Method.Get(this.ActivityContext)), BuildMessageImportance.Low);

                    if (this.MethodParameters != null)
                    {
                        // Add the input parameters.
                        foreach (string[] data in this.MethodParameters.Get(this.ActivityContext).Select(param => param.Split(new[] { "#~#" }, StringSplitOptions.RemoveEmptyEntries)))
                        {
                            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Param: {0}. Value: {1}", data[0], data[1]), BuildMessageImportance.Low);
                            inParams[data[0]] = data[1];
                        }
                    }

                    // Execute the method and obtain the return values.
                    ManagementBaseObject outParams = mgmtClass.InvokeMethod(this.Method.Get(this.ActivityContext), inParams, null);
                    if (outParams != null)
                    {
                        this.ReturnValue.Set(this.ActivityContext, outParams["ReturnValue"].ToString());
                    }
                }
            }
        }
    }
}