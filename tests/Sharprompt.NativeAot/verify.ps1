# 7.4 is the first version where $PSNativeCommandUseErrorActionPreference is not experimental,
# so a failing dotnet or native binary run stops the script.
#Requires -Version 7.4

<#
.SYNOPSIS
Verifies that an application consuming the Sharprompt package can be published and run with Native AOT.

.DESCRIPTION
Packs src/Sharprompt into artifacts/packages, publishes Sharprompt.NativeAot.App with PublishAot against that
package for each target framework, checks that the packaged source generators ran, and runs the native binary.
Any trim or AOT warning fails the publish, and any failed scenario fails the run.

.EXAMPLE
./tests/Sharprompt.NativeAot/verify.ps1

.EXAMPLE
./tests/Sharprompt.NativeAot/verify.ps1 -Framework net10.0
#>
[CmdletBinding()]
param(
    [ValidateSet('net8.0', 'net10.0')]
    [string[]] $Framework = @('net8.0', 'net10.0'),

    [string] $RuntimeIdentifier = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier,

    # A unique version per run, so NuGet never restores a stale package from its global cache.
    [string] $Version = "0.0.0-aot.$([DateTime]::UtcNow.ToString('yyyyMMddHHmmss'))"
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$artifacts = Join-Path $PSScriptRoot 'artifacts'
$packages = Join-Path $artifacts 'packages'
$appDirectory = Join-Path $PSScriptRoot 'Sharprompt.NativeAot.App'
$appProject = Join-Path $appDirectory 'Sharprompt.NativeAot.App.csproj'
$sharpromptProject = Join-Path $PSScriptRoot '../../src/Sharprompt/Sharprompt.csproj'

Write-Host "Packing Sharprompt $Version"

Remove-Item $packages -Recurse -Force -ErrorAction Ignore
dotnet pack $sharpromptProject -c Release -o $packages -p:Version=$Version

foreach ($tfm in $Framework) {
    $publishDirectory = Join-Path $artifacts "publish/$tfm/$RuntimeIdentifier"
    $generatedDirectory = Join-Path $appDirectory "obj/generated/$tfm"

    Write-Host "Publishing for $tfm ($RuntimeIdentifier)"

    Remove-Item $publishDirectory, $generatedDirectory -Recurse -Force -ErrorAction Ignore
    dotnet publish $appProject -c Release -f $tfm -r $RuntimeIdentifier -o $publishDirectory -p:SharpromptVersion=$Version

    # Bind<SignupModel> fails at runtime without a generated binder, but nothing observable distinguishes
    # generated enum metadata from the reflection fallback, so check the generated sources directly.
    $expectedSources = @('*SignupModel.Binder.g.cs', '*LocalPriority.*.EnumMetadata.g.cs', '*LocalTimeOfDay.*.EnumMetadata.g.cs')

    foreach ($pattern in $expectedSources) {
        if (-not (Get-ChildItem $generatedDirectory -Recurse -Filter $pattern -ErrorAction Ignore)) {
            throw "The packaged source generators did not produce $pattern for $tfm."
        }
    }

    if (Get-ChildItem $generatedDirectory -Recurse -Filter '*External*' -ErrorAction Ignore) {
        throw "Enums in Sharprompt.NativeAot.External are expected to use the reflection fallback, but metadata was generated for them."
    }

    $executable = Join-Path $publishDirectory ($IsWindows ? 'Sharprompt.NativeAot.App.exe' : 'Sharprompt.NativeAot.App')

    Write-Host "Running $executable"

    & $executable
}
