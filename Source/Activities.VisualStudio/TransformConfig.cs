//-----------------------------------------------------------------------
// <copyright file="TransformConfig.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.TeamFoundationServer
{
    using System.Activities;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.Web.Publishing.Tasks;

    /// <summary>
    /// Activity to transform config files using the Xml Document Transformations.
    /// </summary>
    /// <remarks>
    /// For more information using Xml Document Tranformations visit:
    /// http://go.microsoft.com/fwlink/?LinkId=125889
    /// </remarks>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class TransformConfig : BaseCodeActivity
    {
        /// <summary>
        /// Gets or sets the Destination file after transformation has been done.
        /// </summary>
        public InArgument<string> DestinationFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the source file which should be transformed.
        /// </summary>
        public InArgument<string> SourceFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file containing the config transformations.
        /// </summary>
        public InArgument<string> TransformFile
        {
            get;
            set;
        }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            if (string.IsNullOrEmpty(this.ActivityContext.GetValue(this.DestinationFile)))
            {
                this.LogBuildError("Destination File is Required");
                return;
            }

            if (string.IsNullOrEmpty(this.ActivityContext.GetValue(this.SourceFile)))
            {
                this.LogBuildError("Source File is Required");
                return;
            }

            if (string.IsNullOrEmpty(this.ActivityContext.GetValue(this.TransformFile)))
            {
                this.LogBuildError("Transform File is Required");
                return;
            }

            using (XmlTransformableDocument xmlTarget = new XmlTransformableDocument { PreserveWhitespace = true })
            {
                xmlTarget.Load(this.ActivityContext.GetValue(this.SourceFile));
                bool fileTransformed = OpenTransformFile(this.ActivityContext.GetValue(this.TransformFile)).Apply(xmlTarget);
                if (fileTransformed)
                {
                    xmlTarget.Save(this.ActivityContext.GetValue(this.DestinationFile));
                }
            }
        }

        /// <summary>
        /// Opens a transformation file that will be used to transform a config file.
        /// </summary>
        /// <param name="transformFile">the full path to the file.</param>
        /// <returns>the Xml transformation</returns>
        private static XmlTransformation OpenTransformFile(string transformFile)
        {
            return new XmlTransformation(transformFile);
        }
    }
}