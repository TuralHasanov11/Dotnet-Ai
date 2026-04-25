# Dotnet-Ai — Copilot Instructions

These instructions make AI agents productive quickly in this repo by documenting architecture, workflows, and project-specific conventions.

## Big Picture
- Minimal ASP.NET Core app (net10.0) exposing a root endpoint and health checks: see ../src/AgentFrameworkQuickStart/Program.cs.
- Containerized via multi-stage Dockerfile and orchestrated locally with Docker Compose: see ../src/AgentFrameworkQuickStart/Dockerfile and ../docker-compose.yaml.
- Kubernetes manifests provided under ../k8s for a simple deployment/service in namespace "dotnet-ai" with probes and a LoadBalancer service.
- Central Package Management is enabled: see ../Directory.Packages.props.

## Code & Endpoints
- Root: GET / returns the assembly name.
- Health: GET /health via AddHealthChecks().
- OpenAPI: enabled only in Development; AddOpenApi and MapOpenApi expose docs in dev.

## Build & Run (local)
- Build: dotnet build src/AgentFrameworkQuickStart/AgentFrameworkQuickStart.csproj
- Run: dotnet run --project src/AgentFrameworkQuickStart/AgentFrameworkQuickStart.csproj
- Config: default logging and hosts in ../src/AgentFrameworkQuickStart/appsettings.json. Dev overrides in ../src/AgentFrameworkQuickStart/appsettings.Development.json.

## Docker & Compose
- Image build context is repo root; Dockerfile lives in project folder.
- Compose service: agent-framework-quick-start builds dotnet-ai/agent-framework-quick-start and exposes ports 5002 (HTTP) and 5003 (HTTPS): see ../docker-compose.yaml.
- Note: the base Dockerfile uses USER $APP_UID (Visual Studio debug pattern). If building outside VS, ensure APP_UID is set or remove that line.

## Kubernetes (kind or any K8s)
- Namespace: apply ../k8s/dotnet-ai-namespace.yaml.
- ConfigMap sets ASP.NET ports/URLs: see ../k8s/agent-framework-quick-start-config.yaml.
- Deployment/Service with probes and LoadBalancer: see ../k8s/agent-framework-quick-start-service.yaml.
- ServiceAccount used by the Deployment: see ../k8s/agent-framework-quick-start-service-account.yaml.
- Optional local cluster: kind config in ../k8s/cluster-config.yaml.

## Project Conventions
- Target framework: net10.0 (see ../src/AgentFrameworkQuickStart/AgentFrameworkQuickStart.csproj).
- Central Package Versions: managed in ../Directory.Packages.props (e.g., Microsoft.AspNetCore.OpenApi). Add new packages via this file.
- Health/readiness/liveness probes use /health on port 5002. Keep probe paths aligned with MapHealthChecks.
- Environment defaults (ports/urls) are provided via ConfigMap in k8s to match container ports.

## Common Tasks (examples)
- Add an endpoint: extend Program.cs (Minimal API style) and ensure it’s covered by OpenAPI in Development.
- Update image/tag: change image: dotnet-ai/agent-framework-quick-start:latest in K8s and/or compose.
- Tune probes/resources: edit the Deployment block in ../k8s/agent-framework-quick-start-service.yaml.
- Add a package: declare version in ../Directory.Packages.props and reference it in the project.

## Notes
- Redis/Qdrant manifests exist under ../k8s but the app doesn’t currently integrate with them; wire up services/env as needed.
- HTTPS port 5003 is exposed for future use; current app uses HTTP 5002 with probes.
