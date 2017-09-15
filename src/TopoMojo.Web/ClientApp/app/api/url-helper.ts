
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
