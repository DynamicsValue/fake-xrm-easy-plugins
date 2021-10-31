
param (
    [string]$targetFrameworks = "netcoreapp3.1"
)

./build.ps1 -targetFrameworks $targetFrameworks
./pack-push.ps1 -targetFrameworks $targetFrameworks -versionSuffix "zlocal"

