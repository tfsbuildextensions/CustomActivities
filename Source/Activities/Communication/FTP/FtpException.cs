//-----------------------------------------------------------------------
// <copyright file="FtpException.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Communication
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The <c>FtpException</c> class encapsulates an FTP exception.
    /// </summary>
    [Serializable]
    public class FtpException : Exception
    {
        private readonly int ftpError;

        /// <summary>
        /// FtpException
        /// </summary>
        public FtpException()
        {
            this.ftpError = 0;            
        }

        /// <summary>
        /// FtpException
        /// </summary>
        /// <param name="message">string</param>
        public FtpException(string message) : this(-1, message)
        {
        }
        
        /// <summary>
        /// FtpException
        /// </summary>
        /// <param name="error">int</param>
        /// <param name="message">string</param>
        public FtpException(int error, string message) : base(message)
        {
            this.ftpError = error;
        }

        /// <summary>
        /// FtpException
        /// </summary>
        /// <param name="message">string</param>
        /// <param name="innerException">Exception</param>
        public FtpException(string message, Exception innerException) : base(message, innerException)
        {         
        }

        /// <summary>
        /// FtpException
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">StreamingContext</param>
        protected FtpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {         
        }

        /// <summary>
        /// ErrorCode
        /// </summary>
        public int ErrorCode
        {
            get { return this.ftpError; }
        }

        /// <summary>
        /// No specific impelementation is needed of the GetObjectData to serialize this object
        /// because all attributes are redefined.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data. </param>
        /// <param name="context">The destination for this serialization. </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}