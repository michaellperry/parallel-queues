FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["WiredBrain.Billing/WiredBrain.Billing.csproj", "WiredBrain.Billing/"]
COPY ["WiredBrain.Messages/WiredBrain.Messages.csproj", "WiredBrain.Messages/"]
RUN dotnet restore "WiredBrain.Billing/WiredBrain.Billing.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "WiredBrain.Billing/WiredBrain.Billing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WiredBrain.Billing/WiredBrain.Billing.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WiredBrain.Billing.dll"]
