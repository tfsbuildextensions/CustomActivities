//-----------------------------------------------------------------------
// <copyright file="ArgumentValidation.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.TfsUtilities
{
    using System;

    /// <summary>
    /// Enumeration of TFS Uri types.
    /// </summary>
    public enum TfsUriType
    {
        /// <summary>
        /// Project Collection Uri
        /// </summary>
        ProjectCollection,

        /// <summary>
        /// Build Server Uri
        /// </summary>
        BuildServer,

        /// <summary>
        /// Individual build Uri
        /// </summary>
        Build,

        /// <summary>
        /// Build definition Uri
        /// </summary>
        BuildDefinition,

        /// <summary>
        /// TF Service Uri
        /// </summary>
        Service
    }

    /// <summary>
    /// Provides basic validation. Throws an exception if validation fails.
    /// </summary>
    public static class ArgumentValidation
    {
        /// <summary>
        /// Validates that an object is not null. 
        /// </summary>
        /// <param name="argumentValue">Value of the argument</param>
        /// <param name="argumentName">Name of the argument</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentNullException"/>
        public static void ValidateObjectIsNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName, "Argument cannot be null");
            }
        }

        /// <summary>
        /// Validates that a string argument has a value.
        /// </summary>
        /// <param name="argumentValue">Value of the argument</param>
        /// <param name="argumentName">Name of the argument</param>
        /// <exception cref="System.ArgumentException"/>
        public static void ValidateStringIsNotEmpty(string argumentValue, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argumentValue))
            {
                throw new ArgumentException("Argument cannot be null or empty string", argumentName);
            }
        }

        /// <summary>
        /// Validates that a supplied URI has a value and the proper format and scheme.  
        /// </summary>
        /// <param name="argumentValue">Value of the argument</param>
        /// <param name="argumentName">Name of the argument</param>
        /// <param name="uriType">Type of Uri to validate</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        public static void ValidateTfsUri(System.Uri argumentValue, string argumentName, TfsUriType uriType)
        {
            ValidateObjectIsNotNull(argumentValue, argumentName);
            try
            {
                bool throwException = false;
                string exceptionMessage = string.Empty; 
                switch (uriType)
                {
                    case TfsUriType.ProjectCollection:
                        if (argumentValue.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) || argumentValue.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                        {
                            throwException = true;
                            exceptionMessage = "Project collection Uri scheme is not valid.";
                        }

                        if (argumentValue.Segments.Length != 1)
                        {
                            throwException = true;
                            exceptionMessage = "Project collection Uri requires a project collection to be specified.";
                        }

                        break;
                    case TfsUriType.BuildServer:
                        break;
                    case TfsUriType.Build:
                        break;
                    case TfsUriType.BuildDefinition:
                        break;
                    case TfsUriType.Service:
                        break;
                    default:
                        break;
                }

                if (throwException)
                {
                    throw new ArgumentException(exceptionMessage, argumentName);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Uri is invalid", argumentName, ex);
            }
        }
    }
}
