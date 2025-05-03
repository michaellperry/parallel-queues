FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["WiredBrain.Ordering/WiredBrain.Ordering.csproj", "WiredBrain.Ordering/"]
COPY ["WiredBrain.Messages/WiredBrain.Messages.csproj", "WiredBrain.Messages/"]
RUN dotnet restore "WiredBrain.Ordering/WiredBrain.Ordering.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "WiredBrain.Ordering/WiredBrain.Ordering.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WiredBrain.Ordering/WiredBrain.Ordering.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port for metrics
EXPOSE 80
EXPOSE 9090

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ORDER_ARRIVAL_RATE_MS=1000

ENTRYPOINT ["dotnet", "WiredBrain.Ordering.dll"]
