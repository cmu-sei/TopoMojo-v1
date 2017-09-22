/**
 * Helper class to decode and find JWT expiration.
 */
export class JwtToken {
  claims: any;
  hash: any;
  method: any;
}
export class JwtHelper {
  token : JwtToken = new JwtToken();

  constructor (
    token : string
  ) {
      this.decodeToken(token);
  }

  public urlBase64Decode(str: string): string {
    let output = str.replace(/-/g, '+').replace(/_/g, '/');
    switch (output.length % 4) {
      case 0: { break; }
      case 2: { output += '=='; break; }
      case 3: { output += '='; break; }
      default: {
        throw 'Illegal base64url string!';
      }
    }
    return this.b64DecodeUnicode(output);
  }

  // credits for decoder goes to https://github.com/atk
  private b64decode(str: string): string {
    let chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';
    let output: string = '';

    str = String(str).replace(/=+$/, '');

    if (str.length % 4 == 1) {
      throw new Error("'atob' failed: The string to be decoded is not correctly encoded.");
    }

    for (
      // initialize result and counters
      let bc: number = 0, bs: any, buffer: any, idx: number = 0;
      // get next character
      buffer = str.charAt(idx++);
      // character found in table? initialize bit storage and add its ascii value;
      ~buffer && (bs = bc % 4 ? bs * 64 + buffer : buffer,
        // and if not first of each 4 characters,
        // convert the first 8 bits to one ascii character
        bc++ % 4) ? output += String.fromCharCode(255 & bs >> (-2 * bc & 6)) : 0
    ) {
      // try to find character in table (0-63, not found => -1)
      buffer = chars.indexOf(buffer);
    }
    return output;
  }

  // https://developer.mozilla.org/en/docs/Web/API/WindowBase64/Base64_encoding_and_decoding#The_Unicode_Problem
  private b64DecodeUnicode(str: any) {
    return decodeURIComponent(Array.prototype.map.call(this.b64decode(str), (c: any) => {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
  }

  public decodeToken(token: string): any {
    let parts = token.split('.');

    if (parts.length !== 3) {
      throw new Error('JWT must have 3 parts');
    }

    // let decoded0 = this.urlBase64Decode(parts[0]);
    // this.token.method = JSON.parse(decoded0);

    let decoded = this.urlBase64Decode(parts[1]);
    if (!decoded) {
      throw new Error('Cannot decode the token');
    }
    this.token.claims = JSON.parse(decoded);

    return this.token.claims;
  }

  public getTokenExpirationDate(token: string): Date {
    let decoded: any;
    decoded = this.decodeToken(token);

    if (!decoded.hasOwnProperty('exp')) {
      return null;
    }

    let date = new Date(0); // The 0 here is the key, which sets the date to the epoch
    date.setUTCSeconds(decoded.exp);

    return date;
  }

  public isExpired() {
    if (this.token.claims.hasOwnProperty('exp')) {
      let exp = new Date(0).setUTCSeconds(this.token.claims.exp);
      return !(exp.valueOf() > (new Date().valueOf()));
    }
    return false;
  }

  public isExpiring() {
    let now = new Date().valueOf();

    if (this.token.claims.hasOwnProperty('exp')
      && this.token.claims.hasOwnProperty('nbf')) {
      let exp = new Date(0).setUTCSeconds(this.token.claims.exp);
      let nbf = new Date(0).setUTCSeconds(this.token.claims.nbf);
      let hl = exp - (exp-nbf)/2;
      return ((now < exp.valueOf()) && (now > hl.valueOf()));
    }
    return false;
  }

  public isAdmin() {
    return (this.token.claims.tmad === 'True');
  }

  public isTokenExpired(token: string, offsetSeconds?: number): boolean {
    let date = this.getTokenExpirationDate(token);
    offsetSeconds = offsetSeconds || 0;

    if (date == null) {
      return false;
    }

    // Token expired?
    return !(date.valueOf() > (new Date().valueOf() + (offsetSeconds * 1000)));
  }
}