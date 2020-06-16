#!/bin/bash
echo running $0
set -e
if [ -z "$ENTRY_DIR" ]; then ENTRY_DIR=/entry.d; fi

dst=/etc/pki/ca-trust/source/anchors
if [ -d "$dst" ]; then
    echo "updating trusted certificates (rhel)"
    cp -f $ENTRY_DIR/*.crt $dst | true
    update-ca-trust
fi

dst=/usr/local/share/ca-certificates
if [ -d "$dst" ]; then
    echo "updating trusted certificates (debian)"
    cp -f $ENTRY_DIR/*.crt $dst | true
    update-ca-certificates
fi
