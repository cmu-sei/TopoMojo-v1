#!/bin/node
/*
 * This script nudges the ci wrangler to swing into action.
 * It sends the svc and a url to the publish package.
 * I.e. node ci-nudge.js step-browser http://cloud.net/updates/Step.Browser-1.0.0.zip
*/
const http = require('http');
const crypto = require('crypto');
const alg = "aes128";
const cryptokey = "secretkey";
const message = "svc=##SVC##&url=##URL##&ts=##TS##";
let url = "http://localhost:8080/ci/";

/* MAIN */
if (process.argv.length < 4) {
    console.log("usage: node ci-notify.js svc url");
    return;
}

let [n, s, pSvc, pUrl] = process.argv;
const utility = new Utility();

let msg = message
    .replace(/##SVC##/, pSvc)
    .replace(/##URL##/, pUrl)
    .replace(/##TS##/, Date.now());
console.log(msg);

url += utility.encrypt(msg);
http.get(url, (response) => {
    let body = '';
    response
        .on('data', function(data) {
            body += data;
        })
        .on('end', function() {
            console.log(body);
        });
});


function Utility() {
    this.encrypt = function (text) {
        let token = '';
        if (text) {
            const cipher = crypto.createCipher(alg, this.hash(cryptokey));
            token = cipher.update(text + '', 'utf8', 'hex');
            token += cipher.final('hex');
        }
        return token;
    };
    this.hash = function (text) {
        var result = '';
        if (text) {
            result = crypto.createHash('sha1').update(text).digest("hex");
        }
        return result;
    };
}
