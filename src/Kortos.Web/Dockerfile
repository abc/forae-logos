FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/NethysEmbed.Web/NethysEmbed.Web.csproj", "src/NethysEmbed.Web/"]
RUN dotnet restore "src/NethysEmbed.Web/NethysEmbed.Web.csproj"
COPY . .
WORKDIR "/src/src/NethysEmbed.Web"
RUN dotnet build "NethysEmbed.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NethysEmbed.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NethysEmbed.Web.dll"]
