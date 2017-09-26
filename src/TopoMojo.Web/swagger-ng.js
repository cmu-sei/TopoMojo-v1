#!/usr/local/bin/node
/*
 * Api Client Generator: Swagger-v2 to Angular Module
 * jmattson@sei.cmu.edu
 *
 * Usage: swagger-ng.js -u url -o output -m map -n ns -h help
 *    or: node swagger-ng.js -u url -o output -m map -n ns -h help
 * Options:
 * -u, --url path to the swagger-v2 json file
 * -o, --out path of the destination folder
 * -m, --map DerivedType:AggregateType
 * -n, --ns  a parent namespace to exlude (default: 'Models')
 * -h, --help displays usage information
 *
 * Swagger v2 has no contruct for an Object querystring parameter.
 * As such, Swashbuckle decomposes a [FromQuery] object into separate
 * parameters for each property.  To reconstitute the object at the
 * client, provide a type mapping on the command line.
 *
 * I.e. -m TermSkipTakeSortFilter:Search
 *
 * By default, a type is prefixed with its parent namespace segement; unless
 * that happens to match `Models` or the value presented with the --ns argument.
 *
 * I.e. `Models.Item` produces `Item`
 * I.e. `Different.Item` produces `DifferentItem`
 *
*/
var http = require('http');
var https = require('https');
var fs = require('fs');
var fspath = require('path');

