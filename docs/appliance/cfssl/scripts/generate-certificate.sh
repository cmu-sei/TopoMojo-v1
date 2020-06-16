#!/bin/sh

cd /opt/cfssl/scripts

if [ ! -f appliance-ca.pem ]; then
    cfssl gencert -initca meta-ca.json | cfssljson -bare appliance-ca
fi

if [ ! -f appliance-int.pem ]; then
    cfssl gencert -ca appliance-ca.pem -ca-key appliance-ca-key.pem -config config.json -profile intca meta-int.json | cfssljson -bare appliance-int
fi

echo [$1]
cp meta-host.json customhost.json
if [ -n "$1" ];then sed -i s,foundry.local,$1, customhost.json; fi
cfssl gencert -ca appliance-int.pem -ca-key appliance-int-key.pem -config config.json -profile server customhost.json | cfssljson -bare appliance

mkdir -p ../output
cp *.pem ../output
chown 1000:1000 ../output/*
