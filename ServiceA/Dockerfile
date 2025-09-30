FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ServiceA/ServiceA.csproj ServiceA/
RUN dotnet restore ServiceA/ServiceA.csproj

COPY . .
RUN dotnet publish ServiceA/ServiceA.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ServiceA.dll"]

