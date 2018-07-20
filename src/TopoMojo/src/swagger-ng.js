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
 * -m, --map dt:at  map derived-type to aggregate-type
 * -n, --ns marker clip namespace after marker (default: 'Models')
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
const http = require('http');
const https = require('https');
const fs = require('fs');
const fspath = require('path');

(function GenerateAngularClient(argv) {

    function CreateClient(api, output = 'api') {

        //make folder
        fs.mkdir(output, function(err) {});
        fs.mkdir(fspath.join(output, 'gen'), function(err) {});

        //generate metadata
        let meta = GenerateMetadata(api);

        //write gen/models.ts
        fs.writeFile(
            fspath.join(output, 'gen', 'models.ts'),
            GenerateModels(meta, api),
            (err) => {
                if (err) console.log(err.message);
            }
        );

        //write gen/_service.ts
        fs.writeFile(
            fspath.join(output, 'gen', '_service.ts'),
            ngBaseService,
            (err) => {
                if (err) console.log(err.message);
            }
        );

        //write gen/tag.service.ts files
        for (const svc in meta.types) {
            fs.writeFile(
                fspath.join(output, 'gen', svc.toLowerCase() + '.service.ts'),
                GenerateServices(meta, svc, api.paths),
                (err) => {
                    if (err) console.log(err.message);
                }
            );
        }

        //add extended service, if it doesn't already exist
        for (let svc in meta.types) {
            let svcPath = fspath.join(output, svc.toLowerCase() + '.service.ts');
            fs.access(
                svcPath,
                (err) => {
                    if (err) {
                        fs.writeFile(
                            svcPath,
                            ngService
                                .replace(/##SVC##/g, svc)
                                .replace(/##svc##/g, svc.toLowerCase())
                                .replace(/##REFS##/, meta.types[svc].join(', ')),
                            (err) => { }
                        );
                    }
                }
            );
        }
        let svcPath = fspath.join(output, 'api-settings.ts');
        fs.access(
            svcPath,
            (err) => {
                if (err) {
                    fs.writeFile(
                        svcPath,
                        ngSettings,
                        (err) => { }
                    );
                }
            }
        );

        //write api.module.ts
        fs.writeFile(
            fspath.join(output, 'gen', 'api.module.ts'),
            GenerateModule(meta),
            (err) => {
                if (err) console.log(err.message);
            }
        );
    }

    function GenerateModule(meta) {
        let imports = '';
        let svcs = [];
        for (let svc in meta.types) {
            let sn = svc + 'Service';
            svcs.push(sn);
            imports += 'import { ' + sn + ' } from \'../' + svc.toLowerCase() + '.service\';' + crlf;
        }
        return imports + ngModule.replace(/##SVCS##/, svcs.join(',' + crlf + tab + tab));
    }

    function GenerateMetadata(api) {
        let meta = { types: {}, derivatives: {} };

        //gather refs
        for (let path in api.paths) {
            for (let method in api.paths[path]) {
                let operation = api.paths[path][method];
                let tag = operation['tags'][0];
                if (!meta.types[tag]) meta.types[tag] = [];
                meta.types[tag].push(...FindReferenceTypes(api, operation.parameters));
                meta.types[tag].push(...FindEnumTypes(path, operation.parameters)); //todo: maybe exclude enums that aggregate into derived type
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
                        return e.normalizeRef();
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

    function GenerateModels(meta, api) {
        let text = '';
        let enums = {};
        let types = [];

        for (const name in api.definitions) {
            let model = api.definitions[name];
            let typeName = name.normalizeRef();
            types.push(typeName);
            text += 'export interface ' + typeName + ' {' + crlf;
            for (const p in model.properties) {
                let prop = model.properties[p];
                let type = findType(prop);
                if (prop.enum) {
                    type = name.normalizeRef() + p.pascalCase() + 'Enum';
                    if (!enums[type]) enums[type] = prop.enum;
                }
                text += tab + p + '?: ' + type + ';' + crlf;
            }
            text += '}' + crlf + crlf;
        }

        //grab any query param enums
        for (let path in api.paths) {
            for (let op in api.paths[path]) {
                if (api.paths[path][op].parameters) {
                    api.paths[path][op].parameters.forEach(
                        (prop) => {
                            if (prop.enum) {
                                let type = prop.name.pascalCase() + 'Enum';
                                if (!enums[type]) enums[type] = prop.enum;
                            }
                        }
                    );
                }
            }
        }

        for (let d in meta.derivatives) {
            if (types.indexOf(d) < 0) {
                let params = meta['derivatives'][d];
                text += 'export interface ' + d + ' {' + crlf;
                for (const p in params) {
                    if (params[p].in == 'query') {
                        let prop = params[p];
                        let type = findType(prop);
                        if (prop.enum) {
                            type = (prop.name || p) + 'Enum';
                            if (!enums[type]) enums[type] = prop.enum;
                        }
                        text += tab + params[p].name.camelCase() + '?: ' + type + ';' + crlf;
                    }
                }
                text += '}' + crlf + crlf;
            }
        }

        for (let e in enums) {
            text += 'export enum ' + e + ' {' + crlf;
            let val = '';
            for (let v in enums[e]) {
                val = enums[e][v].camelCase();
                text += tab + val + ' = <any>\'' + val + '\'' + (((+v)<enums[e].length-1) ? ',' : '') + crlf;
            }
            text += '}' + crlf + crlf;
        }

        return text;
    }

    function GenerateServices(meta, svc, paths) {
        let text = ngGeneratedService
            .replace(/##SVC##/, svc)
            .replace(/##REFS##/, meta.types[svc].join(', '));
        for (let path in paths) {
            for (let method in paths[path]) {
                let operation = paths[path][method];
                if (operation.tags[0] == svc) {
                    let opn = OperationName(operation.operationId);
                    let opi = OperationInput(operation.parameters);
                    let opr = OperationResponse(operation.responses);
                    let opb = OperationBody(path, method, opr, operation.parameters);
                    text += tab + 'public ' + opn + '(' + opi + '): Observable<' + opr + '> {' + crlf; //' + operation.operationId);
                    text += tab + tab + 'return ' + opb + crlf;
                    text += tab + '}' + crlf;
                }
            }
        }
        text += ngGeneratedServiceTail;
        return text;
    }

    function OperationName(op) {
        let p = op
            //.replace(/By[A-Z][a-z]+/g, '')
            .replace(/ById/g, '')
            .replace(/Id/g, '')
            .replace(/([A-Z])/g, ' $1')
            .trim()
            .split(' ');
        let object = '', action = '', prop = '';
        [api, object, ...rest] = p;
        action = rest.pop();
        prop = rest.pop() || '';

        let r = action.camelCase() + object + prop;
        //console.log(op + ' => ' + r);
        return r;
    }

    function OperationInput(params) {
        const opi = [];
        if (params) {
            let r = params
                .filter((p) => { return p.in != 'query'})
                .map((p) => { return p.name + ': ' + findType(p)})
                .concat(...ParamsToInput(params));
            if (r.length > 0) opi.push(...r);
        }
        return opi.join(', ');
    }

    function OperationResponse(response) {
        let rt = '';
        for (const k in response) {
            const r = response[k];
            if (r.description == 'Success') {
                rt = (r.schema)
                    ? findType(r.schema)
                    : 'any';
            }
        }
        return rt;
    }

    function OperationBody(path, op, opr, params) {
        path = path.replace(/\{(\w+)\}/g, '\' + $1 + \'');
        let bodyparam = '';
        if (params) {
            let qs = ParamsToUrl(params);
            if (qs)
                bodyparam = ` + ${qs}`;

            qs = ParamsToBody(params);
            if (!qs && (op=='post' || op=='put')) qs = '{}';
            if (qs)
                bodyparam = `, ${qs}`;
        }
        return `this.http.${op}<${opr}>(this.api.url + '${path}'${bodyparam});`.replace(/ \+ \'\'/, '');
    }

    function ParamsToBody(params) {
        return params.filter(
            (p) => {
                return p.in == 'body';
            }
        ).map((p) => { return p.name; }).join();

    }
    function ParamsToInput(params) {
        let result = [];
        let ps = params
            .filter((p) => {return p.in == 'query';});

        if (ps.length > 0) {
            let type = ps.map((p) => { return p.name }).join('');
            let i = derivedTypeMap.indexOf(type);
            if (i != -1) {
                result.push(`${derivedTypeMap[i+1].camelCase()}: ${derivedTypeMap[i+1]}`);
            } else {
                result.push(ps.map((p)=> { return p.name.camelCase() + ': ' + findType(p)}));
            }
        }
        return result;
    }

    function ParamsToUrl(params) {
        let result = '';
        let ps = params
            .filter((p) => { return p.in == 'query';})
            .map((p) => { return p.name });

        if (ps.length > 0) {
            let i = derivedTypeMap.indexOf(ps.join(''));
            if (i != -1) {
                result = `this.paramify(${derivedTypeMap[i+1].camelCase()})`;
            } else {
                let items = ps.map(
                    (n) => {
                        let i = n.camelCase();
                        return i + ': ' + i;
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
            if (props[prop].enum) {
                refs.push((props[prop].name+'Enum').pascalCase());
            }
        }
        return refs;
    }

    function FindMappedParamType(api, params) {
        let ref = '';
        for (let p in params)
            if (params[p].in == 'query')
                ref += params[p].name;
        let i = derivedTypeMap.indexOf(ref);
        return (i!=-1) ? derivedTypeMap[i+1] : '';
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
                        if (cpr.enum) refs.push(c.trimNS() + cp.pascalCase() + 'Enum');
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

        if (item.type && item.type == 'array')
            return FindRefItem(item.items || item.schema);

        if (typeof(item) == 'string')
            return item;

        return '';
    }

    function findType(item) {
        if (item.enum)
            return (item.name || 'FixMe').pascalCase() + 'Enum';

        if (item.$ref)
            return item.$ref.normalizeRef();

        if (item.schema)
            return findType(item.schema);

        if (item.type) {
            if (item.type == 'array')
                return 'Array<' + findType(item.items || item.schema) + '>';

            return item.type.normalizeType();
        }
        return '';
    }

    String.prototype.normalizeType = function() {
        let type = this+'';
        switch (type) {
            case 'integer':
            case 'short':
            case 'long':
            case 'float':
            case 'single':
            case 'double':
                type = 'number';
                break;
        }
        return type;
    }

    String.prototype.normalizeRef = function() {
        return this.split(',')[0]
        .substring(this.lastIndexOf('/') + 1)
        .match(/([^\[]*)\[?([^\]]*)\]?/)
        .slice(1).reverse().join('').trimNS();
    }

    // String.prototype.normalizeRef = function() {
    //     const f = this.split(',')[0]
    //     .substring(this.lastIndexOf('/') + 1)
    //     .replace(/\`\d+\[\[/, '-')
    //     .split('-');

    //     return (f.length > 1)
    //         ? f[1].trimNS() + f[0].trimNS()
    //         : f[0].trimNS();
    // }

    String.prototype.trimNS = function() {
        ns = this.replace(nsRegex, '').split('.');
        let r = ns[0];
        if (ns.length > 1) {
            r = (ns[ns.length-1].match(ns[ns.length-2]))
                ? ns[ns.length-1]
                : ns[ns.length-2] + ns[ns.length-1];
        }
        return r;
    }

    String.prototype.pascalCase = function() {
        return this.substring(0,1).toUpperCase() + this.substring(1);
    }

    String.prototype.camelCase = function() {
        return this.substring(0,1).toLowerCase() + this.substring(1);
    }

    function showUsage() {
        console.log(`usage: ${argv[1]} [options]`);
        console.log('  options:');
        console.log('    -u, --url url      Url or Filepath to url-to-swagger-v2-json');
        console.log('    -o, --output path  Saves api client files in path');
        console.log('    -m, --map dt:at    map derived-type to aggregate-type');
        console.log('    -n, --ns marker    clip namespace after marker (Default: Models)');
        console.log('    -h, --help         Shows this usage info');
        console.log();
    }

    function processArgs(argv) {
        let keys = [ '-u', '-o', '-m', '-n', '-h'];
        let names = [ 'url', 'output', 'map', 'ns', 'help'];
        let args = {};
        argv.forEach(
            (v, i, a) => {
                if (v.startsWith('-')) {
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
    // let r = '#/definitions/TopoMojo.Core.Models.SearchResult`1[[TopoMojo.Core.Models.Template.TemplateDetail, TopoMojo.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]';
    // console.log('TransformRef should yield: TemplateDetailSearchResult, and did yield: ' + TransformRef(r));
    // console.log('TrimNamespace should yield: ChannelSummary, and did yield: ' + TrimNamespace('Step.Core.Models.Channel.ChannelSummary'));
    // console.log('TrimNamespace should yield: VirtualTemplate, and did yield: ' + TrimNamespace('Step.Core.Models.Virtual.Template'));
    // return;

    /* MAIN */
    let args = processArgs(argv);
    let derivedTypeMap = (args.map) ? args.map.split(':') : []; //[ 'TermSkipTakeSortFilters', 'Search'];
    let nsRegex = new RegExp(`.+${args.ns || 'Models'}\\.`);

    /* TESTS */
    // let r = '#/definitions/TopoMojo.Core.Models.SearchResult`1[[TopoMojo.Core.Models.Template.TemplateDetail, TopoMojo.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]';
    // console.log(r.normalizeRef());
    // return;

    if (!args.help && args.url) {
        if (args.url.startsWith('http')) {
            let client = (args.url.startsWith('https')) ? https : http;
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
const crlf = '\r\n';
const tab = '    ';

const ngModule = `
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { ApiSettings } from '../api-settings';

@NgModule({
    imports: [ HttpClientModule ],
    providers: [
        ApiSettings,
        ##SVCS##
    ]
})
export class ApiModule { }
`;

const ngBaseService = `
import { HttpClient } from '@angular/common/http';
import { ApiSettings } from '../api-settings';

export class GeneratedService {

    constructor(
        protected http: HttpClient,
        protected api: ApiSettings
    ) { }

    protected paramify(obj: any): string {
        const segments: Array<string> = new Array<string>();
        for (const p in obj) {
            if (obj.hasOwnProperty(p)) {
                const prop = obj[p];
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
        }
        const qs = segments.join('&');
        return (qs) ? '?' + qs : '';
    }

    private encodeKVP(key: string, value: string) {
        return encodeURIComponent(key) + '=' + encodeURIComponent(value);
    }
}
`;

const ngGeneratedService = `
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from '../api-settings';
import { GeneratedService } from './_service';
import { ##REFS## } from './models';

@Injectable()
export class Generated##SVC##Service extends GeneratedService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

`;

const ngGeneratedServiceTail = `
}
`;

const ngService = `
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { Generated##SVC##Service } from './gen/##svc##.service';
import { ##REFS## } from './gen/models';

@Injectable()
export class ##SVC##Service extends Generated##SVC##Service {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }
}
`;

const ngSettings= `
import { Injectable } from '@angular/core';

@Injectable()
export class ApiSettings {

    constructor(
    ) {
        this.url = '';
    }

    url: string;
}
`;
