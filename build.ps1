
$localPackagesFolder = '../local-packages'
Write-Host "Checking if local packages folder '$($localPackagesFolder)' exists..."

$packagesFolderExists = Test-Path $localPackagesFolder -PathType Container

if(!($packagesFolderExists)) 
{
    New-Item $localPackagesFolder -ItemType Directory
}

dotnet restore
dotnet build --configuration Debug --no-restore
dotnet test --configuration Debug --no-restore --verbosity normal --collect:"XPlat code coverage" --settings tests/.runsettings

Write-Host "Succeeded :)"