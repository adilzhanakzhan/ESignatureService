# Use the ASP.NET runtime image as the base
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install libgdiplus and its dependencies
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libgdiplus libc6-dev && \
    ln -s /usr/lib/libgdiplus.so /usr/lib/libgdiplus.so.0 || cat /var/log/apt/* && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Use the .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ESignatureService.csproj", "."]
RUN dotnet restore "./ESignatureService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ESignatureService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ESignatureService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage: runtime image with the published application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ESignatureService.dll"]
