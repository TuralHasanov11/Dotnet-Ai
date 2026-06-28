### Init Spec
```sh
specify init --here
```

### Start docker composer with override file

```sh
docker compose -f docker-compose.yaml -f docker-compose.override.yaml up -d --build
```

### Export dev certificate for HTTPS (Windows PowerShell)
```sh
$certDir = Join-Path $env:APPDATA 'ASP.NET\Https'; New-Item -ItemType Directory -Path $certDir -Force | Out-Null; $pwd = [Convert]::ToBase64String((1..24 | ForEach-Object {Get-Random -Maximum 256} | ForEach-Object {[byte]$_})); $certPath = Join-Path $certDir '<project-name>.pfx'; dotnet dev-certs https -ep $certPath -p $pwd; Set-Content -Path (Join-Path $certDir '<project-name>.pfx.password.txt') -Value $pwd -NoNewline; Write-Output ("Exported dev cert to: $certPath")
```

### Install dotnetup on Windows
```powershell
iwr https://aka.ms/dotnetup/get-dotnetup.ps1 | iex
```

### Verify dotnetup setup
```powershell
dotnet --version
dotnetup list
```

### Test
```sh
dotnet test ./DotnetAi.slnx

./run-unit-tests-and-coverage.ps1
./run-integration-tests-and-coverage.ps1
./run-unit-tests-and-coverage.sh
./run-integration-tests-and-coverage.sh
```

### OpenSSF socorecard
```sh
docker run -e GITHUB_AUTH_TOKEN=<your access token> gcr.io/openssf/scorecard:stable --repo=https://github.com/TuralHasanov11/Dotnet-Ai
```

### Add Project to Solution
```sh
dotnet sln <solution-file>.sln add <project-file>.csproj
```

### Ollama
```sh
ollama pull qwen3.5:0.8b
ollama run qwen3.5:0.8b
```