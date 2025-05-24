FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["WiredBrain.Payment/WiredBrain.Payment.csproj", "WiredBrain.Payment/"]
RUN dotnet restore "WiredBrain.Payment/WiredBrain.Payment.csproj"
COPY . .
WORKDIR "/src/WiredBrain.Payment"
RUN dotnet build "WiredBrain.Payment.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WiredBrain.Payment.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WiredBrain.Payment.dll"]