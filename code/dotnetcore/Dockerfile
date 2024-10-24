FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

## Copy csproj and restore as distinct layers
COPY . .
RUN dotnet publish -c Release -o output


FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

WORKDIR /app
COPY --from=build /app/output ./

ENTRYPOINT ["dotnet", "dotnetcore.dll"]
