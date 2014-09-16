##-----------------------------------------------------------------------
## <copyright file="CleanDrop.ps1">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
##-----------------------------------------------------------------------
<#
.SYNOPSIS
    Cleans the binaries directory of unneeded project directories.

.DESCRIPTION
    The CleanDrop.ps1 script cleans the binaries directory of unneeded projet directories. This 
    script is meant to be use with outputs generated using the GenerateProjectSpecificOutputFolder 
    MSBuild parameter.
    If a project is a web site, the web site content inside _PublishedWebsites directory will be moved 
    to the project root directory.

.PARAMETER Include
    The directory names to keep separated by a comma. All other directories except logs will be deleted.

.PARAMETER Exclude
    The directory names to delete separated by a comma. All other directories will be kept.

.PARAMETER Disable
    Specifies whether the script is disabled and will take no action.
#>
[CmdletBinding(DefaultParameterSetName = 'Include')]
param(
    [Parameter(ParameterSetName = 'Include')]
    [string] $Include,
    [Parameter(ParameterSetName = 'Exclude')]
    [string] $Exclude,
    [switch] $Disable
)

#-----------------------------------------------------------------------------
# GLOBAL VARIABLES

$script:excludedDirectories = @()
$script:webSitesDirectories = @()

#-----------------------------------------------------------------------------
# INTERNAL FUNCTIONS

# checks if specified directory contains a web site
function TestWebSiteDirectory
{
    param(
        [string] $Path
    )

    if (Test-Path (Join-Path $Path '_PublishedWebsites'))
    {
        Write-Verbose "Web sites directory '${Path}'."
        $script:webSitesDirectories += $Path
    }
}

# get excluded directories based on specified directory names to keep.
function ProcessIncludeDirectories
{
    param(
        [string] $Path,
        [string[]] $Includes
    )

    if ($Includes -icontains (Split-Path $Path -Leaf))
    {
        # current directory included
        TestWebSiteDirectory $Path

        return $true
    }

    # check for child directory included
    $includeChild = $false
    $excludedChildren = @()
    Get-ChildItem $Path -Directory | foreach {
        if (-not (ProcessIncludeDirectories -Path $_.FullName -Includes $Includes))
        {
            $excludedChildren += $_.FullName
        }
        else
        {
            $includeChild = $true
        }
    }

    if ($includeChild)
    {
        # exclude none included children
        $excludedChildren | foreach {
            Write-Verbose "Exclude directory '$_'."
            $script:excludedDirectories += $_
        }
    }

    return $includeChild
}

# get excluded directories based on specified directory names to exclude.
function ProcessExcludeDirectories
{
    param(
        [string] $Path,
        [string[]] $Excludes
    )

    if ($Excludes -icontains (Split-Path $Path -Leaf))
    {
        # current directory exclude
        Write-Verbose "Exclude directory '${Path}'."
        $script:excludedDirectories += $Path

        return
    }

    # process child directories
    TestWebSiteDirectory $Path

    Get-ChildItem $Path -Directory | foreach {
        ProcessExcludeDirectories -Path $_.FullName -Excludes $Excludes
    }
}

#-----------------------------------------------------------------------------
# MAIN

if ($Disable)
{
    Write-Verbose "Script disabled; no action will be taken."
}

# if the script is not running on a build server, remind user to set environment variables
if (-not $env:TF_BUILD -or -not $env:TF_BUILD_BINARIESDIRECTORY)
{
    Write-Error 'You must set the following environement variables to test this script interactively: $env:TF_BUILD, $env:TF_BUILD_BINARIESDIRECTORY.'

    exit 1
}

# make sure path to binaries output directory is available
if (-not (Test-Path $env:TF_BUILD_BINARIESDIRECTORY))
{
    Write-Error "Directory '${env:TF_BUILD_BINARIESDIRECTORY}' doesn't exists."

    exit 1
}

# find excluded directories and included web site directories
switch ($PSCmdlet.ParameterSetName)
{
    'Include' {
        $includes = $Include.Split(',')
        $includes += 'logs'

        ProcessIncludeDirectories -Path $env:TF_BUILD_BINARIESDIRECTORY -Includes $includes | Out-Null
    }
    'Exclude' {
        ProcessExcludeDirectories -Path $env:TF_BUILD_BINARIESDIRECTORY -Excludes ($Exclude.Split(',')) | Out-Null
    }
}

# delete excluded directories
if (-not $Disable -and $excludedDirectories.Length)
{
    $excludedDirectories | Remove-Item -Recurse -Force

    Write-Verbose "Directories deleted."
}

# clean web sites directories
if (-not $Disable -and $webSitesDirectories.Length)
{
    $webSitesDirectories | foreach {
        $path = $_
        $publishPath = Join-Path $path '_PublishedWebsites'

        Get-ChildItem $path -Exclude '_PublishedWebsites' | Remove-Item -Recurse -Force

        Get-ChildItem $publishPath -Directory | foreach {
            Move-Item (Join-Path $_.FullName '*') $path -Force
        }

        Remove-Item $publishPath -Recurse -Force
    }

    Write-Verbose "Web site directories cleaned."
}