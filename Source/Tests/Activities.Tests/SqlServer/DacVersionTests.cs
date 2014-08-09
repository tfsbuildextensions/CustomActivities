//-----------------------------------------------------------------------
// <copyright file="SqlExecuteTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.SqlServer;

    [TestClass]
    public class DacVersionTests
    {
        // test SqlServerProj file prefix.
        private const string TestFilePrefix = @"TestDatabase";

        /// <summary>
        /// Gets the contents of a database project file.
        /// </summary>
        private string ProjectFileContents
        {
            get
            {
                return @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <Name>Product.Database</Name>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectVersion>4.1</ProjectVersion>
    <ProjectGuid>{557C0A9C-504C-444F-AAD8-FA9A34FD455C}</ProjectGuid>
    <DSP>Microsoft.Data.Tools.Schema.Sql.Sql100DatabaseSchemaProvider</DSP>
    <OutputType>Database</OutputType>
    <RootPath>
    </RootPath>
    <RootNamespace>Product.Database</RootNamespace>
    <AssemblyName>Product.Database</AssemblyName>
    <ModelCollation>1039,CI</ModelCollation>
    <DefaultFileStructure>BySchemaAndSchemaType</DefaultFileStructure>
    <DeployToDatabase>True</DeployToDatabase>
    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
    <TargetLanguage>CS</TargetLanguage>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <SqlServerVerification>False</SqlServerVerification>
    <IncludeCompositeObjects>True</IncludeCompositeObjects>
    <TargetDatabaseSet>True</TargetDatabaseSet>
    <SccProjectName>SAK</SccProjectName>
    <SccProvider>SAK</SccProvider>
    <SccAuxPath>SAK</SccAuxPath>
    <SccLocalPath>SAK</SccLocalPath>
    <DefaultCollation>English_CI_AS</DefaultCollation>
    <DefaultFilegroup>PRIMARY</DefaultFilegroup>
    <DacVersion>1.0.0.0</DacVersion>
    <AllowSnapshotIsolation>True</AllowSnapshotIsolation>
    <ReadCommittedSnapshot>True</ReadCommittedSnapshot>
    <IsChangeTrackingOn>True</IsChangeTrackingOn>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <OutputPath>bin\Release\</OutputPath>
    <BuildScriptName>$(MSBuildProjectName).sql</BuildScriptName>
    <TreatWarningsAsErrors>False</TreatWarningsAsErrors>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <DefineDebug>false</DefineDebug>
    <DefineTrace>true</DefineTrace>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <OutputPath>bin\Debug\</OutputPath>
    <BuildScriptName>$(MSBuildProjectName).sql</BuildScriptName>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <DefineDebug>true</DefineDebug>
    <DefineTrace>true</DefineTrace>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <!-- VS10 without SP1 will not have VisualStudioVersion set, so do that here -->
  <PropertyGroup>
    <VisualStudioVersion Condition=""'$(VisualStudioVersion)' == ''"">10.0</VisualStudioVersion>
    <!-- Default to the v10.0 targets path if the targets file for the current VS version is not found -->
    <SSDTExists Condition=""Exists('$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets')"">True</SSDTExists>
    <VisualStudioVersion Condition=""'$(SSDTExists)' == ''"">10.0</VisualStudioVersion>
  </PropertyGroup>
  <Import Project=""$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets"" />
  <ItemGroup>
    <Folder Include=""Properties"" />
  </ItemGroup>
  <ItemGroup>
    <ArtifactReference Include=""$(DacPacRootPath)\Extensions\Microsoft\SQLDB\Extensions\SqlServer\100\SqlSchemas\master.dacpac"">
      <HintPath>$(DacPacRootPath)\Extensions\Microsoft\SQLDB\Extensions\SqlServer\100\SqlSchemas\master.dacpac</HintPath>
      <SuppressMissingDependenciesErrors>False</SuppressMissingDependenciesErrors>
      <DatabaseVariableLiteralValue>master</DatabaseVariableLiteralValue>
    </ArtifactReference>
  </ItemGroup>
</Project>";
            }
        }

        [TestMethod]
        [DeploymentItem(@"Framework\TestFiles\Database.sqlproj")]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void SqlServerProj_UpdatesDacVersion_WhenExecuteInvoked()
        {
            // Create a temp file and write some dummy attribute to it
            var fileName = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName() + ".sqlproj";
            FileInfo f = new FileInfo(fileName);
            File.WriteAllText(f.FullName, this.ProjectFileContents);

            var target = new DacVersion{ SqlProjFilePath = f.FullName, Version = "1.0.156.3" };
            var invoker = new WorkflowInvoker(target);

            // act
            invoker.Invoke();

            // assert
            var text = File.ReadAllText(f.FullName);
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("<DacVersion>1.0.156.3</DacVersion>", DateTime.Today), StringComparison.Ordinal));
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void SqlServerProj_UpdateDacVersion_UsingFileList()
        {
            // Create a temp file and write some dummy attribute to it
            var fileName = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName() + ".sqlproj";
            FileInfo f = new FileInfo(fileName);
            File.WriteAllText(f.FullName, this.ProjectFileContents);

            var target = new DacVersion();

            var parameters = new Dictionary<string, object>
            { 
                { "Files", new[] { f.FullName } },
                { "Version", "1.0.156.3"}
            };

            var invoker = new WorkflowInvoker(target);

            // act
            invoker.Invoke(parameters);

            // assert
            var text = File.ReadAllText(f.FullName);
            Assert.AreNotEqual(-1, text.IndexOf(string.Format("<DacVersion>1.0.156.3</DacVersion>", DateTime.Today), StringComparison.Ordinal));
        }
    }
}
