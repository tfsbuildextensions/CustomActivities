//-----------------------------------------------------------------------
// <copyright file="ProductionDescriptionType.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS.EC2
{
    /// <summary>
    /// Strongly typed names for product descriptions, kind-of
    /// </summary>
    public sealed class ProductionDescriptionType
    {
        /// <summary>
        /// A Windows-based AMI.
        /// </summary>
        public const string Windows = "Windows";

        /// <summary>
        /// A generic Linux-based AMI.
        /// </summary>
        public const string Linux = "Linux/UNIX";

        /// <summary>
        /// A SUSE Linux AMI.
        /// </summary>
        public const string Suse = "SUSE Linux";
    }
}
