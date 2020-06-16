#!/bin/bash
## ensure certificates generates before running containers

cfssl=/home/admin/appliance/cfssl

if [ ! -e "$cfssl/output/appliance.pem" ]; then
    /bin/bash $cfssl/setup.sh
fi

/usr/local/bin/docker-compose up -d
