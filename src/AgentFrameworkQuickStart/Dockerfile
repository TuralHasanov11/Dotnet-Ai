FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5002

ENV ASPNETCORE_URLS=http://+:5002

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/AgentFrameworkQuickStart/AgentFrameworkQuickStart.csproj", "src/AgentFrameworkQuickStart/"]
RUN dotnet restore "src/AgentFrameworkQuickStart/AgentFrameworkQuickStart.csproj"
COPY . .
WORKDIR "/src/src/AgentFrameworkQuickStart"
RUN dotnet build "AgentFrameworkQuickStart.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "AgentFrameworkQuickStart.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgentFrameworkQuickStart.dll"]
