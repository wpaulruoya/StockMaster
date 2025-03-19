# Use official .NET SDK image for building the MVC app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Use SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StockMaster/StockMaster.csproj", "StockMaster/"]
RUN dotnet restore "StockMaster/StockMaster.csproj"

COPY . .
WORKDIR "/src/StockMaster"
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockMaster.dll"]
