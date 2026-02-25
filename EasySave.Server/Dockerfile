# Use the .NET runtime as a base image for the final application
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base

# Specify the user for the application to run as
USER $APP_UID

# Set the working directory for the application
WORKDIR /app

# Use the .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set the build configuration argument with a default value
ARG BUILD_CONFIGURATION=Release

# Set the working directory for the build context
WORKDIR /src

# Copy the project file into the container
COPY "Server.csproj" "."

# Restore the dependencies defined in the project file
RUN dotnet restore Server.csproj

# Copy the rest of the application code into the container
COPY . .

# Set the working directory to the source folder
WORKDIR "/src/"

# Build the application
RUN dotnet build "./Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application for deployment
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Publish the application to the output folder
RUN dotnet publish "./Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create the final image for running the application
FROM base AS final

# Set the working directory for the final application
WORKDIR /app

# Copy the published files from the publish stage
COPY --from=publish /app/publish .

# Create a logs directory for the application
RUN mkdir logs

# Expose port 5000 for the application to listen on
EXPOSE 5000

# Set /app/logs as a volume so that logs can be stored externally
VOLUME /app/logs

# Define the entry point for the application when running the container
ENTRYPOINT ["dotnet", "Server.dll"]