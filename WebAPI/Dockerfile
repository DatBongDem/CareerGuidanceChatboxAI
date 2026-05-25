#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all .csproj files and restore dependencies
# This layer is cached until a .csproj file changes
COPY ["BusinessLogic/BusinessLogic.csproj", "BusinessLogic/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["WebAPI/WebAPI.csproj", "WebAPI/"]
RUN dotnet restore "WebAPI/WebAPI.csproj"

# Copy the rest of the source code
COPY . .

# Publish the application
WORKDIR "/src/WebAPI"
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render uses the PORT environment variable to determine which port to listen on.
# ASP.NET Core automatically listens on the port specified by the ASPNETCORE_URLS environment variable.
# We don't need to explicitly EXPOSE a port, as Render handles it.
# The default ASP.NET Core port is 8080.
ENTRYPOINT ["dotnet", "WebAPI.dll"]
