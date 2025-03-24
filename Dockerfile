# Use official .NET 8.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY StockMaster.csproj ./
RUN dotnet restore StockMaster.csproj

# Copy the entire project and build the release version
COPY . .
RUN dotnet publish StockMaster.csproj -c Release -o /app/publish

# Use .NET 8.0 runtime for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose the ports
EXPOSE 5120
EXPOSE 7085

# Start the application
CMD ["dotnet", "StockMaster.dll"]
