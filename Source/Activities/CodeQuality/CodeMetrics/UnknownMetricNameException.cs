//-----------------------------------------------------------------------
// <copyright file="UnknownMetricNameException.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Extended
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception raised if we find a metric in the metric.exe outfit file (Default called metrics.xml with a name we dont recognize.
    /// Other option is that we have misspelled the metrics name in the code, but that will never ever happen
    /// </summary>
    [Serializable]
    public class UnknownMetricNameException : Exception
    {
        ////
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        ////

        /// <summary>
        /// UnknownMetricNameException
        /// </summary>
        public UnknownMetricNameException()
        {
        }

        /// <summary>
        /// UnknownMetricNameException
        /// </summary>
        public UnknownMetricNameException(string message) : base(message)
        {
        }

        /// <summary>
        /// UnknownMetricNameException
        /// </summary>
        public UnknownMetricNameException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// UnknownMetricNameException
        /// </summary>
        protected UnknownMetricNameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
