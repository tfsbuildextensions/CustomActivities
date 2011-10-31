//-----------------------------------------------------------------------
// <copyright file="TFBaseCodeActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Base class for TFS Activities that require a TeamProjectCollection
    /// </summary>
    public abstract class TFBaseCodeActivity : BaseCodeActivity
    {
        /// <summary>
        /// Team Project Collection for this activity. 
        /// </summary>
        [System.ComponentModel.Description("The TFS Team Project Collection for this activity's operations")]
        public InArgument<TfsTeamProjectCollection> TeamProjectCollection { get; set; }

        /// <summary>
        /// Protected; value of the TeamProjectCollection InArgument
        /// </summary>
        protected TfsTeamProjectCollection ProjectCollection { get; private set; }

        /// <summary>
        /// Entry point to the Activity. This validates that the TeamProjectCollection is not null and calls the base Execute
        /// </summary>
        /// <param name="context">CodeActivityContext</param>
        protected override void Execute(CodeActivityContext context)
        {
            this.ProjectCollection = this.TeamProjectCollection.Get(context);
            TfsBuildExtensions.TfsUtilities.ArgumentValidation.ValidateObjectIsNotNull(this.ProjectCollection, "TeamProjectCollection");

            base.Execute(context);
        }
    }
}
