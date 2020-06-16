#!/bin/bash
## helper script for toggling web proxy config

if [ -z "$1" ]; then
 echo usage: $0 http://proxy.my.domain:8080 [no-proxy-string]
 exit
fi

proxy=$1
noproxy=${2:-localhost,.local}

# set for shells
echo "export http_proxy=$proxy" > /etc/profile.d/http_proxy.sh
echo "export https_proxy=$proxy" >> /etc/profile.d/http_proxy.sh
echo "export no_proxy=$noproxy" >> /etc/profile.d/http_proxy.sh

# set for yum
echo proxy=$proxy >> /etc/yum.conf

# set for docker
target=/etc/systemd/system/docker.service.d
mkdir -p $target
cat <<EOF > $target/http-proxy.conf
[Service]
Environment="HTTP_PROXY=$proxy"
Environment="HTTPS_PROXY=$proxy"
Environment="NO_PROXY=$noproxy"
EOF

systemctl daemon-reload
systemctl restart docker

echo Proxy set.  Please open a new shell.
