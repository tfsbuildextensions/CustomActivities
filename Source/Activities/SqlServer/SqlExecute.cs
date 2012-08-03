//-----------------------------------------------------------------------
// <copyright file="SqlExecute.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.SqlServer
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.SqlServer.Extended;

    /// <summary>
    /// SqlExecuteAction
    /// </summary>
    public enum SqlExecuteAction
    {
        /// <summary>
        /// Execute
        /// </summary>
        Execute,

        /// <summary>
        /// Execute
        /// </summary>
        ExecuteScalar,

        /// <summary>
        /// Execute
        /// </summary>
        ExecuteRawReader
    }

    /// <summary>
    /// <b>Valid Actions are:</b>
    /// <para><i>Execute</i> (<b>Required: </b> ConnectionString, Sql or Files <b>Optional:</b> CommandTimeout, Parameters, Retry, UseTransaction)</para>
    /// <para><i>ExecuteRawReader</i> (<b>Required: </b> ConnectionString, Sql <b>Optional:</b> CommandTimeout, Parameters, Retry, UseTransaction <b>Output: </b> RawReaderResult)</para>
    /// <para><i>ExecuteScalar</i> (<b>Required: </b> ConnectionString, Sql <b>Optional:</b> CommandTimeout, Parameters, Retry, UseTransaction <b>Output: </b> ScalarResult)</para>
    /// <para><b>Remote Execution Support:</b> NA</para>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class SqlExecute : BaseCodeActivity
    {
        private static readonly Regex Splitter = new Regex(@"^\s*GO\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private SqlExecuteAction action = SqlExecuteAction.Execute;
        private int commandTimeout = 30;
        private DateTime timer;
        private string[] files;

        internal delegate void ScriptExecutionEventHandler(object sender, ExecuteEventArgs e);

        internal event ScriptExecutionEventHandler ScriptFileExecuted;

        /// <summary>
        /// Sets the connection string to use for executing the Sql or Files
        /// </summary>
        public InArgument<string> ConnectionString { get; set; }

        /// <summary>
        /// Sets the timeout in seconds. Default is 30
        /// </summary>
        public int CommandTimeout
        {
            get { return this.commandTimeout; }
            set { this.commandTimeout = value; }
        }

        /// <summary>
        /// Sets the files to execute
        /// </summary>
        public InArgument<IEnumerable<string>> Files { get; set; }

        /// <summary>
        /// Sets the Sql to execute
        /// </summary>
        public InArgument<string> Sql { get; set; }

        /// <summary>
        /// Sets the parameters to substitute at execution time
        /// </summary>
        public InArgument<string[]> Parameters { get; set; }

        /// <summary>
        /// Specifies whether files should be re-executed if they initially fail
        /// </summary>
        public bool Retry { get; set; }

        /// <summary>
        /// Set to true to run the sql within a transaction
        /// </summary>
        public bool UseTransaction { get; set; }

        /// <summary>
        /// Gets the scalar result
        /// </summary>
        public OutArgument<string> ScalarResult { get; set; }

        /// <summary>
        /// Gets the raw output from the reader
        /// </summary>
        public OutArgument<string> RawReaderResult { get; set; }

        /// <summary>
        /// Specifies the action to perform
        /// </summary>
        public SqlExecuteAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.Action)
            {
                case SqlExecuteAction.Execute:
                case SqlExecuteAction.ExecuteScalar:
                case SqlExecuteAction.ExecuteRawReader:
                    this.ExecuteSql();
                    break;
                default:
                    throw new ArgumentException("Action not supported");
            }
        }

        private static string LoadScript(string fileName)
        {
            string retValue;
            using (StreamReader textFileReader = new StreamReader(fileName, System.Text.Encoding.Default, true))
            {
                retValue = new SqlScriptLoader(textFileReader).ReadToEnd();
            }

            return retValue;
        }

        private string SubstituteParameters(string sqlCommandText)
        {
            if (this.Parameters.Expression == null)
            {
                return sqlCommandText;
            }

            return this.Parameters.Get(this.ActivityContext).Aggregate(sqlCommandText, (current, parameter) => current.Replace(parameter.Split(new char['='])[0], parameter.Split(new char['='])[1]));
         }

        private void ExecuteSql()
        {
            this.ScriptFileExecuted += this.ScriptExecuted;
            try
            {
                this.timer = DateTime.Now;
                if (!string.IsNullOrEmpty(this.Sql.Get(this.ActivityContext)))
                {
                    this.ExecuteText();
                }
                else
                {
                    this.ExecuteFiles();
                }
            }
            finally
            {
                this.ScriptFileExecuted -= this.ScriptExecuted;
            }
        }

        private void ExecuteFiles()
        {
            string sqlCommandText;
            bool retry = true;
            this.files = this.Files.Get(this.ActivityContext).ToArray();
            int previousFailures = this.files.Count();
            ApplicationException lastException = null;
            using (SqlConnection sqlConnection = this.CreateConnection(this.ConnectionString.Get(this.ActivityContext)))
            {
                sqlConnection.Open();
                while (retry)
                {
                    int errorNo = 0;
                    string[] failures = new string[this.files.Count()];
                    foreach (string fullfilename in this.ActivityContext.GetValue(this.Files))
                    {
                        this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Execute: {0}", fullfilename), BuildMessageImportance.High);

                        try
                        {
                            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Loading {0}.", new[] { fullfilename }), BuildMessageImportance.Low);
                            sqlCommandText = this.SubstituteParameters(LoadScript(fullfilename)) + Environment.NewLine;
                            string[] batches = Splitter.Split(sqlCommandText);
                            this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Split {0} into {1} batches.", new object[] { fullfilename, batches.Length }), BuildMessageImportance.Low);
                            SqlTransaction sqlTransaction = null;
                            SqlCommand command = sqlConnection.CreateCommand();
                            if (this.UseTransaction)
                            {
                                sqlTransaction = sqlConnection.BeginTransaction();
                            }

                            try
                            {
                                int batchNum = 1;
                                foreach (string batchText in batches)
                                {
                                    sqlCommandText = batchText.Trim();
                                    if (sqlCommandText.Length > 0)
                                    {
                                        command.CommandText = sqlCommandText;
                                        command.CommandTimeout = this.CommandTimeout;
                                        command.Connection = sqlConnection;
                                        command.Transaction = sqlTransaction;
                                        this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Executing Batch {0}", new object[] { batchNum++ }), BuildMessageImportance.Low);
                                        this.LogBuildMessage(sqlCommandText, BuildMessageImportance.Low);
                                        command.ExecuteNonQuery();
                                    }
                                }

                                if (sqlTransaction != null)
                                {
                                    sqlTransaction.Commit();
                                }
                            }
                            catch
                            {
                                if (sqlTransaction != null)
                                {
                                    sqlTransaction.Rollback();
                                }

                                throw;
                            }

                            this.OnScriptFileExecuted(new ExecuteEventArgs(new FileInfo(fullfilename)));
                        }
                        catch (SqlException se)
                        {
                            lastException = new ApplicationException(string.Format(CultureInfo.CurrentUICulture, "{0}. {1}", fullfilename, se.Message), se);
                            if (!this.Retry)
                            {
                                throw lastException;
                            }

                            failures[errorNo] = fullfilename;
                            errorNo++;
                            this.OnScriptFileExecuted(new ExecuteEventArgs(new FileInfo(fullfilename), se));
                        }
                    }

                    if (!this.Retry)
                    {
                        retry = false;
                    }
                    else
                    {
                        if (errorNo > 0)
                        {
                            this.files = new string[errorNo];
                            for (int i = 0; i < errorNo; i++)
                            {
                                this.files[i] = failures[i];
                            }

                            if (this.files.Count() >= previousFailures)
                            {
                                throw lastException;
                            }

                            previousFailures = this.files.Count();
                        }
                        else
                        {
                            retry = false;
                        }
                    }
                }
            }
        }

        private void ExecuteText()
        {
            using (SqlConnection sqlConnection = this.CreateConnection(this.ConnectionString.Get(this.ActivityContext)))
            using (SqlCommand command = new SqlCommand(this.SubstituteParameters(this.Sql.Get(this.ActivityContext)), sqlConnection))
            {
                command.CommandTimeout = this.CommandTimeout;
                this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Execute: {0}", command.CommandText), BuildMessageImportance.High);
                sqlConnection.Open();
                SqlTransaction sqlTransaction = null;
                try
                {
                    if (this.UseTransaction)
                    {
                        sqlTransaction = sqlConnection.BeginTransaction();
                        command.Transaction = sqlTransaction;
                    }

                    switch (this.Action)
                    {
                        case SqlExecuteAction.Execute:
                            command.ExecuteNonQuery();
                            break;
                        case SqlExecuteAction.ExecuteScalar:
                            var result = command.ExecuteScalar();
                            this.ScalarResult.Set(this.ActivityContext, result.ToString());
                            break;
                        case SqlExecuteAction.ExecuteRawReader:
                            using (SqlDataReader rawreader = command.ExecuteReader())
                            {
                                string temp = string.Empty;
                                while (rawreader.Read())
                                {
                                    string resultRow = string.Empty;
                                    for (int i = 0; i < rawreader.FieldCount; i++)
                                    {
                                        resultRow += rawreader[i] + " ";
                                    }

                                    temp += resultRow + Environment.NewLine;
                                }

                                this.RawReaderResult.Set(this.ActivityContext, temp);
                            }

                            break;
                    }

                    if (sqlTransaction != null)
                    {
                        sqlTransaction.Commit();
                    }

                    TimeSpan s = DateTime.Now - this.timer;
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Execution Time: {0} seconds", s.TotalSeconds), BuildMessageImportance.Low);
                    this.timer = DateTime.Now;
                }
                catch
                {
                    if (sqlTransaction != null)
                    {
                        sqlTransaction.Rollback();
                    }

                    throw;
                }
            }
        }

        private SqlConnection CreateConnection(string connectionString)
        {
            SqlConnection returnedConnection;
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.InfoMessage += this.TraceMessageEventHandler;
                returnedConnection = connection;
                connection = null;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }

            return returnedConnection;
        }

        private void TraceMessageEventHandler(object sender, SqlInfoMessageEventArgs e)
        {
            if (this.ScriptFileExecuted != null)
            {
                ExecuteEventArgs args = new ExecuteEventArgs(e.Errors);
                this.ScriptFileExecuted(null, args);
            }
        }

        private void OnScriptFileExecuted(ExecuteEventArgs scriptFileExecuted)
        {
            if (scriptFileExecuted != null && this.ScriptFileExecuted != null)
            {
                this.ScriptFileExecuted(null, scriptFileExecuted);
            }
        }

        private void ScriptExecuted(object sender, ExecuteEventArgs scriptInfo)
        {
            if (scriptInfo.ScriptFileInfo != null)
            {
                if (scriptInfo.Succeeded)
                {
                    TimeSpan s = DateTime.Now - this.timer;
                    this.LogBuildMessage(string.Format(CultureInfo.CurrentCulture, "Successfully executed: {0} ({1} seconds)", scriptInfo.ScriptFileInfo.Name, s.TotalSeconds));
                    this.timer = DateTime.Now;
                }
                else
                {
                    TimeSpan s = DateTime.Now - this.timer;
                    this.LogBuildWarning(string.Format(CultureInfo.CurrentCulture, "Failed to executed: {0}. {1} ({2} seconds)", scriptInfo.ScriptFileInfo.Name, scriptInfo.ExecutionException.Message, s.TotalSeconds));
                    this.timer = DateTime.Now;
                }
            }
            else
            {
                if (scriptInfo.SqlInfo != null)
                {
                    foreach (SqlError infoMessage in scriptInfo.SqlInfo)
                    {
                        this.LogBuildMessage("    - " + infoMessage.Message);
                    }
                }
            }
        }
    }
}