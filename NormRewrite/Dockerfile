FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NormRewrite/NormRewrite.csproj", "NormRewrite/"]
RUN dotnet restore "NormRewrite/NormRewrite.csproj" --source=https://api.nuget.org/v3/index.json --source=https://nuget.emzi0767.com/api/v3/index.json
COPY . .
WORKDIR "/src/NormRewrite"
RUN dotnet build "NormRewrite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NormRewrite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ARG DOTNET_ENV=Development
ENV DOTNET_ENVIRONMENT=${DOTNET_ENV}
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NormRewrite.dll"]
