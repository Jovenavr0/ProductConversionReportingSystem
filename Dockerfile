FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "Web/Web.csproj"

RUN dotnet publish "Web/Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
RUN apt-get update && apt-get install -y curl net-tools
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Web.dll"]