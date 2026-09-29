FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Dungeon.Presentation.slnx ./
COPY Dungeon.Contracts/Dungeon.Contracts.csproj Dungeon.Contracts/
COPY Dungeon.Domain/Dungeon.Domain.csproj Dungeon.Domain/
COPY Dungeon.Application/Dungeon.Application.csproj Dungeon.Application/
COPY Dungeon.Infrastructure/Dungeon.Infrastructure.csproj Dungeon.Infrastructure/
COPY Dungeon.Presentation/Dungeon.Presentation.csproj Dungeon.Presentation/
COPY Dungeon.Test/Dungeon.Test.csproj Dungeon.Test/

RUN dotnet restore Dungeon.Presentation.slnx

COPY . .
RUN dotnet publish Dungeon.Presentation/Dungeon.Presentation.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080 8081

COPY --from=build /app/publish .

# Unprivileged "app" user shipped by the aspnet image.
USER $APP_UID

ENTRYPOINT ["dotnet", "Dungeon.Presentation.dll"]
