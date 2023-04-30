param (
    [string]$folderPath = "./src/FakeXrmEasy.Plugins/bin"
)

Get-ChildItem -Path $folderPath -Include * -File -Recurse | foreach { $_.Delete()}
