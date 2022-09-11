FROM mcr.microsoft.com/dotnet/sdk:7.0.100-preview.7-bullseye-slim-amd64 AS build-env
WORKDIR /DataParser
    

# Build the sqlite database
COPY DataParser/* ./
COPY data_source/csse_covid_19_data/csse_covid_19_daily_reports/*.csv ./data_source/
# RUN dotnet run "./data_source" "./data.sqlite"

# # Build the API Server
# WORKDIR /APIServer
# COPY APIServer/*.csproj ./
# RUN dotnet restore
# COPY APIServer/* ./
# RUN dotnet publish -c Release -o out
    
# # # Build runtime image
# FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-preview.7-bullseye-slim-amd64
# WORKDIR /app
# COPY --from=build-env /DataParser/data.sqlite .
# COPY --from=build-env /APIServer/out .
# ENTRYPOINT ["APIServer", "data.sqlite"]