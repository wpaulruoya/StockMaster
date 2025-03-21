# Use official .NET SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and restore as distinct layers
COPY ["StockMaster.csproj", "./"]
RUN dotnet restore "StockMaster.csproj"

# Copy the rest of the application source code
COPY . . 
WORKDIR "/src"

# Build and publish the application
RUN dotnet publish "StockMaster.csproj" -c Release -o /app/publish

# Use a runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "StockMaster.dll", "--urls", "http://+:5120"]
