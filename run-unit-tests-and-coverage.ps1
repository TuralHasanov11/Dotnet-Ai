# Run unit tests with coverage
dotnet test --settings unit-test.runsettings.xml

# Find the latest Cobertura file for unit tests
$unitCoverageFile = Get-ChildItem -Path "TestResults\Unit" -Recurse -Filter *.cobertura.xml | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($unitCoverageFile) {
    dotnet "$env:UserProfile\.nuget\packages\reportgenerator\5.5.0\tools\net9.0\ReportGenerator.dll" `
        -reports:"$($unitCoverageFile.FullName)" `
        -targetdir:unittestcoveragereport
} else {
    Write-Host "No unit test coverage file found."
}