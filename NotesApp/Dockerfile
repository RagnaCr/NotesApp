# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj files and restore the dependencies
COPY ./NotesApp/*.csproj ./
RUN dotnet restore

# Copy all source files and publish the project
COPY ./NotesApp ./
RUN dotnet publish -c Release -o /app -v diag

# Use a lighter .NET image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /app .

# Open port 80 to run the application in the container
EXPOSE 80
 
# Run the application
ENTRYPOINT ["dotnet", "NotesApp.dll"]