//-----------------------------------------------------------------------
// <copyright file="ResolvePowerShellScriptTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests.Scripting
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TfsBuildExtensions.Activities.Scripting;

    [TestClass]
    public class ResolvePowerShellScriptTests
    {
        [TestMethod]
        public void A_valid_server_path_generates_a_cmd_with_arguments()
        {
            // Arrange
            var fakeTfsProvider = new Moq.Mock<IUtilitiesForPowerShellActivity>();
            fakeTfsProvider.Setup(f => f.IsServerItem(It.IsAny<string>())).Returns(true);
            fakeTfsProvider.Setup(f => f.GetLocalFilePathFromWorkspace(null, It.IsAny<string>())).Returns(@"c:\serverfile\script.ps1");
            fakeTfsProvider.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);

            var activity = new InvokePowerShellCommand(fakeTfsProvider.Object);

            // Act
            var actual = activity.ResolveScript(null, "$/Test Path/Not A Real Path", "-myarg");

            // assert
            Assert.AreEqual(@"& 'c:\serverfile\script.ps1' -myarg", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.FileNotFoundException))]
        public void If_a_valid_server_path_that_cannot_be_found_locally_throws_exception()
        {
            // Arrange
            var fakeTfsProvider = new Moq.Mock<IUtilitiesForPowerShellActivity>();
            fakeTfsProvider.Setup(f => f.IsServerItem(It.IsAny<string>())).Returns(true);
            fakeTfsProvider.Setup(f => f.GetLocalFilePathFromWorkspace(null, It.IsAny<string>())).Returns(@"c:\serverfile\script.ps1");
            fakeTfsProvider.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            var activity = new InvokePowerShellCommand(fakeTfsProvider.Object);

            // Act
            var actual = activity.ResolveScript(null, "$/Test Path/Not A Real Path", "-myarg");

            // assert
            // checked with attribute
        }

        [TestMethod]
        public void An_invalid_server_path_is_treated_as_a_filesystem_script_file_and_arguments_appended()
        {
            // Arrange
            var fakeTfsProvider = new Moq.Mock<IUtilitiesForPowerShellActivity>();
            fakeTfsProvider.Setup(f => f.IsServerItem(It.IsAny<string>())).Returns(false);
            fakeTfsProvider.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);

            var activity = new InvokePowerShellCommand(fakeTfsProvider.Object);

            // Act
            var actual = activity.ResolveScript(null, @"c:\localscript.ps1", "-myarg");

            // assert
            Assert.AreEqual(@"& 'c:\localscript.ps1' -myarg", actual);
        }

        [TestMethod]
        public void An_invalid_server_path_is_treated_as_a_script_and_arguments_ignored_if_local_path_that_cannot_be_found()
        {
            // Arrange
            var fakeTfsProvider = new Moq.Mock<IUtilitiesForPowerShellActivity>();
            fakeTfsProvider.Setup(f => f.IsServerItem(It.IsAny<string>())).Returns(false);
            fakeTfsProvider.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            var activity = new InvokePowerShellCommand(fakeTfsProvider.Object);

            // Act
            var actual = activity.ResolveScript(null, @"some powershell commands", "-myarg");

            // assert
            Assert.AreEqual(@"some powershell commands", actual);
        }
    }
}
