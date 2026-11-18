FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore ./src/ChangelogPublisher/ChangelogPublisher.csproj

RUN dotnet publish \
    ./src/ChangelogPublisher/ChangelogPublisher.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "/app/ChangelogPublisher.dll"]
