#!/bin/bash
## auto start / stop with systemd

cat <<EOF > /etc/systemd/system/appliance.service
[Unit]
Description=Control appliance stack
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/home/admin/appliance
ExecStart=/bin/bash scripts/start.sh
ExecStop=/usr/local/bin/docker-compose down

[Install]
WantedBy=multi-user.target

EOF

systemctl daemon-reload
systemctl enable appliance
