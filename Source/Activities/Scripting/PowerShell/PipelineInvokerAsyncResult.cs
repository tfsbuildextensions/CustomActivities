using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TfsBuildExtensions.Activities.Scripting.PowerShell
{
    class PipelineInvokerAsyncResult : IAsyncResult
    {
        AsyncCallback callback;
        object asyncState;
        EventWaitHandle asyncWaitHandle;

        Collection<ErrorRecord> errorRecords;
        public Collection<ErrorRecord> ErrorRecords
        {
            get
            {
                if (this.errorRecords == null)
                {
                    this.errorRecords = new Collection<ErrorRecord>();
                }

                return this.errorRecords;
            }
        }

        public Exception Exception
        {
            get;
            set;
        }

        public Collection<PSObject> PipelineOutput
        {
            get;
            set;
        }

        public object AsyncState
        {
            get { return this.asyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return this.asyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return true; }
        }

        public PipelineInvokerAsyncResult(Pipeline pipeline, AsyncCallback callback, object state)
        {
            this.asyncState = state;
            this.callback = callback;
            this.asyncWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            pipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(OnStateChanged);
            pipeline.InvokeAsync();
        }

        void Complete()
        {
            this.asyncWaitHandle.Set();
            if (this.callback != null)
            {
                this.callback(this);
            }
        }

        // Called by the underlying PowerShell pipeline object on state changes.
        void OnStateChanged(object sender, PipelineStateEventArgs args)
        {
            try
            {
                PipelineState state = args.PipelineStateInfo.State;
                Pipeline pipeline = sender as Pipeline;

                if (state == PipelineState.Completed)
                {
                    this.PipelineOutput = pipeline.Output.ReadToEnd();
                    ReadErrorRecords(pipeline);
                    Complete();
                }
                else if (state == PipelineState.Failed)
                {
                    this.Exception = args.PipelineStateInfo.Reason;
                    ReadErrorRecords(pipeline);
                    Complete();
                }
                else if (state == PipelineState.Stopped)
                {
                    Complete(); ;
                }
                else
                {
                    return; // nothing to do
                }
            }
            catch (Exception e)
            {
                this.Exception = e;
                Complete();
            }
        }

        void ReadErrorRecords(Pipeline pipeline)
        {
            Collection<object> errorsRecords = pipeline.Error.ReadToEnd();
            if (errorsRecords.Count != 0)
            {
                foreach (PSObject item in errorsRecords)
                {
                    ErrorRecord errorRecord = item.BaseObject as ErrorRecord;
                    this.ErrorRecords.Add(errorRecord);
                }
            }
        }
    }
}