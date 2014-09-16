##-----------------------------------------------------------------------
## <copyright file="GatherItemsForDrop.ps1">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
##-----------------------------------------------------------------------
# Copy the binaries to the bin directory 
# so that the build server can drop them
# to the staging location specified on the Build Defaults tab 
#
# See 
#	http://msdn.microsoft.com/en-us/library/bb778394(v=vs.120).aspx
#	http://msdn.microsoft.com/en-us/library/dd647547(v=vs.120).aspx#scripts 	
	
# Enable -Verbose option
[CmdletBinding()]
	
# Disable parameter
# Convenience option so you can debug this script or disable it in 
# your build definition without having to remove it from
# the 'Post-build script path' build process parameter.
param([switch]$Disable)
if ($PSBoundParameters.ContainsKey('Disable'))
{
	Write-Verbose "Script disabled; no actions will be taken on the files."
}
	
# This script copies the basic file types for managed code projects.
# You can change this list to meet your needs.
$FileTypes = $("*.exe","*.dll","*.exe.config","*.pdb")
	
# Specify the sub-folders to include
$SourceSubFolders = $("*bin*","*obj*")
	
# If this script is not running on a build server, remind user to 
# set environment variables so that this script can be debugged
if(-not $Env:TF_BUILD -and -not ($Env:TF_BUILD_SOURCESDIRECTORY -and $Env:TF_BUILD_BINARIESDIRECTORY))
{
	Write-Error "You must set the following environment variables"
	Write-Error "to test this script interactively."
	Write-Host '$Env:TF_BUILD_SOURCESDIRECTORY - For example, enter something like:'
	Write-Host '$Env:TF_BUILD_SOURCESDIRECTORY = "C:\code\FabrikamTFVC\HelloWorld"'
	Write-Host '$Env:TF_BUILD_BINARIESDIRECTORY - For example, enter something like:'
	Write-Host '$Env:TF_BUILD_BINARIESDIRECTORY = "C:\code\bin"'
	exit 1
}
	
# Make sure path to source code directory is available
if (-not $Env:TF_BUILD_SOURCESDIRECTORY)
{
	Write-Error ("TF_BUILD_SOURCESDIRECTORY environment variable is missing.")
	exit 1
}
elseif (-not (Test-Path $Env:TF_BUILD_SOURCESDIRECTORY))
{
	Write-Error "TF_BUILD_SOURCESDIRECTORY does not exist: $Env:TF_BUILD_SOURCESDIRECTORY"
	exit 1
}
Write-Verbose "TF_BUILD_SOURCESDIRECTORY: $Env:TF_BUILD_SOURCESDIRECTORY"
	
# Make sure path to binary output directory is available
if (-not $Env:TF_BUILD_BINARIESDIRECTORY)
{
	Write-Error ("TF_BUILD_BINARIESDIRECTORY environment variable is missing.")
	exit 1
}
if ([IO.File]::Exists($Env:TF_BUILD_BINARIESDIRECTORY))
{
	Write-Error "Cannot create output directory."
    Write-Error "File with name $Env:TF_BUILD_BINARIESDIRECTORY already exists."
	exit 1
}
Write-Verbose "TF_BUILD_BINARIESDIRECTORY: $Env:TF_BUILD_BINARIESDIRECTORY"
	
# Tell user what script is about to do
Write-Verbose "Will look for and then gather "
Write-Verbose "$FileTypes files from"
Write-Verbose "$Env:TF_BUILD_SOURCESDIRECTORY and copy them to "
Write-Verbose $Env:TF_BUILD_BINARIESDIRECTORY
	
# Find the files
$files = gci $Env:TF_BUILD_SOURCESDIRECTORY -recurse -include $SourceSubFolders | 
	?{ $_.PSIsContainer } | 
	foreach { gci -Path $_.FullName -Recurse -include $FileTypes }
if($files)
{
	Write-Verbose "Found $($files.count) files:"
		
	foreach ($file in $files) {
		Write-Verbose $file.FullName 
	}
}
else
{
	Write-Warning "Found no files."
}
	
# If binary output directory exists, make sure it is empty
# If it does not exist, create one
# (this happens when 'Clean workspace' build process parameter is set to True)
if ([IO.Directory]::Exists($Env:TF_BUILD_BINARIESDIRECTORY)) 
{ 
	$DeletePath = $Env:TF_BUILD_BINARIESDIRECTORY + "\*"
	Write-Verbose "$Env:TF_BUILD_BINARIESDIRECTORY exists."
	if(-not $Disable)
	{
		Write-Verbose "Ready to delete $DeletePath"
		Remove-Item $DeletePath -recurse
		Write-Verbose "Files deleted."
	}	
} 
else
{ 
	Write-Verbose "$Env:TF_BUILD_BINARIESDIRECTORY does not exist."
		
	if(-not $Disable)
	{
		Write-Verbose "Ready to create it."
		[IO.Directory]::CreateDirectory($Env:TF_BUILD_BINARIESDIRECTORY) | Out-Null
		Write-Verbose "Directory created."
	}
} 
	
# Copy the binaries 
Write-Verbose "Ready to copy files."
if(-not $Disable)
{
	foreach ($file in $files) 
	{
		Copy $file $Env:TF_BUILD_BINARIESDIRECTORY
	}
	Write-Verbose "Files copied."
}
