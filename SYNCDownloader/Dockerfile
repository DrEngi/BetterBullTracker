#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SYNCDownloader/SYNCDownloader.csproj", "SYNCDownloader/"]
COPY ["SyncromaticsAPI/SyncromaticsAPI.csproj", "SyncromaticsAPI/"]
RUN dotnet restore "SYNCDownloader/SYNCDownloader.csproj"
COPY . .
WORKDIR "/src/SYNCDownloader"
RUN dotnet build "SYNCDownloader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SYNCDownloader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /app/build/Config.json /app/config.json
ENTRYPOINT ["dotnet", "SYNCDownloader.dll"]