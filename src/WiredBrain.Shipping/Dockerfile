FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["WiredBrain.Shipping/WiredBrain.Shipping.csproj", "WiredBrain.Shipping/"]
COPY ["WiredBrain.Messages/WiredBrain.Messages.csproj", "WiredBrain.Messages/"]
RUN dotnet restore "WiredBrain.Shipping/WiredBrain.Shipping.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "WiredBrain.Shipping/WiredBrain.Shipping.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WiredBrain.Shipping/WiredBrain.Shipping.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY WiredBrain.Shipping/start.sh .
RUN chmod +x /app/start.sh
EXPOSE 80
ENTRYPOINT ["dotnet", "WiredBrain.Shipping.dll"]
