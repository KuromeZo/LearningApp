
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LearningApp/LearningApp.csproj", "LearningApp/"]
RUN dotnet restore "LearningApp/LearningApp.csproj"

COPY LearningApp/ LearningApp/

WORKDIR "/src/LearningApp"
RUN dotnet build "LearningApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LearningApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LearningApp.dll"]