(function GenerateAngularClient(argv) {

    function CreateClient(api, output = "api-module") {

        //make folder
        fs.mkdir(output, function(err) {});
        fs.mkdir(fspath.join(output, "gen"), function(err) {});

        //generate metadata
        let meta = GenerateMetadata(api);

        //write gen/models.ts
        fs.writeFile(
            fspath.join(output, "gen", "models.ts"),
            GenerateModels(meta, api.definitions),
            (err) => {
                if (err) console.log(err.message);
            }
        );

        //write gen/_service.ts
        fs.writeFile(
            fspath.join(output, "gen", "_service.ts"),
            ngBaseService,
            (err) => {
                if (err) console.log(err.message);
            }
        );

        //write gen/tag.service.ts files
        for (var svc in meta.types) {
            fs.writeFile(
                fspath.join(output, "gen", svc.toLowerCase() + ".service.ts"),
                GenerateServices(meta, svc, api.paths),
                (err) => {
                    if (err) console.log(err.message);
                }
            );
        }

        //add extended service, if it doesn't already exist
        for (let svc in meta.types) {
            let svcPath = fspath.join(output, svc.toLowerCase() + ".service.ts");
            fs.access(
                svcPath,
                (err) => {
                    if (err) {
                        fs.writeFile(
                            svcPath,
                            ngService
                                .replace(/##SVC##/g, svc)
                                .replace(/##svc##/g, svc.toLowerCase())
                                .replace(/##REFS##/, meta.types[svc]),
                            (err) => { }
                        );
                    }
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
    }

    function GenerateModule(meta) {
        let imports = "";
        let svcs = [];
        for (let svc in meta.types) {
            let sn = svc + "Service";
            svcs.push(sn);
            imports += "import { " + sn + " } from \"./" + svc.toLowerCase() + ".service\";" + crlf;
        }
        return imports + ngModule.replace(/##SVCS##/, svcs.join("," + crlf + "\t\t"));
    }

    function GenerateMetadata(api) {
        let meta = { types: {}, derivatives: {} };

        //gather refs
        for (let path in api.paths) {
            for (let method in api.paths[path]) {
                let operation = api.paths[path][method];
                let tag = operation["tags"][0];
                if (!meta.types[tag]) meta.types[tag] = [];
                meta.types[tag].push(...FindReferenceTypes(api, operation.parameters));
                meta.types[tag].push(...FindEnumTypes(path, operation.parameters));
                meta.types[tag].push(...FindReferenceTypes(api, operation.responses));
                let qpt = FindMappedParamType(api, operation.parameters);
                if (qpt) {
                    meta.types[tag].push(qpt);
                    if (!meta.derivatives[qpt])
                    meta.derivatives[qpt] = operation.parameters;
                }
            }
        }

        //distinct and transform
        for (let t in meta.types) {
            meta.types[t] = meta.types[t]
                .map(
                    (e) => {
                        return TransformRef(e);
                    }
                )
                .filter(
                    (e, i, a) => {
                        return a.indexOf(e) == i;
                    }
                ).sort();
        }
        return meta;
    }

    function GenerateModels(meta, models) {
        let text = '';
        let enums = {};
        let types = [];

        for (var name in models) {
            let model = models[name];
            let typeName = TransformRef(name);
            types.push(typeName);
            text += "export interface " + typeName + " {" + crlf;
            for (var p in model.properties) {
                let prop = model.properties[p];
                let type = TransformTypeItem(prop);
                if (prop.enum) {
                    type = TransformRef(name) + PascalCase(p) + "Enum";
                    if (!enums[type]) enums[type] = prop.enum;
                }
                text += "\t" + p + "?: " + type + ";" + crlf;
            }
            text += "}" + crlf + crlf;
        }

        for (let e in enums) {
            text += "export enum " + e + " {" + crlf;
            for (let v in enums[e])
                text += "\t" + enums[e][v] + " = <any>'" + enums[e][v] + "'" + (((+v)<enums[e].length-1) ? "," : "") + crlf;
            text += "}" + crlf + crlf;
        }

        //console.log(meta);
        for (let d in meta.derivatives) {
            if (types.indexOf(d) < 0) {
                let params = meta["derivatives"][d];
                text += "export interface " + d + " {" + crlf;
                for (var p in params) {
                    if (params[p].in == "query") {
                        let prop = params[p];
                        let type = TransformTypeItem(prop);
                        if (prop.enum) {
                            type = TransformRef(name) + PascalCase(p) + "Enum";
                            if (!enums[type]) enums[type] = prop.enum;
                        }
                        text += "\t" + CamelCase(params[p].name) + "?: " + type + ";" + crlf;
                    }
                }
                text += "}" + crlf + crlf;
            }
        }

        return text;
    }

    function GenerateServices(meta, svc, paths) {
        let text = ngGeneratedService
            .replace(/##SVC##/, svc)
            .replace(/##REFS##/, meta.types[svc]);
        for (let path in paths) {
            for (let method in paths[path]) {
                let operation = paths[path][method];
                if (operation.tags[0] == svc) {
                    let opn = OperationName(operation.operationId);
                    let opi = OperationInput(operation.parameters);
                    let opr = OperationResponse(operation.responses);
                    let opb = OperationBody(path, method, opr, operation.parameters);
                    text += "\tpublic " + opn + "(" + opi + ") : Observable<" + opr + "> {" + crlf; //" + operation.operationId);
                    text += "\t\treturn " + opb + crlf;
                    text += "\t}" + crlf;
                }
            }
        }
        text += ngGeneratedServiceTail;
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
        return CamelCase(action) + object;
    }

    function OperationInput(params) {
        var opi = [];
        if (params) {
            let r = params
                .filter((p) => { return p.in != "query"})
                .map((p) => { return p.name + ": " + TransformTypeItem(p)})
                .concat(...ParamsToInput(params));
            if (r.length > 0) opi.push(...r);
        }
        return opi.join(', ');
    }

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
        return rt;
    }

    function OperationBody(path, op, opr, params) {
        path = path.replace(/\{(\w+)\}/g, '" + $1 + "');
        let bodyparam = "";
        if (params) {
            let qs = ParamsToUrl(params);
            if (qs)
                bodyparam = ` + ${qs}`;

            qs = ParamsToBody(params);
            if (qs)
                bodyparam = `, ${qs}`;

            // for (let i in params) {
            //     if (params[i].in == "body") bodyparam = ", " + params[i].name;
            //     if (params[i].name == "Skip" && params[i].in == "query") {
            //         bodyparam = " + this.queryStringify(search)";
            //         break;
            //     }
            // }
        }
        return `this.http.${op}<${opr}>("${path}"${bodyparam});`.replace(/ \+ \"\"/, "");
        //return ("this.http." + op + '("' + path + '"' + bodyparam + ');').replace(/ \+ \"\"/, "");
    }

    function ParamsToBody(params) {
        return params.filter(
            (p) => {
                return p.in == "body";
            }
        ).map((p) => { return p.name; }).join();

    }
    function ParamsToInput(params) {
        let result = [];
        let ps = params
            .filter((p) => {return p.in == "query";});

        if (ps.length > 0) {
            let type = ps.map((p) => { return p.name }).join('');
            let i = derivedTypeMap.indexOf(type);
            if (i != -1) {
                result.push(`${CamelCase(derivedTypeMap[i+1])}: ${derivedTypeMap[i+1]}`);
            } else {
                result.push(ps.map((p)=> { return CamelCase(p.name) + ": " + TransformTypeItem(p)}));
            }
        }
        return result;
    }
    function GenerateParamModels(paths) {
        let result = "";
        for (let path in paths) {
            for (let method in paths[path]) {
                let op = paths[path][method];

            }
        }
        // if (ps.length > 0) {
        //     let type = ps.map((p) => { return p.name }).join('');
        //     let i = qsmap.indexOf(type);
        //     if (i != -1) {
        //         result.push(`${CamelCase(qsmap[i+1])}: ${qsmap[i+1]}`);
        //     } else {
        //         result.push(ps.map((p)=> { return CamelCase(p.name) + ": " + TransformTypeItem(p)}));
        //     }
        // }
        return result;
    }

    function ParamsToUrl(params) {
        let result = "";
        let ps = params
            .filter((p) => { return p.in == "query";})
            .map((p) => { return p.name });

        if (ps.length > 0) {
            let i = derivedTypeMap.indexOf(ps.join(''));
            if (i != -1) {
                result = `this.paramify(${CamelCase(derivedTypeMap[i+1])})`;
            } else {
                let items = ps.map(
                    (n) => {
                        let i = CamelCase(n);
                        return i + ": " + i;
                    }
                ).join(', ');
                result = `this.paramify({${items}})`;
            }
        }
        return result;
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

    function FindMappedParamType(api, params) {
        let ref = "";
        for (let p in params)
            if (params[p].in == "query")
                ref += params[p].name;
        let i = derivedTypeMap.indexOf(ref);
        return (i!=-1) ? derivedTypeMap[i+1] : "";
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
                        if (cpr.enum) refs.push(TrimNamespace(c) + PascalCase(cp) + "Enum");
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
        ns = ns.replace(nsRegex, "").split(".");
        let r = ns[0];
        if (ns.length > 1) {
            r = (ns[ns.length-1].match(ns[ns.length-2]))
                ? ns[ns.length-1]
                : ns[ns.length-2] + ns[ns.length-1];
        }
        return r;
    }

    function CamelCase(s) {
        return s.substring(0,1).toLowerCase() + s.substring(1);
    }

    function PascalCase(s) {
        return s.substring(0,1).toUpperCase() + s.substring(1);
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
        let keys = [ "-u", "-o", "-m", "-n", "-h"];
        let names = [ "url", "output", "map", "ns", "help"];
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
    let derivedTypeMap = (args.map) ? args.map.split(':') : []; //[ "TermSkipTakeSortFilters", "Search"];
    let nsRegex = new RegExp(`.+${args.ns || 'Models'}\\.`);

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
})(process.argv);

/* TEMPLATES */
const crlf = "\r\n";

const ngModule = `
import { NgModule } from '@angular/core';
import { HttpClientModule } from "@angular/common/http";

@NgModule({
    imports: [ HttpClientModule ],
    providers: [
        ##SVCS##
    ]
})
export class ApiModule { }
`;

const ngBaseService = `
import { HttpClient } from "@angular/common/http";

export class GeneratedService {

    constructor(
        protected http : HttpClient
    ){ }

    protected paramify(obj : any) : string {
        var segments = [];
        for (let p in obj) {
            let prop = obj[p];
            if (prop) {
                if (Array.isArray(prop)) {
                    prop.forEach(element => {
                        segments.push(this.encodeKVP(p, element));
                    });
                } else {
                    segments.push(this.encodeKVP(p, prop));
                }
            }
        }
        let qs = segments.join('&');
        return (qs) ? "?" + qs : "";
    }

    private encodeKVP(key : string, value: string) {
        return encodeURIComponent(key) + "=" + encodeURIComponent(value);
    }
}

`;

const ngGeneratedService = `
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedService } from "./_service";
import { ##REFS## } from "./models";

@Injectable()
export class Generated##SVC##Service extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

`;

const ngGeneratedServiceTail = `
}

`;

const ngService = `
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { Generated##SVC##Service } from "./gen/##svc##.service";
import { ##REFS## } from "./gen/models";

@Injectable()
export class ##SVC##Service extends Generated##SVC##Service {

    constructor(
       protected http: HttpClient
    ) { super(http); }
}
`;
