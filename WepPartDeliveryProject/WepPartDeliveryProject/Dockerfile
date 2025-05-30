#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["WepPartDeliveryProject/WepPartDeliveryProject.csproj", "WepPartDeliveryProject/"]
COPY ["DbManager/DbManager.csproj", "DbManager/"]
RUN dotnet restore "./WepPartDeliveryProject/WepPartDeliveryProject.csproj"
COPY . .
WORKDIR "/src/WepPartDeliveryProject"
RUN dotnet build "./WepPartDeliveryProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "./WepPartDeliveryProject.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WepPartDeliveryProject.dll"]