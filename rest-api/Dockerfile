FROM mcr.microsoft.com/dotnet/sdk:9.0 as build

WORKDIR /app

COPY . .

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as deploy

WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "rest-api.dll"]