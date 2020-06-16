#!/bin/bash
## clean up and shutdown for distribution

cd /home/admin/appliance

# shutdown and delete apps
docker-compose down
docker volume prune -f

# remove yum cache
yum clean all
rm -rf /var/cache/yum

# remove any proxy info
rm -f /etc/profile.d/http_proxy.sh
rm -f /etc/systemd/system/docker.service.d/http-proxy.conf
sed -i s,proxy=.*$,, /etc/yum.conf

#remove any certs
rm -rf /home/admin/appliance/cfssl/output
rm -f /home/admin/appliance/cfssl/scripts/*.pem
rm -f /home/admin/appliance/vols/nginx/conf.d/*.pem
rm -f /home/admin/appliance/vols/nginx/html/*.pem
rm -f /home/admin/appliance/vols/entry.d/*.crt

# remove history
rm -rf /home/admin/.ssh
rm -rf /home/admin/.vscode-server
rm -f /home/admin/.bash_history
rm -f /root/.bash_history

history -c && shutdown -h now
