# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

# Copy only .csproj first to optimize caching
COPY ["ToDo.csproj", "./"]

# Restore dependencies
RUN dotnet restore "ToDo.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "ToDo.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
RUN dotnet publish "ToDo.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

# Final stage: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5555

# Copy published application from build stage
COPY --from=build /app/publish .

COPY ./MariaDB.sql .

# Run the application
ENTRYPOINT ["dotnet", "ToDo.dll"]