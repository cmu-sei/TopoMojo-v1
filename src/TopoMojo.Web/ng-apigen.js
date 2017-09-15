#!/usr/local/bin/node

/*
 * Angular Api Module Generator
 * jmattson@sei.cmu.edu
 *
 * Usage: ng-apigen.js -u url -o outputPath [-h help]
 *    or: node ng-apigen.js -u url -o output [-h help]
*/
var http = require('http');
var https = require('https');
var fs = require('fs');
var fspath = require('path');

const GenerateAngularClient = function(argv) {

    function CreateClient(api, output = "api-module") {

        //make folder
        fs.mkdir(output, function(err) {});

        //write api-models.ts
        fs.writeFile(
            fspath.join(output, "api-models.ts"),
            GenerateModels(api.definitions),
            (err) => {
                if (err) console.log(err.message);
            }
        );

        //generate metadata
        let meta = GenerateMetadata(api);

        //write tag.service.ts files
        for (var svc in meta) {
            fs.writeFile(
                fspath.join(output, svc.toLowerCase() + ".service.ts"),
                GenerateServices(meta, svc, api.paths),
                (err) => {
                    if (err) console.log(err.message);
                }
            );
        }

        //write api.module.ts
        fs.writeFile(
            fspath.join(output, "api.module.ts"),
            GenerateModule(meta),
            (err) => {
                if (err) console.log(err.message);
            }
        );

        //write url-helper.ts
        fs.writeFile(
            fspath.join(output, "url-helper.ts"),
            ngUrlHelper,
            (err) => {
                if (err) console.log(err.message);
            }
        );
    }

    function GenerateModule(meta) {
        let imports = "";
        let svcs = [];
        for (let svc in meta) {
            let sn = svc + "Service";
            svcs.push(sn);
            imports += "import { " + sn + " } from \"./" + svc.toLowerCase() + ".service\";" + crlf;
        }
        return imports + ngModule.replace(/##SVCS##/, svcs.join("," + crlf + "\t\t"));
    }

    function GenerateMetadata(api) {
        let meta = { };

        //gather refs
        for (let path in api.paths) {
            for (let method in api.paths[path]) {
                let operation = api.paths[path][method];
                let tag = operation["tags"][0];
                if (!meta[tag]) meta[tag] = [];
                meta[tag].push(...FindReferenceTypes(api, operation.parameters));
                meta[tag].push(...FindEnumTypes(path, operation.parameters));
                meta[tag].push(...FindReferenceTypes(api, operation.responses));
            }
        }

        //distinct and transform
        for (let t in meta) {
            meta[t] = meta[t]
                .filter(
                    (e, i, a) => {
                        return a.indexOf(e) == i;
                    }
                )
                .map(
                    (e) => {
                        return TransformRef(e);
                    }
                );
        }
        return meta;
    }

    function GenerateModels(models) {
        let text = '';
        let enums = {};
        for (var name in models) {
            let model = models[name];
            text += "export interface " + TransformRef(name) + " {" + crlf;
            for (var p in model.properties) {
                let prop = model.properties[p];
                let type = TransformTypeItem(prop);
                if (prop.enum) {
                    type = TransformRef(name) + p.substring(0,1).toUpperCase() + p.substring(1) + "Enum";
                    if (!enums[type]) enums[type] = prop.enum;
                }
                text += "\t" + p + "?: " + type + ";" + crlf;
            }
            text += "}" + crlf + crlf;
        }

        for (var e in enums) {
            text += "export enum " + e + " {" + crlf;
            for (v in enums[e])
                text += "\t" + enums[e][v] + " = <any>'" + enums[e][v] + "'" + (((+v)<enums[e].length-1) ? "," : "") + crlf;
            text += "}" + crlf + crlf;
        }
        return text;
    }

    function GenerateServices(meta, svc, paths) {
        let text = ngServiceHeader
            .replace(/##SVC##/, svc)
            .replace(/##REFS##/, meta[svc]);
        for (let path in paths) {
            for (let method in paths[path]) {
                let operation = paths[path][method];
                if (operation.tags[0] == svc) {
                    let opn = OperationName(operation.operationId);
                    let opi = OperationInput(operation.parameters);
                    let opr = OperationResponse(operation.responses);
                    let opb = OperationBody(path, method, operation.parameters);
                    text += "\tpublic " + opn + "(" + opi + ") : " + opr + " {" + crlf; //" + operation.operationId);
                    text += "\t\treturn " + opb + crlf;
                    text += "\t}" + crlf;
                }
            }
        }
        text += "}" + crlf;
        return text;
    }

    function OperationName(op) {
        let p = op
            .replace(/By[A-Z][a-z]+/g, "")
            .replace(/([A-Z])/g, " $1")
            .trim()
            .split(" ");
        let object = "", action = "";
        //console.log(p);
        if (p.length > 2) {
            object = p.splice(0,2).pop();
            //console.log(p);
            action = p.shift();
            //console.log(p);
        } else {
            object = p[0];
            action = p[1];
        }
        //console.log( action + object);
        return action.substring(0,1).toLowerCase() + action.substring(1) + object;
    }

    function OperationInput(params) {
        var opi = "";
        if (params) {
            for (let i in params) {
                opi += params[i].name + ": " + TransformTypeItem(params[i]);
                if (i < params.length-1) opi += ", ";
                if (params[i].name == "Skip" && params[i].in == "query") {
                    opi = "search : Search";
                    break;
                }
            }
        }
        return opi;
    }

    // function OperationInputType(param) {
    //     var opit = TransformType(param.type);
    //     if (param.schema && param.schema.$ref) opit = TransformRef(param.schema.$ref);
    //     if (opit == "array") {
    //         if (param.items.$ref) opit = "Array<" + TransformRef(param.items.$ref) + ">";
    //         if (param.items.type) opit = "Array<" + TransformType(param.items.type) + ">";
    //     }
    //     return opit;
    // }

    function OperationResponse(response) {
        var rt = "";
        for (var k in response) {
            var r = response[k];
            if (r.description == "Success") {
                rt = (r.schema)
                    ? TransformTypeItem(r.schema)
                    : "any";
            }
        }
        return "Observable<" + rt + ">";
    }

    function OperationBody(path, op, params) {
        path = path.replace(/\{(\w+)\}/g, '" + $1 + "');
        let bodyparam = "";
        if (params) {
            for (let i in params) {
                if (params[i].in == "body") bodyparam = ", " + params[i].name;
                if (params[i].name == "Skip" && params[i].in == "query") {
                    bodyparam = " + UrlHelper.queryStringify(search)";
                    break;
                }
            }
        }
        return ("this.http." + op + '("' + path + '"' + bodyparam + ');').replace(/ \+ \"\"/, "");
    }

    function FindEnumTypes(path, props) {
        let refs = [];
        path = path.substring(path.lastIndexOf('.'+1));
        for (let prop in props) {
            //console.log(prop);
            if (props[prop].enum)
                refs.push(path+prop+"Enum");
        }
        return refs;
    }

    function FindReferenceTypes(api, args) {
        let refs = [];
        if (args) {
            for (let i in args) {
                let pref = FindRefItem(args[i]);
                if (pref) {
                    refs.push(pref);
                    let c = pref.split('/').pop();
                    let t = api.definitions[c];
                    for (let cp in t.properties) {
                        let cpr = t.properties[cp];
                        if (cpr.enum) refs.push(TrimNamespace(c) + cp.substring(0,1).toUpperCase() + cp.substring(1) + "Enum");
                        let ref = FindRefItem(cpr);
                        if (ref) refs.push(ref);
                    }
                }
            }
        }
        return refs;
    }

    function FindRefItem(item) {
        if (item.$ref)
            return FindRefItem(item.$ref);

        if (item.schema)
            return FindRefItem(item.schema);

        if (item.type && item.type == "array")
            return FindRefItem(item.items || item.schema);

        if (typeof(item) == "string")
            return item;

        return "";
    }

    function TransformTypeItem(item) {
        //console.log(item);
        if (item.$ref)
            return TransformRef(item.$ref);

        if (item.schema)
            return TransformTypeItem(item.schema);

        if (item.type) {
            if (item.type == "array")
                return "Array<" + TransformTypeItem(item.items || item.schema) + ">";

            return TransformType(item.type);
        }
        return "";
    }

    function TransformType(type) {
        switch (type) {
            case "integer":
                type = "number";
                break;
        }
        return type;
    }

    function TransformRef(ref) {
        var f = ref.split(',')[0]
        .substring(ref.lastIndexOf('/') + 1)
        .replace(/\`\d+\[\[/, "-")
        .split("-");

        return (f.length > 1)
            ? TrimNamespace(f[1]) + TrimNamespace(f[0])
            : TrimNamespace(f[0]);
    }

    function TrimNamespace(ns) {
        ns = ns.replace(/.+Models\./, "").split(".");
        let r = ns[0];
        if (ns.length > 1) {
            r = (ns[ns.length-1].match(ns[ns.length-2]))
                ? ns[ns.length-1]
                : ns[ns.length-2] + ns[ns.length-1];
        }
        return r;
    }

    function showUsage() {
        console.log(`usage: ${argv[1]} [options]`);
        console.log("  options:");
        console.log("    -u, --url url      Url or Filepath to url-to-swagger-v2-json");
        console.log("    -o, --output path  Saves api client files in path");
        console.log("    -h, --help         Shows this usage info");
        console.log();
    }

    function processArgs(argv) {
        let keys = [ "-u", "-o", "-h"];
        let names = [ "url", "output", "help"];
        let args = {};
        argv.forEach(
            (v, i, a) => {
                if (v.startsWith("-")) {
                    let match = v.match(/-[a-z]/);
                    if (match) {
                        let key = keys.indexOf(match[0]);
                        if (key != -1) {
                            args[names[key]] = names[key];
                            if (a.length > i+1) args[names[key]] = a[i+1];
                        }
                    }
                }
            }
        )
        return args;
    }

    /* TESTS */
    // let r = "#/definitions/TopoMojo.Core.Models.SearchResult`1[[TopoMojo.Core.Models.Template.TemplateDetail, TopoMojo.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]";
    // console.log("TransformRef should yield: TemplateDetailSearchResult, and did yield: " + TransformRef(r));
    // console.log("TrimNamespace should yield: ChannelSummary, and did yield: " + TrimNamespace("Step.Core.Models.Channel.ChannelSummary"));
    // console.log("TrimNamespace should yield: VirtualTemplate, and did yield: " + TrimNamespace("Step.Core.Models.Virtual.Template"));
    // return;

    /* MAIN */
    let args = processArgs(argv);

    if (!args.help && args.url) {
        if (args.url.startsWith("http")) {
            let client = (args.url.startsWith("https")) ? https : http;
            client.get(args.url, (response) => {
                let body = '';
                response
                    .on('data', function(data) {
                        body += data;
                    })
                    .on('end', function() {
                        CreateClient(JSON.parse(body), args.output);
                    });
            });
        } else {
            fs.readFile(args.url, 'utf8',
                (err, data) => {
                    if (err) throw err;
                    CreateClient(JSON.parse(data), args.output);
                }
            );
        }
    }

    if (args.help || !args.url) {
        showUsage();
    }
}

/* TEMPLATES */
const crlf = "\r\n";
const ngServiceHeader = `
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/core/http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { ##REFS## } from "./api-models";

@Injectable()
export class ##SVC##Service {

    constructor(
        private http: HttpClient
    ) { }

`;

const ngServiceFooter = `
}
`;

const ngModule = `
import { NgModule } from '@angular/core';

@NgModule({
    providers: [
        ##SVCS##
    ]
})
export class ApiModule { }
`;

const ngUrlHelper = `
export class UrlHelper {

    public static queryStringify(obj : any) : string {
        var keys = (obj) ? Object.keys(obj) : [];
        if (keys.length > 0) {
            var segments = [];
            for (var i = 0; i < keys.length; i++) {
                let prop = obj[keys[i]];
                if (prop !== undefined) {
                    if (Array.isArray(prop)) {
                        prop.forEach(element => {
                            segments.push(this.encodeKVP(keys[i], element));
                        });
                    } else {
                        segments.push(this.encodeKVP(keys[i], prop));
                    }
                }
            }
            return "?" + segments.join('&');
        }
        return "";
    }

    private static encodeKVP(key : string, value: string) {
        return encodeURIComponent(key) + "=" + encodeURIComponent(value);
    }
}
`;

GenerateAngularClient(process.argv);
