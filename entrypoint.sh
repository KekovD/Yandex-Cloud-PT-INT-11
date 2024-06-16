#!/bin/sh

printenv | grep -v "no_proxy" >> /etc/environment

/usr/bin/dotnet /app/YandexCloudVMTagChecker.dll

cron

tail -f /var/log/cron.log
