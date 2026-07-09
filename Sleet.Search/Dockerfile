FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 7450

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sleet.Search/Sleet.Search.csproj","Sleet.Search/"]
RUN dotnet restore "Sleet.Search/Sleet.Search.csproj"

COPY . .
WORKDIR "/src/Sleet.Search/"
RUN dotnet build "Sleet.Search.csproj" -c $BUILD_CONFIGURATION  -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sleet.Search.csproj" -p:PublishSingleFile=true -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=7450
ENTRYPOINT ["./Sleet.Search"]
