[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/TuralHasanov11/Dotnet-Ai/badge)](https://scorecard.dev/viewer/?uri=github.com/TuralHasanov11/Dotnet-Ai)

## Project purpose

Dotnet-Ai is a .NET 10 sample workspace for building and testing AI-oriented application building blocks in ASP.NET Core.  
It provides quick-start services (including API/OpenAPI setup and identity integration), shared libraries, automated tests, and container/Kubernetes deployment assets so you can prototype and evolve AI-ready backend services with production-style structure.

## Local Setup

For Windows, the recommended way to manage the .NET toolchain is `dotnetup` in Terminal Mode.

1. Install `dotnetup` from PowerShell:

	```powershell
	iwr https://aka.ms/dotnetup/get-dotnetup.ps1 | iex
	```

2. Choose `latest` as the initial channel unless you need a different servicing line.
3. Choose Terminal Mode so the current shell profile uses the `dotnetup`-managed SDKs by default.
4. Restart the terminal, then verify the setup:

	```powershell
	dotnet --version
	dotnetup list
	```

If you prefer to keep your existing .NET installation untouched, use Isolation Mode and run commands through `dotnetup dotnet` instead.