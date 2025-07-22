param (
    [string]$versionSuffix = "",
    [string]$targetFrameworks = "net6.0",
    [string]$packageIdPrefix = "FakeXrmEasy.PluginsTests",
    [string]$projectName = "FakeXrmEasy.Plugins.Tests",
    [string]$projectPath = "tests/FakeXrmEasy.Plugins.Tests",
    [string]$packTests = "true"
 )

Write-Host "Running with versionSuffix '$($versionSuffix)'..."

$packageIdPrefix = "FakeXrmEasy.PluginsTests"
$projectName = "FakeXrmEasy.Plugins.Tests"
$projectPath = "tests/FakeXrmEasy.Plugins.Tests"

Write-Host "Packing All Configurations for project $($projectName)" -ForegroundColor Green

./pack-configuration.ps1 -targetFrameworks $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -packTests $packTests -configuration "FAKE_XRM_EASY_2013"
./pack-configuration.ps1 -targetFrameworks $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -packTests $packTests -configuration "FAKE_XRM_EASY_2015"
./pack-configuration.ps1 -targetFrameworks $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -packTests $packTests -configuration "FAKE_XRM_EASY_2016"
./pack-configuration.ps1 -targetFrameworks $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -packTests $packTests -configuration "FAKE_XRM_EASY_365"
./pack-configuration.ps1 -targetFrameworks $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -packTests $packTests -configuration "FAKE_XRM_EASY_9"

Write-Host "Pack Succeeded  :)" -ForegroundColor Green