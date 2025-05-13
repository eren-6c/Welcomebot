# Use .NET SDK to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out --no-self-contained

# Use minimal runtime image with environment optimizations
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 💡 Low-memory optimizations
ENV DOTNET_GCHeapHardLimit=512000000 \
    DOTNET_GCHeapHardLimitPercent=50 \
    DOTNET_GCServer=0 \
    DOTNET_GCConcurrent=0

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "Ticket BOT.dll"]

