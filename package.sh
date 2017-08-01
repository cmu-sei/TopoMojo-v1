#!/bin/bash

if [ ! -e dist ]; then
    mkdir dist
fi

ver=1.0.0-`date +%Y%m%d`$1

for i in TopoMojo.Web; do
    dotnet publish -c Release -o ../../$i src/$i
    tar czf dist/$i-$ver.tgz $i
    rm -rf $i
done