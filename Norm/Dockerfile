#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Norm/Norm.csproj", "Norm/"]
COPY ["Norm.Database/Norm.Database.csproj", "Norm.Database/"]
COPY ["Norm.Omdb/Norm.Omdb.csproj", "Norm.Omdb/"]
COPY ["nuget.config", "/"]
RUN dotnet restore "Norm/Norm.csproj"
COPY . .
WORKDIR "/src/Norm"
RUN dotnet build "Norm.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Norm.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Norm.dll"]