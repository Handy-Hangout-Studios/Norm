#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /Norm

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore
COPY . .
WORKDIR "/src/Norm"
RUN dotnet build "Norm.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Norm.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /Norm
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Norm.dll"]