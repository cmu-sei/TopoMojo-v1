# TopoMojo Appliance

A virtual machine hosting TopoMojo applications.

## Overview

This application stack consists of an nginx webserver, the foundry identity server, the topomojo server, and postgresql containers.

They are assembled using docker-compose on a CentOs 7 operating system.

Everything starts up with some defaults applied.  You will want to walk through the configuration to modify settings to your needs.

If viewing this doc *on the appliance*, please review the latest version at [github.com/cmu-sei/TopoMojo](https://github.com/cmu-sei/TopoMojo/docs/appliance).

## Quick Start
1. start the appliance vm
    - find the ip address and/or set up DNS
2. browse to https://foundry.local (or ip address if DNS or hosts file not set)
3. login with `admin@foundry.local` : `321ChangeMe!`

## Access

> vm login credentials: `admin` : `321ChangeMe!`

By default, the vm is set to get an ip address from DHCP. To set a static ip, update the network configuration:
```bash
cd /etc/sysconfig/network-scripts
cp ifcfg-ens32-static-example ifcfg-ens32
vi ifcfg-ens32
```

It is important to have DNS resolving for your domain names.  If using hosts files, be sure to add an entry for <id, topomojo, appliance>.foundry.local on both your workstation (resolving to your appliance ip) and on the appliance itself (resolving to 127.0.0.1).

Better would be to create a DNS record for foundry.local and *.foundry.local on an external DNS server that points to your appliance's ip address.

The default domain is `foundry.local`. You can change this globally using something like:

```
cd /home/admin/appliance
find . -type f -exec sed -i s/foundry\.local/whatever\.domain/g {} \;
```

## Certificate

By default the appliance generates a local certificate authority and issues a certificate for all the applications.

After changing the domain from `foundry.local` to your custom domain, use `cfssl/setup.sh` script to generate a new wildcard certificate for your domain.  That script will distribute the new certificate appropriately.  See the `cfssl/Readme.md` if other configuration is needed.

```bash
> cd ~/appliance
> bash cfssl/setup.sh your.domain

# if stack is running, restart nginx
> docker-compose restart nginx
```

From your workstation, it may be convenient to install your Root CA cert from `https://foundry.local`.

If providing a certificate by some other means, be sure to coordinate it's location with nginx. (See below.)

For other certificates the applications need to trust, drop them in `vols/entry.d/*.crt`.  Generally this is only needed if your vCenter CA certificate (or hypervisor certificate CA) is not trusted by default.

## Nginx Configuration

An Nginx reverse proxy is deployed to provide a single entry point to the apps and to terminate TLS.
If you change the name or location of the site's certificate files, also update `vols/nginx/nginx.conf`.  You may also need to mount an additional volume to the container in the docker-compose.yaml.

## Identity Configuration

The identity server is configured with default settings.  Look through the `vols/identity-conf/identity.conf` file to edit them.

A default admin account is configured there: `admin@foundry.local` and `321ChangeMe!`.  Use that username and password to login and set up other accounts, or see below to reset the database.

User's need a confirmation code for 2FA, registration, and password resets.  To set up email, edit the `appmail` environment variables in `docker-compose.yaml`.  To run without email, you can set up an initial `Account__OverrideCode` in identity.conf, or use the ui at https://id.foundry.local/accounts to add one.

More information for Identity server might exist at https://github.com/cmu-edu/Identity.

## TopoMojo Configuration

The TopoMojo server is configured with default settings.  Look through the `vols/topomojo-conf/topomojo.conf` file to edit them.

Without specifying a `Pod__Url` value, TopoMojo operates with a fake hypervisor, so you can test without actually having a VMware backend in place.  When you do add a url to vcenter (or vsphere hosts), add the vCenter root ca certificate to `vols/entry.d/vcenter-root.crt`. (Any file name is fine, but should be in PEM format with `.crt` extension.)

<!-- Also, ensure the appliance trusts it.
```bash
cp vcenter-ca.crt /etc/pki/ca-trust/source/anchors
sudo update-ca-trust
``` -->

The topomojo service user needs a lot of permission.
// TODO: list required vSphere permissions

Adding stock templates is a manual process.  On the TopoMojo Admin, Templates page, create a new template and initialize the disk and install an operating system.  To make it available for others to use, be sure to check the *Published* box.

If you have existing vm's, create a Template and edit the disk path to the location of the existing vmdk.  You may need to adjust other settings as well to match your vm's vmx file.  By convention, stock vmdk's are located in the `[topomojo] 00000000-0000-0000-0000-000000000000` folder.

TopoMojo supports uploading files that can be attached to vm's as ISO files.  For this to work, the application must save the iso file to a datastore to which the hypervisors have access.  This is generally done by mounting an nfs share to the topomojo container as well as exposing the same nfs share to the hypervisors as a datastore.

If using an NFS datastore, you only need one, i.e. [topomojo]. You can mount it to the topomojo container to support uploaded files, and reference it in the `Pod__` settings.

If your hypervisors use block storage, you'll need two datastores, i.e [topomojo] and [topomojo-nfs].  In this case, you'd mount the nfs share to the topomojo container and set `Pod_Isostore = [topomojo-nfs]`.  The other two stores would be set to the block datastore...`Pod__Vmstore = [topomojo] _run`, `Pod__Diskstore` = [topomojo]`.  The datastore names/paths can be whatever you want them to be; this discussion is mainly to highlight that the ISOSTORE should be an nfs datastore into which TopoMojo can write files.

## Docker Compose

This appliance uses docker-compose to manage the application stack.  If comfortable editing the docker-compose.yaml to adjust the stack configuration, feel free to do so.

You can control the stack with `docker-compose help`.

If your appliance is behind a web proxy, the `scripts/set-proxy.sh` might help you configure it.

## Reset Database
To delete existing data and start fresh use:
```
docker-compose down
docker volumes prune -f
```

## Recommendations
- Change the appliance admin password with `passwd`.
- Find and replace `321ChangeMe!` in ./vols/*
- Refine the database users (different user/password for each app).

## Build your own appliance

The scripts to build the appliance are included in `scripts`.
They target CentOS-7, but could be modified fairly easily for other variants.

From a vanilla centos-7-minimal installation, the following sequence should yield
a functioning appliance.

```bash

## set up proxy if necessary
# export http_proxy=http://proxy.my.domain:8080
# export https_proxy=http://proxy.my.domain:8080
# export no_proxy=localhost,.local
# sudo echo proxy=http://proxy.my.domain:8080 >> /etc/yum.conf
# sudo echo "export http_proxy=http://proxy.my.domain:8080" > /etc/profile.d/http_proxy.sh
# sudo echo "export https_proxy=http://proxy.my.domain:8080" >> /etc/profile.d/http_proxy.sh
# sudo echo "export no_proxy=localhost,.local" >> /etc/profile.d/http_proxy.sh


## get appliance bundle
cd ~
sudo yum -y update
sudo yum -y install git
git clone https://github.com/cmu-sei/topomojo.git
cp -r topomojo/docs/appliance .
rm -rf topomojo
cd appliance

## install docker; if behind web proxy, include it as an argument
# sudo bash scripts/install-docker.sh http://proxy.my.domain:8080
sudo bash scripts/install-docker.sh

## systemctl if desired
sudo bash scripts/enable-apphost.sh

## start up the stack
sudo systemctl start appliance
```

It is important to start the stack the first time using
`systemctl start appliance` or `bash scripts/start.sh`
so that the certificates get generated.

Alternatively, use `bash cfssl/setup.sh` before running `docker-compose up -d`
