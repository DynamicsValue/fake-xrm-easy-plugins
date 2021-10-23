
param (
    [string]$targetFramework = "netcoreapp3.1"
)

./build.ps1 -targetFramework $targetFramework
./pack-push.ps1 -targetFrameworks $targetFramework -versionSuffix "zlocal"

