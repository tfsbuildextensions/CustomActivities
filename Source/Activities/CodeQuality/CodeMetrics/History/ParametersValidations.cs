//-----------------------------------------------------------------------
// <copyright file="ParametersValidations.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
#pragma warning disable 1591
namespace TfsBuildExtensions.Activities.CodeQuality.History
{
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.CodeQuality.Proxy;

    /// <summary>
    /// Provides validations for the parameters of the <see cref="CodeMetricsHistory"/> activity.
    /// </summary>
    public interface IParametersValidations
    {
        bool ParametersAreValid();
    }

    /// <summary>
    /// Provides validations for the parameters of the <see cref="CodeMetricsHistory"/> activity.
    /// </summary>
    public class ParametersValidations : IParametersValidations
    {
        private IActivityContextProxy proxyContext;
        private IFileSystemProxy proxyFileSystem;

        public ParametersValidations(IActivityContextProxy proxyContext, IFileSystemProxy proxyFileSystem)
        {
            this.proxyContext = proxyContext;
            this.proxyFileSystem = proxyFileSystem;
        }

        public bool ParametersAreValid()
        {
            return this.MandatoryParametersArePresent() && this.ParametersExists();
        }

        private bool ParametersExists()
        {
            bool valid = true;

            if (!this.proxyFileSystem.FileExists(this.proxyContext.SourceFileName))
            {
                valid = this.FailCurrentBuild("SourceFileName property for the CodeMetricsHistory does not match an existing file");
            }

            if (!this.proxyFileSystem.DirectoryExists(this.proxyContext.HistoryDirectory))
            {
                valid = this.FailCurrentBuild("HistoryDirectory property for the CodeMetricsHistory does not match an existing directory");
            }

            return valid;
        }

        private bool MandatoryParametersArePresent()
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(this.proxyContext.HistoryDirectory))
            {
                valid = this.FailCurrentBuild("HistoryDirectory property for the CodeMetricsHistory cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(this.proxyContext.HistoryFileName))
            {
                valid = this.FailCurrentBuild("HistoryFileName property for the CodeMetricsHistory cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(this.proxyContext.SourceFileName))
            {
                valid = this.FailCurrentBuild("SourceFileName property for the CodeMetricsHistory cannot be empty");
            }

            if (this.proxyContext.HowManyFilesToKeepInDirectory < 2)
            {
                valid = this.FailCurrentBuild("HowManyFilesToKeepInDirectory property for the CodeMetricsHistory should be higher than 1");
            }

            return valid;
        }

        private bool FailCurrentBuild(string msg)
        {
            this.proxyContext.BuildDetail.Status = BuildStatus.Failed;
            this.proxyContext.BuildDetail.Save();
            this.proxyContext.LogBuildError(msg);
            return false;
        }
    }
}
