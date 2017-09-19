#!/bin/node
/*
 * This script wrangles ci publish packages.
 * It validates the ci-nudge messages and initiates the publish script
 *
*/
const fs = require('fs');
const crypto = require('crypto');
const http = require('http');
const querystring = require('querystring');
const exec = require('child_process').exec;
const cryptokey = "secretkey";
const target = "echo";
const utility = new Utility();

http.createServer(
    (request, response) => {
        console.log(request.url);
        request.on('error', function (err) {
            console.error(err);
            response.statusCode = 400;
            response.end();
        });

        try {
            let data = utility.params(
                utility.decrypt(
                    request.url.split("/").pop()
                )
            );
            //console.log(data);
            if (utility.isValid(data)) {
                var cmd = `${target} ${data.svc} ${data.url}`;
                //console.log(cmd);
                var child = exec(cmd, function (err, stdout, stderr) {
                    console.log("fired: " + cmd);
                });
                response.status = 200;
                response.end("Got it, thanks.", "utf8");
            } else {
                throw new Error();
            }
        } catch (err) {
            console.error(err.message);
            response.statusCode = 400;
            response.end();
        }
    }
).listen(8080);
console.log("listening on port 8080");

function Utility() {
    this.encrypt = function(text) {
        let token = '';
        if (text) {
            const cipher = crypto.createCipher('aes128', this.hash(cryptokey));
            token = cipher.update(text + '', 'utf8', 'hex');
            token += cipher.final('base64');
        }
        return token;
    };
    this.decrypt = function(token) {
        let text = '';
        if (token) {
            const decipher = crypto.createDecipher('aes128', this.hash(cryptokey));
            text = decipher.update(token, 'hex', 'utf8');
            text += decipher.final('utf8');
        }
        return text;
    };
    this.hash =  function(text) {
        var result = '';
        if (text) {
            result = crypto.createHash('sha1').update(text).digest("hex");
        }
        return result;
    };
    this.params = function(data) {
        let result = {};
        data.replace(/[^?]*?/, "").split('&').forEach(
            (v, i) => {
                let kvp = v.split('=');
                if (kvp[0]) result[kvp[0]] = (kvp.length > 1) ? kvp[1] : null;
            }
        );
        return result;
    };
    this.isValid = function(data) {
        return data.svc && data.url && data.ts && (Date.now()-data.ts<1E4);
    };
}
