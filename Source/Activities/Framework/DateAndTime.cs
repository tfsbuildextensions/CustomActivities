//-----------------------------------------------------------------------
// <copyright file="DateAndTime.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Framework
{
    using System;
    using System.Activities;
    using System.Globalization;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// DateAndTimeAction
    /// </summary>
    public enum DateAndTimeAction
    {
        /// <summary>
        /// Get
        /// </summary>
        Get,

        /// <summary>
        /// GetElapsed
        /// </summary>
        GetElapsed,

        /// <summary>
        /// CheckLater
        /// </summary>
        CheckLater,

        /// <summary>
        /// CheckBetween
        /// </summary>
        CheckBetween,

        /// <summary>
        /// AddDays
        /// </summary>
        AddDays,

        /// <summary>
        /// AddHours
        /// </summary>
        AddHours,

        /// <summary>
        /// AddMilliseconds
        /// </summary>
        AddMilliseconds,

        /// <summary>
        /// AddMinutes
        /// </summary>
        AddMinutes,

        /// <summary>
        /// AddMonths
        /// </summary>
        AddMonths,

        /// <summary>
        /// AddSeconds
        /// </summary>
        AddSeconds,

        /// <summary>
        /// AddTicks
        /// </summary>
        AddTicks,

        /// <summary>
        /// AddYears
        /// </summary>
        AddYears,
    }

    /// <summary>
    /// <b>Valid Action values are:</b>
    /// <para><i>AddDays</i> - <b>Required: </b>Format, Value <b>Optional: </b>Start <b>Output: </b> Result</para>
    /// <para><i>AddHours</i> - <b>Required: </b>Format, Value <b>Optional: </b>Start <b>Output: </b> Result</para>
    /// <para><i>AddMilliseconds</i> - <b>Required: </b>Format, Value <b>Optional: </b>Start <b>Output: </b> Result</para>
    /// <para><i>AddMinutes</i> - <b>Required: </b>Format, Value <b>Optional: </b>Start <b>Output: </b> Result</para>
    /// <para><i>AddMonths</i> - <b>Required: </b>Format, Value <b>Optional: </b>Start <b>Output: </b> Result</para>
    /// <para><i>AddSeconds</i> - <b>Required: </b>Format, Value <b>Optional: </b>Start <b>Output: </b> Result</para>
    /// <para><i>AddTicks</i> - <b>Required: </b>Format, Value <b>Optional: </b>Start <b>Output: </b> Result</para>
    /// <para><i>AddYears</i> - <b>Required: </b>Format, Value <b>Optional: </b>Start <b>Output: </b> Result</para>
    /// <para><i>CheckBetween</i> - <b>Required: </b>Start, End <b>Optional:</b> UseUtc <b>Output: </b> BoolResult</para>
    /// <para><i>CheckLater</i> - <b>Required: </b>Start <b>Optional:</b> UseUtc <b>Output: </b> BoolResult</para>
    /// <para><i>Get</i> - <b>Required: </b>Format <b>Optional:</b> UseUtc <b>Output: </b> Result</para>
    /// <para><i>GetElapsed</i> - <b>Required: </b>Format, Start <b>Optional: </b>End, UseUtc <b>Output: </b> Result</para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class DateAndTime : BaseCodeActivity
    {
        private DateAndTimeAction action = DateAndTimeAction.Get;

        /// <summary>
        /// Specifies the action to perform. Default is Get.
        /// </summary>
        public DateAndTimeAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// The start time to use
        /// </summary>
        public InArgument<DateTime> Start { get; set; }

        /// <summary>
        /// The end time to use for GetElapsed. Default is DateTime.Now
        /// </summary>
        public InArgument<DateTime> End { get; set; }

        /// <summary>
        /// Format to apply to the Result. For GetTime, Format can be any valid DateTime format. For GetElapsed, Format can be Milliseconds, Seconds, Minutes, Hours, Days or Total. Total returns dd:hh:mm:ss
        /// </summary>
        public InArgument<string> Format { get; set; }

        /// <summary>
        /// The Value to use in calculations
        /// </summary>
        public InArgument<string> Value { get; set; }

        /// <summary>
        /// The output Result
        /// </summary>
        public OutArgument<string> Result { get; set; }

        /// <summary>
        /// The output boolean result.
        /// </summary>
        public OutArgument<bool> BoolResult { get; set; }

        /// <summary>
        /// Set to true to use UTC Date / Time for the Action. Default is false.
        /// </summary>
        public bool UseUtc { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.Action)
            {
                case DateAndTimeAction.Get:
                    this.GetDate();
                    break;
                case DateAndTimeAction.GetElapsed:
                    this.GetElapsed();
                    break;
                case DateAndTimeAction.CheckLater:
                    this.CheckLater();
                    break;
                case DateAndTimeAction.CheckBetween:
                    this.CheckBetween();
                    break;
                case DateAndTimeAction.AddDays:
                    this.AddDays();
                    break;
                case DateAndTimeAction.AddHours:
                    this.AddHours();
                    break;
                case DateAndTimeAction.AddMilliseconds:
                    this.AddMilliseconds();
                    break;
                case DateAndTimeAction.AddMinutes:
                    this.AddMinutes();
                    break;
                case DateAndTimeAction.AddMonths:
                    this.AddMonths();
                    break;
                case DateAndTimeAction.AddSeconds:
                    this.AddSeconds();
                    break;
                case DateAndTimeAction.AddTicks:
                    this.AddTicks();
                    break;
                case DateAndTimeAction.AddYears:
                    this.AddYears();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private static DateTime GetDefaultOrUserStartTime(DateTime startTime)
        {
            // Default to current time if caller did not specify a time.
            return startTime == Convert.ToDateTime("01/01/0001 00:00:00", CultureInfo.CurrentCulture) ? DateTime.Now : startTime;
        }

        private void CheckLater()
        {
            if (this.ActivityContext.GetValue(this.Start) == Convert.ToDateTime("01/01/0001 00:00:00", CultureInfo.CurrentCulture))
            {
                this.LogBuildError("Start must be specified");
                return;
            }

            if (this.UseUtc)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Checking if: {0} is later than: {1}", DateTime.UtcNow.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture), this.ActivityContext.GetValue(this.Start).ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture)));
                this.ActivityContext.SetValue(this.BoolResult, DateTime.UtcNow > this.ActivityContext.GetValue(this.Start));
            }
            else
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Checking if: {0} is later than: {1}", DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture), this.ActivityContext.GetValue(this.Start).ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture)));
                this.ActivityContext.SetValue(this.BoolResult, DateTime.Now > this.ActivityContext.GetValue(this.Start));
            }
        }

        private void CheckBetween()
        {
            if (this.ActivityContext.GetValue(this.Start) == Convert.ToDateTime("01/01/0001 00:00:00", CultureInfo.CurrentCulture))
            {
                this.LogBuildError("Start must be specified");
                return;
            }

            if (this.ActivityContext.GetValue(this.End) == Convert.ToDateTime("01/01/0001 00:00:00", CultureInfo.CurrentCulture))
            {
                this.LogBuildError("End must be specified");
                return;
            }

            if (this.UseUtc)
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Checking if: {0} is between: {1} and: {2}", DateTime.UtcNow.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture), this.ActivityContext.GetValue(this.Start).ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture), this.ActivityContext.GetValue(this.End).ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture)));
                this.ActivityContext.SetValue(this.BoolResult, DateTime.UtcNow > this.ActivityContext.GetValue(this.Start) && DateTime.UtcNow < this.ActivityContext.GetValue(this.End));
            }
            else
            {
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Checking if: {0} is between: {1} and: {2}", DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture), this.ActivityContext.GetValue(this.Start).ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture), this.ActivityContext.GetValue(this.End).ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CurrentCulture)));
                this.ActivityContext.SetValue(this.BoolResult, DateTime.Now > this.ActivityContext.GetValue(this.Start) && DateTime.Now < this.ActivityContext.GetValue(this.End));
            }
        }

        private void GetElapsed()
        {
            if (this.ActivityContext.GetValue(this.Start) == Convert.ToDateTime("01/01/0001 00:00:00", CultureInfo.CurrentCulture))
            {
                this.LogBuildError("Start must be specified");
                return;
            }

            if (this.ActivityContext.GetValue(this.End) == Convert.ToDateTime("01/01/0001 00:00:00", CultureInfo.CurrentCulture))
            {
                this.ActivityContext.SetValue(this.End, this.UseUtc ? DateTime.UtcNow : DateTime.Now);
            }

            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Getting Elapsed: {0}", this.ActivityContext.GetValue(this.Format)));
            TimeSpan t = this.ActivityContext.GetValue(this.End) - this.ActivityContext.GetValue(this.Start);

            switch (this.ActivityContext.GetValue(this.Format))
            {
                case "MilliSeconds":
                    this.ActivityContext.SetValue(this.Result, t.TotalMilliseconds.ToString(CultureInfo.CurrentCulture));
                    break;
                case "Seconds":
                    this.ActivityContext.SetValue(this.Result, t.TotalSeconds.ToString(CultureInfo.CurrentCulture));
                    break;
                case "Minutes":
                    this.ActivityContext.SetValue(this.Result, t.TotalMinutes.ToString(CultureInfo.CurrentCulture));
                    break;
                case "Hours":
                    this.ActivityContext.SetValue(this.Result, t.TotalHours.ToString(CultureInfo.CurrentCulture));
                    break;
                case "Days":
                    this.ActivityContext.SetValue(this.Result, t.TotalDays.ToString(CultureInfo.CurrentCulture));
                    break;
                case "Total":
                    this.ActivityContext.SetValue(this.Result, string.Format(CultureInfo.CurrentCulture, "{0}:{1}:{2}:{3}", t.Days.ToString("00", CultureInfo.CurrentCulture), t.Hours.ToString("00", CultureInfo.CurrentCulture), t.Minutes.ToString("00", CultureInfo.CurrentCulture), t.Seconds.ToString("00", CultureInfo.CurrentCulture)));
                    break;
                default:
                    this.LogBuildError("Format must be specified");
                    return;
            }
        }

        private void GetDate()
        {
            this.LogBuildMessage("Getting Date / Time");
            this.ActivityContext.SetValue(this.Result, this.UseUtc ? DateTime.UtcNow.ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture) : DateTime.Now.ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }

        private void AddDays()
        {
            this.LogBuildMessage("Add Days");
            this.ActivityContext.SetValue(this.Result, GetDefaultOrUserStartTime(this.ActivityContext.GetValue(this.Start)).AddDays(Convert.ToDouble(this.ActivityContext.GetValue(this.Value), CultureInfo.CurrentCulture)).ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }

        private void AddHours()
        {
            this.LogBuildMessage("Add Hours");
            this.ActivityContext.SetValue(this.Result, GetDefaultOrUserStartTime(this.ActivityContext.GetValue(this.Start)).AddHours(Convert.ToDouble(this.ActivityContext.GetValue(this.Value), CultureInfo.CurrentCulture)).ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }

        private void AddMilliseconds()
        {
            this.LogBuildMessage("Add Milliseconds");
            this.ActivityContext.SetValue(this.Result, GetDefaultOrUserStartTime(this.ActivityContext.GetValue(this.Start)).AddMilliseconds(Convert.ToDouble(this.ActivityContext.GetValue(this.Value), CultureInfo.CurrentCulture)).ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }

        private void AddMinutes()
        {
            this.LogBuildMessage("Add Minutes");
            this.ActivityContext.SetValue(this.Result, GetDefaultOrUserStartTime(this.ActivityContext.GetValue(this.Start)).AddMinutes(Convert.ToDouble(this.ActivityContext.GetValue(this.Value), CultureInfo.CurrentCulture)).ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }

        private void AddMonths()
        {
            this.LogBuildMessage("Add Months");
            this.ActivityContext.SetValue(this.Result, GetDefaultOrUserStartTime(this.ActivityContext.GetValue(this.Start)).AddMonths(Convert.ToInt32(this.ActivityContext.GetValue(this.Value), CultureInfo.CurrentCulture)).ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }

        private void AddSeconds()
        {
            this.LogBuildMessage("Add Seconds");
            this.ActivityContext.SetValue(this.Result, GetDefaultOrUserStartTime(this.ActivityContext.GetValue(this.Start)).AddSeconds(Convert.ToDouble(this.ActivityContext.GetValue(this.Value), CultureInfo.CurrentCulture)).ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }

        private void AddTicks()
        {
            this.LogBuildMessage("Add Ticks");
            this.ActivityContext.SetValue(this.Result, GetDefaultOrUserStartTime(this.ActivityContext.GetValue(this.Start)).AddTicks(Convert.ToInt64(this.ActivityContext.GetValue(this.Value), CultureInfo.CurrentCulture)).ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }

        private void AddYears()
        {
            this.LogBuildMessage("Add Years");
            this.ActivityContext.SetValue(this.Result, GetDefaultOrUserStartTime(this.ActivityContext.GetValue(this.Start)).AddYears(Convert.ToInt32(this.ActivityContext.GetValue(this.Value), CultureInfo.CurrentCulture)).ToString(this.ActivityContext.GetValue(this.Format), CultureInfo.CurrentCulture));
        }
    }
}