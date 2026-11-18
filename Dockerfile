FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src/ChangelogPublisher/*.csproj ./ChangelogPublisher/
RUN dotnet restore ./ChangelogPublisher/ChangelogPublisher.csproj

COPY src/ChangelogPublisher/ ./ChangelogPublisher/

RUN dotnet publish \
    ./ChangelogPublisher/ChangelogPublisher.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ChangelogPublisher.dll"]
