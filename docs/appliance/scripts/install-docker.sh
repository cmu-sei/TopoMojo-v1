#!/bin/bash
#
# CENTOS docker install
#

# if behind a proxy, set these values first!
proxy=$1
noproxy=${2:-localhost,.local}

export http_proxy=$proxy
export https_proxy=$proxy
export noproxy=$noproxy

# if [ -n "$proxy" ]; then echo proxy=$proxy >> /etc/yum.conf; fi

yum install -y yum-utils

yum-config-manager \
    --add-repo \
    https://download.docker.com/linux/centos/docker-ce.repo

yum -y install docker-ce docker-ce-cli containerd.io

if [ -n "$proxy" ]; then

  mkdir -p /etc/systemd/system/docker.service.d
  cat <<EOF >> /etc/systemd/system/docker.service.d/http-proxy.conf
[Service]
Environment="HTTP_PROXY=$proxy"
Environment="HTTPS_PROXY=$proxy"
Environment="NO_PROXY=$noproxy"
EOF

fi

mkdir -p /etc/docker
cat <<EOF >> /etc/docker/daemon.json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m"
  }
}
EOF

groupadd docker
usermod -aG docker admin

systemctl enable docker
systemctl start docker

# install compose
tag=`curl -s https://github.com/docker/compose/releases/latest | awk -F'"' '{print $2}' | awk -F/ '{print $NF}'`
curl -L https://github.com/docker/compose/releases/download/$tag/docker-compose-`uname -s`-`uname -m` -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

echo Done installing docker.
