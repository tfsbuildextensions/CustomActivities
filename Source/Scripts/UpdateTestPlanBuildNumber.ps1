##-----------------------------------------------------------------------
## <copyright file="UpdateTestPlanBuildNumber.ps1">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
##-----------------------------------------------------------------------
# Update all test plan using the current build definition with the latest build number. 
#
	
# Enable -Verbose option
[CmdletBinding()]
	
# Disable parameter
# Convenience option so you can debug this script or disable it in 
# your build definition without having to remove it from
# the 'Post-build script path' build process parameter.
#
# Username/password parameters
# Optional parameters to authenticate against TFS/VSO
# if a Personal Access Token is to be used (preferred option) 
# then leave username blank.
param([switch]$Disable, $username, $password)

if ($PSBoundParameters.ContainsKey('Disable'))
{
	Write-Verbose "Script disabled; no actions will be taken on the files."
}
	
# If this script is not running on a build server, remind user to 
# set environment variables so that this script can be debugged
if(-not $Env:TF_BUILD -and -not ($Env:TF_BUILD_BUILDURI -and $Env:TF_BUILD_BUILDDEFINITIONNAME -and $Env:TF_BUILD_COLLECTIONURI))
{
	Write-Error "You must set the following environment variables"
	Write-Error "to test this script interactively."
	Write-Error '$Env:TF_BUILD_BUILDURI - For example, enter something like:'
	Write-Error '$Env:TF_BUILD_BUILDURI = "vstfs:///Build/Build/15"'
	Write-Error '$Env:TF_BUILD_BUILDDEFINITIONNAME - For example, enter something like:'
	Write-Error '$Env:TF_BUILD_BUILDDEFINITIONNAME = "MyProduct.Main.CI"'
	Write-Error '$Env:TF_BUILD_COLLECTIONURI - For example, enter something like:'
	Write-Error '$Env:TF_BUILD_COLLECTIONURI = "http://localhost:8080/tfs/DefaultCollection"'
	exit 1
}
	
# Make sure build uri is set
if (-not $Env:TF_BUILD_BUILDURI)
{
	Write-Error ("TF_BUILD_BUILDURI environment variable is missing.")
	exit 1
}
Write-Verbose "TF_BUILD_BUILDURI: $Env:TF_BUILD_BUILDURI"

# Make sure build definition name is set
if (-not $Env:TF_BUILD_BUILDURI)
{
	Write-Error ("TF_BUILD_BUILDDEFINITIONNAME environment variable is missing.")
	exit 1
}
Write-Verbose "TF_BUILD_BUILDDEFINITIONNAME: $Env:TF_BUILD_BUILDDEFINITIONNAME"

# Make sure tfs collection uri is set
if (-not $Env:TF_BUILD_COLLECTIONURI)
{
	Write-Error ("TF_BUILD_COLLECTIONURI environment variable is missing.")
	exit 1
}
Write-Verbose "TF_BUILD_BUILDDEFINITIONNAME: $Env:TF_BUILD_BUILDDEFINITIONNAME"

[Reflection.Assembly]::LoadWithPartialName('Microsoft.TeamFoundation.Client')
[Reflection.Assembly]::LoadWithPartialName('Microsoft.TeamFoundation.TestManagement.Client')
[Reflection.Assembly]::LoadWithPartialName('Microsoft.TeamFoundation.Build.Client')

# Find all test plans using this build definition
if (-not $password)
{
	Write-Verbose "Authenticating using service identity."
	$tpc = New-Object Microsoft.TeamFoundation.Client.TfsTeamProjectCollection($env:TF_BUILD_COLLECTIONURI)
}
else
{
	Write-Verbose "Authenticating with credentials."
	$netCred = New-Object System.Net.NetworkCredential($username,$password)
	$basicCred = New-Object Microsoft.TeamFoundation.Client.BasicAuthCredential($netCred)
	$tfsCred = New-Object Microsoft.TeamFoundation.Client.TfsClientCredentials($basicCred)
	$tfsCred.AllowInteractive = $false
	$tpc = New-Object Microsoft.TeamFoundation.Client.TfsTeamProjectCollection($env:TF_BUILD_COLLECTIONURI,$tfsCred)
	$tpc.EnsureAuthenticated()
}
$tcm = $tpc.GetService([Microsoft.TeamFoundation.TestManagement.Client.ITestManagementService])
$buildServer = $tpc.GetService([Microsoft.TeamFoundation.Build.Client.IBuildServer])
$teamProject = $buildServer.GetBuild($Env:TF_BUILD_BUILDURI);
$testProject = $tcm.GetTeamProject($teamProject.TeamProject);
$testPlans = $testProject.TestPlans.Query("SELECT * FROM TestPlan")

$matchingTestPlans = @()
foreach($testPlan in $testPlans)
{
    if($testPlan.BuildFilter.BuildDefinition -eq $Env:TF_BUILD_BUILDDEFINITIONNAME)
    {
        $matchingTestPlans += $testPlan
    }
}

# Update test plans with latest build
if($matchingTestPlans)
{
	Write-Host "Will update test plans using $Env:TF_BUILD_BUILDDEFINITIONNAME to $Env:TF_BUILD_BUILDURI to $($matchingTestPlans.count) test plans."
	
	foreach ($matchingTestPlan in $matchingTestPlans) {
		if(-not $Disable)
		{
			$matchingTestPlan.BuildUri = $Env:TF_BUILD_BUILDURI
			$matchingTestPlan.Save()
			Write-Verbose "$matchingTestPlan.Name - version applied"
		}
	}
}
else
{
	Write-Warning "Found no test plans to update."
}
