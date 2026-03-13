FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ProductManagement.API/ProductManagement.API.csproj", "src/ProductManagement.API/"]
COPY ["src/ProductManagement.Application/ProductManagement.Application.csproj", "src/ProductManagement.Application/"]
COPY ["src/ProductManagement.Infrastructure/ProductManagement.Infrastructure.csproj", "src/ProductManagement.Infrastructure/"]
COPY ["src/ProductManagement.Domain/ProductManagement.Domain.csproj", "src/ProductManagement.Domain/"]

RUN dotnet restore "src/ProductManagement.API/ProductManagement.API.csproj"

COPY . .
RUN dotnet build "src/ProductManagement.API/ProductManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/ProductManagement.API/ProductManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductManagement.API.dll"]
