#!/bin/bash
path=assets/fonts
dst=src/$path
mkdir -p $dst

fonts=(
    'https://fonts.googleapis.com/icon?family=Material+Icons#material-icons.css'
    'https://fonts.googleapis.com/css?family=Open+Sans:300,400,600#open-sans.css'
    )

for u in ${fonts[@]}; do
    n=`echo $u | awk -F# '{print $2}'`
    curl $u -o $dst/$n -H 'Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8' -H 'User-Agent: Mozilla/5.0 (Windows NT 6.3; rv:39.0) Gecko/20100101 Firefox/39.0'
    for w in $(grep url $dst/$n | sed -n 's/.*url(\([^)]*\)).*/\1/p' | sort -u); do
        fn=`echo $w | awk -F"/" '{print $NF}'`
        sed -i.bak s,$w,/$path/$fn, $dst/$n
        curl $w -o $dst/$fn
    done
done

rm -f $dst/*.bak
