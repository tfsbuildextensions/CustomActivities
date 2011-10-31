//-----------------------------------------------------------------------
// <copyright file="SpotRequestType.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS.EC2
{
    /// <summary>
    /// Strongly typed names for spot requests, kind-of
    /// </summary>
    public sealed class SpotRequestType
    {
        /// <summary>
        /// Request a single spot instance.
        /// </summary>
        public const string OneTime = "one-time";

        /// <summary>
        /// Make a standing request for spot instances.
        /// </summary>
        public const string Persistent = "persistent";
    }
}
