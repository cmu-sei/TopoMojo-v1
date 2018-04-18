#!/bin/bash
#
#.Synopsis Add a migration for multiple database providers
#.Notes Assumes project is freshly built
#

if [ "$#" -ne 1 ]; then
    echo "usage: $0 context"
    exit 1
fi

context=$1
declare -a providers=("Sqlite" "SqlServer" "PostgreSQL")

for provider in "${providers[@]}"; do
    export Database__Provider=$provider
    dotnet ef migrations remove --context $context --project ../TopoMojo.Data.$provider --no-build -f
done
