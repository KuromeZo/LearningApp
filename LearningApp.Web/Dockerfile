FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src

COPY LearningApp/LearningApp.csproj LearningApp/
RUN dotnet restore LearningApp/LearningApp.csproj

COPY LearningApp/ LearningApp/
RUN dotnet publish LearningApp/LearningApp.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LearningApp.dll"]