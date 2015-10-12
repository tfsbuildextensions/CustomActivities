using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace TfsBuildExtensions.Activities.Scripting
{
    [Serializable]
    public class PowerShellExecutionException : Exception
    {
        Exception failReason;
        ReadOnlyCollection<ErrorRecord> errorRecords;

        public PowerShellExecutionException()
            : base(ErrorMessages.ErrorOccursWhenInvokePowerShellCommand)
        {
        }

        public PowerShellExecutionException(string message)
            : base(message)
        {
        }

        public PowerShellExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [CLSCompliant(false)]
        public PowerShellExecutionException(Exception reason, Collection<ErrorRecord> errors)
            : base(reason.Message)
        {
            this.failReason = reason;
            this.errorRecords = new ReadOnlyCollection<ErrorRecord>(errors);
        }

        protected PowerShellExecutionException(SerializationInfo serializeInfo, StreamingContext context)
            : base(serializeInfo, context)
        {
            if (serializeInfo == null)
            {
                throw new ArgumentNullException(context.ToString());
            }
            this.failReason = (Exception)serializeInfo.GetValue("FailReason", typeof(Exception));
            this.errorRecords = (ReadOnlyCollection<ErrorRecord>)serializeInfo.GetValue("ErrorRecords", typeof(ReadOnlyCollection<ErrorRecord>));
        }

        [CLSCompliant(false)]
        public ReadOnlyCollection<ErrorRecord> ErrorRecords
        {
            get
            {
                if (this.errorRecords != null)
                {
                    return this.errorRecords;
                }
                else
                {
                    return new ReadOnlyCollection<ErrorRecord>(new Collection<ErrorRecord>());
                }
            }
        }

        public Exception FailReason
        {
            get
            {
                return this.failReason;
            }
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("FailReason", this.FailReason, typeof(Exception));
            info.AddValue("ErrorRecords", this.ErrorRecords, typeof(ReadOnlyCollection<ErrorRecord>));
        }
    }
}
