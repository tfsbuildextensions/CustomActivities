//-----------------------------------------------------------------------
// <copyright file="BuildStepDuration.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;    

    /// <summary>
    /// Enumeration for which Build Steps durations should be returned
    /// </summary>
    public enum BuildStepDurationDisplayOption
    {
        /// <summary>
        /// Duration of all build steps would be returned
        /// </summary>
        All,

        /// <summary>
        /// Duration of the ten slowest build steps would be returned
        /// </summary>
        SlowestTenSteps,

        /// <summary>
        /// Duration of the ten fastest build steps would be returned
        /// </summary>
        FastestTenSteps        
    }

    /// <summary>
    /// The activity allows the durations of each build steps to be shown in build summary. User have option to either view all, slowest ten or fastest ten build steps.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class BuildStepDuration : BaseCodeActivity<IEnumerable<string>>
    {
        private const string DefaultSectionHeading = "Build Step Durations";

        /// <summary>
        /// Gets or sets the build detail to get Build Steps Duration. The Argument is mandatory.
        /// </summary>
        [RequiredArgument]
        public InArgument<IBuildDetail> BuildDetail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to publish the result on the build summary or not. Default Value is True
        /// </summary>        
        [DefaultValue(true)]
        public InArgument<bool> ShowResultOnSummary { get; set; }
        
        /// <summary>
        /// Gets or sets the Heading of the Custom Summary Section. If not set the default value selected is "Build Step Durations"
        /// </summary>
        [DefaultValue(DefaultSectionHeading)]
        public InArgument<string> SummarySectionHeading { get; set; }
        
        /// <summary>
        /// Gets or sets the option to select whether to return all, slowest ten or fast test build steps. Default Value is Slowest Ten Steps
        /// </summary>
        [DefaultValue(BuildStepDurationDisplayOption.SlowestTenSteps)]
        public InArgument<BuildStepDurationDisplayOption> BuildStepDurationOption { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>        
        /// <returns>A formatted string containing the list of build steps and their respective durations</returns>
        protected override IEnumerable<string> InternalExecute()
        {   
            var buildStepList = new Dictionary<int, Tuple<string, TimeSpan>>();
            var buildDetail = this.BuildDetail.Get(this.ActivityContext);
            var buildDurationOption = this.BuildStepDurationOption.Get(this.ActivityContext); 
            var showSummary = this.ShowResultOnSummary.Get(this.ActivityContext);
            var summarySectionHeading = this.SummarySectionHeading.Get(this.ActivityContext);
            if (string.IsNullOrWhiteSpace(summarySectionHeading))
            {
                summarySectionHeading = DefaultSectionHeading;
            }

            buildDetail.RefreshAllDetails();
            foreach (var step in buildDetail.Information.GetSortedNodes(new BuildInformationComparer()))
            {
                if (step.Fields.ContainsKey("StartTime") && step.Fields.ContainsKey("FinishTime") && step.Fields.ContainsKey("DisplayText"))
                {   
                    var startTime = DateTime.Parse(step.Fields["StartTime"]);
                    var finishTime = DateTime.Parse(step.Fields["FinishTime"]);
                    var stepName = step.Fields["DisplayText"];
                    if (finishTime != DateTime.MinValue && startTime != DateTime.MinValue)
                    {
                        var duration = finishTime - startTime;                                                
                        buildStepList.Add(step.Id, new Tuple<string, TimeSpan>(stepName, duration));
                    }                    
                }
            }

            var outputMessages = new Collection<string>();
            foreach (var buildStepDuration in GetSortedDurationSteps(buildDurationOption, buildStepList))
            {
                outputMessages.Add(string.Format("{0} - {1}", buildStepDuration.Item1, buildStepDuration.Item2.ToString(@"hh\:mm\:ss")));
            }

            if (showSummary)
            {
                this.CreateSummarySection(summarySectionHeading, outputMessages);
            }
            
            return outputMessages;
        }

        private static IEnumerable<Tuple<string, TimeSpan>> GetSortedDurationSteps(BuildStepDurationDisplayOption buildStepDurationOption, Dictionary<int, Tuple<string, TimeSpan>> buildStepList)
        {
            switch (buildStepDurationOption)
            {
                case BuildStepDurationDisplayOption.FastestTenSteps:
                    return buildStepList.Values.OrderBy(s => s.Item2).Take(10);

                case BuildStepDurationDisplayOption.SlowestTenSteps:
                    return buildStepList.Values.OrderByDescending(s => s.Item2).Take(10);
            }

            return buildStepList.Values;
        }

        private void CreateSummarySection(string heading, IEnumerable<string> outputMessages)
        {
            foreach (string message in outputMessages)
            {
                this.ActivityContext.Track(new CustomSummaryInformation() 
                                        {
                                            SectionPriority = 75,
                                            Message = message,
                                            SectionHeader = heading,
                                            SectionName = heading.Replace(" ", "_")
                                        });
            }
        }

        internal class BuildInformationComparer : IComparer<IBuildInformationNode>
        {
            public int Compare(IBuildInformationNode itemToCompare, IBuildInformationNode itemToCompareWith)
            {
                if (itemToCompare.Id > itemToCompareWith.Id)
                {
                    return 1;
                }
                else if (itemToCompare.Id == itemToCompareWith.Id)
                {
                    return 0;
                }

                return -1;
            }
        }
    }
}
