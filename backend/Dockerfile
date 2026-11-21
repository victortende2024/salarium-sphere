# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the .csproj files first to leverage Docker layer caching for restore
COPY ["SalariumSphere.API/SalariumSphere.API.csproj", "SalariumSphere.API/"]
COPY ["SalariumSphere.Application/SalariumSphere.Application.csproj", "SalariumSphere.Application/"]
COPY ["SalariumSphere.Domain/SalariumSphere.Domain.csproj", "SalariumSphere.Domain/"]
COPY ["SalariumSphere/SalariumSphere.Infrastructure/SalariumSphere.Infrastructure.csproj", "SalariumSphere/SalariumSphere.Infrastructure/"]
RUN dotnet restore "SalariumSphere.API/SalariumSphere.API.csproj"

# Copy the rest of the source code and publish
COPY . .
WORKDIR "/src/SalariumSphere.API"
RUN dotnet publish "SalariumSphere.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SalariumSphere.API.dll"]