FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ServiceB/ServiceB.csproj ServiceB/
RUN dotnet restore ServiceB/ServiceB.csproj

COPY . .
RUN dotnet publish ServiceB/ServiceB.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ServiceB.dll"]

