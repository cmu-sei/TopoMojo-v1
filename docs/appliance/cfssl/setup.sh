#!/bin/sh

path=/home/admin/appliance
certs=$path/cfssl/output

docker run --rm -v $path/cfssl:/opt/cfssl cfssl -c "sh scripts/generate-certificate.sh $@"

cat $certs/appliance.pem $certs/appliance-int.pem > $path/vols/nginx/conf.d/appliance.pem
cp $certs/appliance-key.pem $path/vols/nginx/conf.d/appliance-key.pem
cp $certs/appliance-ca.pem $path/vols/nginx/html/ca.pem
cp $certs/appliance-ca.pem $path/vols/entry.d/appliance-ca.crt
cp $certs/appliance-int.pem $path/vols/entry.d/appliance-int.crt

openssl pkcs12 -export \
    -in $certs/appliance.pem \
    -inkey $certs/appliance-key.pem \
    -out $path/vols/identity-conf/signer.pfx \
    -nodes \
    -passout pass:
