FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Simulated.Payments/Simulated.Payments.csproj", "Simulated.Payments/"]
RUN dotnet restore "Simulated.Payments/Simulated.Payments.csproj"
COPY . .
WORKDIR "/src/Simulated.Payments"
RUN dotnet build "Simulated.Payments.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Simulated.Payments.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Simulated.Payments.dll"]
