//-----------------------------------------------------------------------
// <copyright file="VSVersionInternal.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.VisualStudio
{
    /// <summary>
    /// Represents the several Visual Studio versions.
    /// <para>
    /// Internally the value is stored as the VS internal number multiplied by 10 (eg: VS2010 is 100 since it's internal version number is 10.0).
    /// </para>
    /// It also contains two reserved versions one for versious previous do VS .Net 2002 and another for versions we do not know about (versions
    /// above VS 2012)
    /// </summary>
    internal enum VSVersionInternal
    {
        /// <summary>
        /// Visual Studio 2012
        /// </summary>
        VS2012 = 110,

        /// <summary>
        /// Visual Studio Dev 11 DEPRECATED. Not deleted so we don't break existing process templates.
        /// </summary>
        VS11 = 110,

        /// <summary>
        /// Visual Studio 2010
        /// </summary>
        VS2010 = 100,

        /// <summary>
        /// Visual Studio 2008
        /// </summary>
        VS2008 = 90,

        /// <summary>
        /// Visual Studio 2005
        /// </summary>
        VS2005 = 80,

        /// <summary>
        /// Visual Studio 2003
        /// </summary>
        VSNet2003 = 71,

        /// <summary>
        /// Visual Studio .Net 2002
        /// </summary>
        VSNet2002 = 70,

        /// <summary>
        /// Previous unregonized version. 
        /// Any version previous to Visual Studio .Net 2002
        /// </summary>
        Previous = 0,

        /// <summary>
        /// Next Version whose number is still not known.
        /// Versions above VS 2010
        /// </summary>
        VSNext = 999,
        
        /// <summary>
        /// Let the system decide which version to use.
        /// </summary>
        Auto = 998
    }
}
