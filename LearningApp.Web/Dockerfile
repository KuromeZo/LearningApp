FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src

COPY LearningApp.Web.csproj .
RUN dotnet restore LearningApp.Web.csproj

COPY . .
RUN dotnet publish LearningApp.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LearningApp.Web.dll"]