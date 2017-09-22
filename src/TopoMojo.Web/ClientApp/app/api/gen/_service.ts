
import { HttpClient } from "@angular/common/http";

export class GeneratedService {

    constructor(
        protected http : HttpClient
    ){ }

    protected queryStringify(obj : any) : string {
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

