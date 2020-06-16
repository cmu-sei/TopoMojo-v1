# Appliance Certificate Management

This is a convenience tool for generating realistic certificates in offline environments.
It constructs a root and intermediate CA, and provides an *appliance* wildcard certificate
for your host/web-server.

## Setup

The `setup.sh` will create the cert chain if it doesn't exist.
By default, it will create a certificate for `foundry.local`.
You can customize it by passing in a different domain, e.g. `./setup.sh example.com`

To modify the Root and Intermediate certificate names, use the `scripts/ca.json` and `scripts/int.json` files.
Init won't overwrite existing int/ca certs, so delete the `scripts/*.pem` files if you want to start over.
Init will overwrite the appliance cert, and distribute it throughout the app.

```bash
> setup.sh [domain]
```
