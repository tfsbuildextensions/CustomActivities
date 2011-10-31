//-----------------------------------------------------------------------
// <copyright file="SimpleCommandLineBuilder.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.CodeQuality.Extended
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Helper class for generating command line arguments
    /// </summary>
    public class SimpleCommandLineBuilder
    {
        private readonly StringBuilder commandLine;

        /// <summary>
        /// Initializes a new instance of the SimpleCommandLineBuilder class
        /// </summary>
        public SimpleCommandLineBuilder()
        {
            this.commandLine = new StringBuilder();
        }

        private StringBuilder CommandLine
        {
            get
            {
                return this.commandLine;
            }
        }

        /// <summary>
        /// Append a list of file names to the argument
        /// </summary>
        /// <param name="fileNames">File names</param>
        /// <param name="delimiter">Delimiter</param>
        public void AppendFileNamesIfNotNull(string[] fileNames, string delimiter)
        {
            if ((fileNames != null) && (fileNames.Length > 0))
            {
                this.AppendSpaceIfNotEmpty();
                for (int j = 0; j < fileNames.Length; j++)
                {
                    if (j != 0)
                    {
                        this.AppendTextUnquoted(delimiter);
                    }

                    this.AppendFileNameWithQuoting(fileNames[j]);
                }
            }
        }

        /// <summary>
        /// Append a switch to the arguments
        /// </summary>
        /// <param name="switchName">Name of the switch</param>
        public void AppendSwitch(string switchName)
        {
            this.AppendSpaceIfNotEmpty();
            this.AppendTextUnquoted(switchName);
        }

        /// <summary>
        /// Append a switch to the arguments, if the parameter is not null
        /// </summary>
        /// <param name="switchName">Name of the switch</param>
        /// <param name="parameter">parameter</param>
        public void AppendSwitchIfNotNull(string switchName, string parameter)
        {
            if (parameter != null)
            {
                this.AppendSwitch(switchName);
                this.AppendTextWithQuoting(parameter);
            }
        }

        /// <summary>
        /// Append a switch to the arguments, if the parameter is not null
        /// </summary>
        /// <param name="switchName">Name of the switch</param>
        /// <param name="parameters">List of parameters</param>
        /// <param name="delimiter">Delimiter</param>
        public void AppendSwitchIfNotNull(string switchName, string[] parameters, string delimiter)
        {
            if ((parameters != null) && (parameters.Length > 0))
            {
                this.AppendSwitch(switchName);
                bool flag = true;
                foreach (string str in parameters)
                {
                    if (!flag)
                    {
                        this.AppendTextUnquoted(delimiter);
                    }

                    flag = false;
                    this.AppendTextWithQuoting(str);
                }
            }
        }

        /// <summary>
        /// Overrides to string to generate command line
        /// </summary>
        /// <returns>Command line arguments</returns>
        public override string ToString()
        {
            return this.CommandLine.ToString();
        }

        private static void AppendQuotedTextToBuffer(StringBuilder buffer, string unquotedTextToAppend)
        {
            if (unquotedTextToAppend != null)
            {
                buffer.Append('"');
                int num = unquotedTextToAppend.Count(t => '"' == t);

                if (num > 0)
                {
                    unquotedTextToAppend = unquotedTextToAppend.Replace("\\\"", "\\\\\"");
                    unquotedTextToAppend = unquotedTextToAppend.Replace("\"", "\\\"");
                }

                buffer.Append(unquotedTextToAppend);
                if (unquotedTextToAppend.EndsWith(@"\", StringComparison.Ordinal))
                {
                    buffer.Append('\\');
                }

                buffer.Append('"');
            }
        }

        private void AppendTextUnquoted(string textToAppend)
        {
            if (textToAppend != null)
            {
                this.CommandLine.Append(textToAppend);
            }
        }

        private void AppendTextWithQuoting(string textToAppend)
        {
            AppendQuotedTextToBuffer(this.CommandLine, textToAppend);
        }

        private void AppendFileNameWithQuoting(string fileName)
        {
            if (fileName != null)
            {
                if ((fileName.Length != 0) && (fileName[0] == '-'))
                {
                    this.AppendTextWithQuoting(@".\" + fileName);
                }
                else
                {
                    this.AppendTextWithQuoting(fileName);
                }
            }
        }

        private void AppendSpaceIfNotEmpty()
        {
            if (this.CommandLine.Length != 0)
            {
                this.CommandLine.Append(" ");
            }
        }
    }
}
