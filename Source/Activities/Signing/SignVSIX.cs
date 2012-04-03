//-----------------------------------------------------------------------
// <copyright file="SignVSIX.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.Signing
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Packaging;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;
    using Microsoft.TeamFoundation.Client;
    using TfsBuildExtensions.TfsUtilities;
using System.ComponentModel;

    /// <summary>
    /// Activity to digitally sign a VSIX package with an authenticode certificate
    /// <para>
    /// A VSIX can't be signed with the signtool.exe since it 
    /// is a file following the Open Packaging Convention 
    /// http://msdn.microsoft.com/en-us/magazine/cc163372.aspx) 
    /// and it doesn't has a SIP capabilities (http://blogs.technet.com/b/eduardonavarro/archive/2008/07/11/3087407.aspx)
    /// for the purpose of code signing.   
    /// </para>
    /// This code has been inspired by Jeff Wilcox's Blog Post http://www.jeff.wilcox.name/2010/03/vsixcodesigning/
    /// </summary>
    [ActivityTracking(ActivityTrackingOption.None)]
    [BuildActivity(HostEnvironmentOption.All)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VSIX")]
    public class SignVSIX : BaseCodeActivity<bool>
    {
        /// <summary>
        /// The path for the VSIX file to be signed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VSIX")]
        [RequiredArgument]
        [Description("The path for the VSIX file to sign")]
        public InArgument<string> VSIXFilePath { get; set; }

        /// <summary>
        /// The certificate file path. It can be a path on a disk or stored in 
        /// source control (fetched automatically)
        /// </summary>
        [RequiredArgument]
        [Description("The path of the certificate file (pfx format). Can be either a local path or a path in source control")]
        public InArgument<string> CertFilePath { get; set; }

        /// <summary>
        /// Certificate Password
        /// </summary>
        [RequiredArgument]
        [Description("The certificate password")]
        public InArgument<string> CertPassword { get; set; }

        /// <summary> 
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <returns>True if signing has been sucessful false otherwise</returns>
        protected override bool InternalExecute()
        {
            var vsixFileName = this.VSIXFilePath.Get(this.ActivityContext);
            var certificatePassword = this.CertPassword.Get(this.ActivityContext);

            using (var autoTracker = new AutoFileTrackerFromSourceControl(this.ActivityContext.GetExtension<TfsTeamProjectCollection>()))
            {
                var certficateFile = autoTracker.GetFile(this.CertFilePath.Get(this.ActivityContext));

                return this.SignVSIXFile(vsixFileName, certficateFile, certificatePassword);
            }
        }       

        /// <summary>
        /// Signs all parts of the VSIX file
        /// </summary>
        /// <param name="VSIXFileName">The file path of the VSIX file</param>
        /// <param name="pfxFileName">The file path of the certificate</param>
        /// <param name="password">The password for the certificate</param>
        /// <returns>True if the file has  been signed, false otherwise</returns>
        private bool SignVSIXFile(string VSIXFileName, string pfxFileName, string password)
        {
            if (File.Exists(VSIXFileName) == false)
            {
                this.LogBuildError("VSIX file doesn't exist");
                return false;
            }

            if (File.Exists(pfxFileName) == false)
            {
                this.LogBuildError("Certificate file doesn't exist");
                return false;
            }

            LogBuildMessage(String.Format("Signing {0} ", Path.GetFileName(VSIXFileName)), BuildMessageImportance.High);

            using (var package = Package.Open(VSIXFileName, FileMode.Open))
            {
                var packageSignatureManager = new PackageDigitalSignatureManager(package) { CertificateOption = CertificateEmbeddingOption.InSignaturePart };

                var partsToSign = new List<Uri>();
                foreach (var packagePart in package.GetParts())
                {
                    partsToSign.Add(packagePart.Uri);
                }

                partsToSign.Add(PackUriHelper.GetRelationshipPartUri(packageSignatureManager.SignatureOrigin));
                partsToSign.Add(packageSignatureManager.SignatureOrigin);
                partsToSign.Add(PackUriHelper.GetRelationshipPartUri(new Uri("/", UriKind.RelativeOrAbsolute)));

                try
                {
                    packageSignatureManager.Sign(partsToSign, new System.Security.Cryptography.X509Certificates.X509Certificate2(pfxFileName, password));
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    this.LogBuildError("Error Signing File " + ex.Message);

                    return false;
                }

                return packageSignatureManager.IsSigned && packageSignatureManager.VerifySignatures(true) == VerifyResult.Success;
            }
        }
    }
}