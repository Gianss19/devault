FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY devault.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup appuser

COPY --from=build /app/publish .

USER appuser

EXPOSE 5164

ENTRYPOINT ["dotnet", "devault.dll"]
