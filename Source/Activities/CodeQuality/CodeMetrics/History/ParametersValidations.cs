//-----------------------------------------------------------------------
// <copyright file="ParametersValidations.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

using TfsBuildExtensions.Activities.CodeQuality.Proxy;

namespace TfsBuildExtensions.Activities.CodeQuality.History
{
    using Microsoft.TeamFoundation.Build.Client;


    /// <summary>
    /// Provides validations for the parameters of the <see cref="CodeMetricsHistory"/> activity.
    /// </summary>
    public interface IParametersValidations
    {
        /// <summary>
        /// Validate the required parameters are there and they refer to an existing item (file, folder)
        /// </summary>
        bool ParametersAreValid();
    }

    /// <summary>
    /// Provides validations for the parameters of the <see cref="CodeMetricsHistory"/> activity.
    /// </summary>
    public class ParametersValidations : IParametersValidations
    {

        private IActivityContextProxy _proxyContext;
        private IFileSystemProxy _proxyFileSystem;

        /// <summary>
        /// Constructor to inject dependencies (coupling concerns)
        /// </summary>
        public ParametersValidations(IActivityContextProxy proxyContext, IFileSystemProxy proxyFileSystem)
        {
            _proxyContext = proxyContext;
            _proxyFileSystem = proxyFileSystem;
        }

        /// <summary>
        /// Validate the required parameters are there and they refer to an existing item (file, folder)
        /// </summary>
        public bool ParametersAreValid()
        {
            return (MandatoryParametersArePresent() && ParametersExists());
        }

        private bool ParametersExists()
        {
            bool valid = true;
            
            if (!_proxyFileSystem.FileExists(_proxyContext.SourceFileName))
            {
                valid = FailCurrentBuild("SourceFileName property for the CodeMetricsHistory does not match an existing file");
            }
            if (!_proxyFileSystem.DirectoryExists(_proxyContext.HistoryDirectory))
            {
                valid = FailCurrentBuild("HistoryDirectory property for the CodeMetricsHistory does not match an existing directory");
            }
            return valid;
        }

        private bool MandatoryParametersArePresent()
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(_proxyContext.HistoryDirectory))
            {
                valid = FailCurrentBuild("HistoryDirectory property for the CodeMetricsHistory cannot be empty");
            }
            if (string.IsNullOrWhiteSpace(_proxyContext.HistoryFileName))
            {
                valid = FailCurrentBuild("HistoryFileName property for the CodeMetricsHistory cannot be empty");
            }
            if (string.IsNullOrWhiteSpace(_proxyContext.SourceFileName))
            {
                valid = FailCurrentBuild("SourceFileName property for the CodeMetricsHistory cannot be empty");
            }
            if (_proxyContext.HowManyFilesToKeepInDirectory < 2)
            {
                valid = FailCurrentBuild("HowManyFilesToKeepInDirectory property for the CodeMetricsHistory should be higher than 1");
            }
            return valid;
        }

        private bool FailCurrentBuild(string msg)
        {
            _proxyContext.BuildDetail.Status = BuildStatus.Failed;
            _proxyContext.BuildDetail.Save();
            _proxyContext.LogBuildError(msg);
            return false;
        }
    }
}
