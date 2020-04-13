param (
    [string]$packageSource = "local-packages",
    [string]$versionSuffix = ""
 )


Write-Host "Running with packageSource '$($packageSource)' and versionSuffix '$($versionSuffix)'..."

$tempNupkgFolder = './nupkgs'
Write-Host "Checking if temp nupkgs folder '$($localPackagesFolder)' exists..."

$tempNupkgFolderExists = Test-Path $tempNupkgFolder -PathType Container

if(!($tempNupkgFolderExists)) 
{
    New-Item $tempNupkgFolder -ItemType Directory
}

Write-Host "Deleting temporary nupkgs..."
Get-ChildItem -Path $tempNupkgFolder -Include *.* -File -Recurse | ForEach-Object { $_.Delete()}

Write-Host "Packing assembly..."
if($versionSuffix -eq "") 
{
    dotnet pack -o $tempNupkgFolder src/FakeXrmEasy.Plugins/FakeXrmEasy.Plugins.csproj
}
else {
    dotnet pack -o $tempNupkgFolder src/FakeXrmEasy.Plugins/FakeXrmEasy.Plugins.csproj /p:VersionSuffix=$versionSuffix
}
if(!($LASTEXITCODE -eq 0)) {
    throw "Error when packing the assembly"
}

Write-Host "Pushing FakeXrmEasy.Plugins to local folder..."
dotnet nuget push $tempNupkgFolder/*.nupkg -s $packageSource
if(!($LASTEXITCODE -eq 0)) {
    throw "Error pushing NuGet package"
}

Write-Host "Succeeded :)" -ForegroundColor Green