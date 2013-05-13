namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Common;
    using Microsoft.TeamFoundation.Client;
    using System.Globalization;
    using Microsoft.TeamFoundation.Framework.Client;
    using Microsoft.TeamFoundation;
    using System.Net;
    using TfsBuildExtensions.TfsUtilities;
    using System.Net.Http;
    using System.IO;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Get the most recent build for a build definition.
    /// </summary>
    [System.ComponentModel.Description("Activity to get a build from the servers un-versioned store")]
    [BuildActivity(HostEnvironmentOption.All)]
    public class GetDropDownloadLocation : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the name of the build to get details for.
        /// </summary>
        public InArgument<IBuildDetail> BuildDetail { get; set; }

        /// <summary>
        /// Gets or sets the name of the build to get details for.
        /// </summary>
        public OutArgument<string> DropDownloadPath { get; set; }

        /// <summary>
        /// Get the label details.
        /// </summary>
        protected override void InternalExecute()
        {
            var buildDetail = this.BuildDetail.Get(this.ActivityContext);
            // Calculate the full path to the Drop file.
            var dropDownloadPath = GetDropDownloadPath(buildDetail.BuildServer.TeamProjectCollection, buildDetail);
            this.DropDownloadPath.Set(this.ActivityContext, dropDownloadPath);

        }

        private string GetDropDownloadPath(TfsTeamProjectCollection collection, IBuildDetail buildDetail)
        {
            string droplocation = buildDetail.DropLocation;
            if (string.IsNullOrEmpty(droplocation))
            {
                throw new FailingBuildException(string.Format(CultureInfo.CurrentCulture, "No drop is available for {0}.", buildDetail.BuildNumber));
            }

            ILocationService locationService = collection.GetService<ILocationService>();
            string containersBaseAddress = locationService.LocationForAccessMapping( ServiceInterfaces.FileContainersResource,
                FrameworkServiceIdentifiers.FileContainers, locationService.DefaultAccessMapping);
            droplocation = BuildContainerPath.Combine(droplocation, string.Format(CultureInfo.InvariantCulture, "{0}.zip", buildDetail.BuildNumber));

            try
            {
                long containerId;
                string itemPath;
                BuildContainerPath.GetContainerIdAndPath(droplocation, out containerId, out itemPath);

                string downloadPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", containersBaseAddress, containerId, itemPath.TrimStart('/'));
                return downloadPath;
            }
            catch (InvalidPathException)
            {
                throw new FailingBuildException(string.Format(CultureInfo.CurrentCulture, "No drop is available for {0}.", buildDetail.BuildNumber));
            }
        }

    }
}
