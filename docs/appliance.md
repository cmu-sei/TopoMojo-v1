# TopoMojo Appliance

Available via the [SEI Tools page](https://www.sei.cmu.edu/research-capabilities/all-work/display.cfm?customel_datapageid_4050=175686).

## Quick Start
1. start the vm
2. set DNS for the host
    * for DNS, add records for foundry.local and *.foundry.local
    * if hosts file, add records for foundry.local and id.foundry.local, topomojo.foundry.local
3. browse to `https://foundry.local`
4. [optional] install the CA certificate
5. Instead of logging in, register a new account using `Code: override` (First registered account is administrator)

## Configuration

Host Credentials: `admin`, `321ChangeMe!`

This application stack consists of an nginx webserver, the foundry identity server, the topomojo server, and postgresql database container.

The stack is managed with docker-compose in `/home/admin/apps`, and is set to start on boot.

If you are behind a proxy, at least uncomment the proxy line in both `/etc/yum.conf` and `/etc/systemd/system/docker.service.d/http-proxy.conf`. Then restart docker (`sudo systemctl restart docker`) or just restart the host.

### Domain Name

The default domain is `foundry.local`. You can change this globally using something like:

```
cd /home/admin/apps
find . -type f -exec sed -i s/foundry\.local/whatever\.domain/g {} \;
```

It is important to have DNS resolving for your domain names.  If using hosts files, be sure to add an entry for <id, topomojo, apphost>.foundry.local on both your workstation (resolving to your apphost vm) and the apphost (resolving to localhost).

### Certificate

After changing the domain from `foundry.local` to your custom domain, use the `cfssl/init.sh new.domain` script to generate a new wildcard certificate for your domain.  That script will distribute the new certificate appropriately.  See the `cfssl/Readme.md` if other configuration is needed.

If adding your own certificate, note where the init.sh script distributes the files.

From your workstation, it may be convenient to grab your Root CA cert from `http://foundry.local/ca.html`

### Settings

Add Virtual Center root ca certificate to `vols/entry.d/vcenter-root.crt` so that consoles will connect.

The `vols/<identity | topomojo>/init/seed.data.json` files create an admin user.

See GitHub for TopoMojo settings reference: https://github.com/cmu-sei/TopoMojo in `/src/TopoMojo.Web/appsettings.json`

Currently, adding stock disks is a manual process.  You'll need to clone or create some base disks into your stock folder, then in the TopoMojo Admin-Templates ui, create a new template and point the disk at the stock disk path.  Be sure to check the *Published* box.

Without specifying a Pod:Url value, TopoMojo operates in Mock mode, so you can play around without actually having a VMware backend in place.  When ready, see the reference above to configure your Pod settings.

TODO: ISO upload comments

TODO: datastore comments

### Testing
To delete existing data and start fresh use:
```
docker-compose down
docker volumes prune
```


