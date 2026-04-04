# Run integration tests with coverage
dotnet test --solution "./DotnetAi.sln" --filter-trait "Category=Integration" --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml --results-directory "tests\TestResults\Integration"

# # Find the latest Cobertura file for integration tests
$integrationCoverageFile = Get-ChildItem -Path "tests\TestResults\Integration" -Recurse -Filter *.cobertura.xml | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($integrationCoverageFile) {
    ReportGenerator -reports:"$($integrationCoverageFile.FullName)" -targetdir:"tests\CoverageResults\Integration"
} else {
    Write-Host "No integration test coverage file found."
}