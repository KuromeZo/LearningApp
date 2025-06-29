FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src

COPY LearningApp.csproj .
RUN dotnet restore LearningApp.csproj

COPY . .
RUN dotnet publish LearningApp.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LearningApp.dll"]