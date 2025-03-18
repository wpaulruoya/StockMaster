# Use official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything and build the app
COPY . ./
RUN dotnet publish -c Release -o out

# Use a lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the port (ensure it matches the API/MVC settings)
EXPOSE 5000
EXPOSE 5001

# Start the application
ENTRYPOINT ["dotnet", "YourProjectName.dll"]
