# Run unit tests with coverage
dotnet test --solution "./DotnetAi.sln" --filter-trait "Category=Unit" --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml --results-directory "tests\TestResults\Unit"

# # Find the latest Cobertura file for unit tests
$unitCoverageFile = Get-ChildItem -Path "tests\TestResults\Unit" -Recurse -Filter *.cobertura.xml | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($unitCoverageFile) {
    ReportGenerator -reports:"$($unitCoverageFile.FullName)" -targetdir:"tests\CoverageResults\Unit"
} else {
    Write-Host "No unit test coverage file found."
}