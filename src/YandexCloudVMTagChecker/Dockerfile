FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y cron tzdata && \
    mkdir -p /var/run/ /var/spool/cron/crontabs /var/log && \
    touch /var/log/cron.log 

ENV TZ=UTC

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/YandexCloudVMTagChecker/YandexCloudVMTagChecker.csproj", "src/YandexCloudVMTagChecker/"]
RUN dotnet restore "src/YandexCloudVMTagChecker/YandexCloudVMTagChecker.csproj"
COPY . .
WORKDIR "/src/src/YandexCloudVMTagChecker"
RUN dotnet build "YandexCloudVMTagChecker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "YandexCloudVMTagChecker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY crontab /etc/cron.d/yandex-cloud-vm-tag-checker-cron
COPY entrypoint.sh /entrypoint.sh

RUN chmod 0644 /etc/cron.d/yandex-cloud-vm-tag-checker-cron && \
    chmod +x /entrypoint.sh

RUN crontab /etc/cron.d/yandex-cloud-vm-tag-checker-cron

ENTRYPOINT ["/entrypoint.sh"]
