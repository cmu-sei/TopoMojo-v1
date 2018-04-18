#!/bin/bash
#
#.Synopsis Add a migration for multiple database providers
#.Notes Assumes project is freshly built
#

if [ "$#" -ne 2 ]; then
    echo "usage: $0 context migration-name"
    exit 1
fi

context=$1
name=$2
folder=`echo $context | sed s,Context,,`
declare -a providers=("Sqlite", "SqlServer" "PostgreSQL")

for provider in "${providers[@]}"; do
    export Database__Provider=$provider
    echo $provider $name $context $folder
    dotnet ef migrations add $name --context $context -o Migrations/$folder --project ../TopoMojo.Data.$provider --no-build
    wait
done
