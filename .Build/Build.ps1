[CmdletBinding()]
Param(
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity,
    [string]$Script = "Build.cake",
    [string]$Target,
    [string]$Configuration,

    [switch]$ShowDescription,
    [switch]$ShowTree,
    [switch]$DryRun,
    [switch]$Exclusive,
    
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

# Restore dotnet tools
Push-Location
Set-Location "$PSScriptRoot\.."
dotnet tool restore
Pop-Location

# Make path absolute...required by dotnet-tool-cake
$Script = "$PSScriptRoot\$Script"

# Build Cake arguments
$cakeArguments = @("$Script");
if ($Target) { $cakeArguments += "--target=$Target" }
if ($Configuration) { $cakeArguments += "-configuration=$Configuration" }
if ($Verbosity) { $cakeArguments += "--verbosity=$Verbosity" }
if ($ShowDescription) { $cakeArguments += "--showdescription" }
if ($Debug) { $cakeArguments += "--debug" }
if ($ShowTree) { $cakeArguments += "--showtree" }
if ($Exclusive) { $cakeArguments += "--exclusive" }
if ($DryRun) { $cakeArguments += "--dryrun" }
$cakeArguments += "--settings_skippackageversioncheck=true"
$cakeArguments += "--settings_enablescriptcache=true"
# $cakeArguments += "--paths_cache=C:\temp\cake_cache\ArgosApi"
$cakeArguments += $ScriptArgs

$start = [DateTime]::Now.ToString( "o" )
Write-Host "Running build script $start ..."

dotnet cake $cakeArguments
exit $LASTEXITCODE