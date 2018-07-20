import { Pipe, PipeTransform } from '@angular/core';
/*
 * Return string truncated at '#'
*/
@Pipe({name: 'untagged'})
export class UntaggedStringPipe implements PipeTransform {
    transform(value: string): string {
        const x = value.indexOf('#');
        if (x >= 0) {
            return value.substring(0, x);
        } else {
            return value;
        }
  }
}

/*
 * Return string truncated at '#'
*/
@Pipe({name: 'aserror'})
export class FormatErrorPipe implements PipeTransform {
    transform(value: string): string {
        const key = value.match(/.*(EXCEPTION\.[A-Z]+).*/);
        return (!!key) ? key[1] : value;
  }
}

@Pipe({name: 'ago'})
export class AgedDatePipe implements PipeTransform {
    transform(date: any): string {
        let r = '';
        const n = new Date();
        const t = new Date(date); // + " GMT");
        const tag = [ 's', 'm', 'h', 'd' ];
        // console.log(date);
        // console.log(n);
        // console.log(t);
        let d: number = n.valueOf() - t.valueOf();
        const a: number[] = [
            d / 1000,
            d / 1000 / 60,
            d / 1000 / 60 / 60,
            d / 1000 / 60 / 60 / 24
        ];
        for (let i = 0; i < a.length; i++) {
            d = Math.floor(a[i]);
            if (!!d) {
                r = d + tag[i];
            }
        }
        // console.log(a);
        return r + ' ago';
    }
}
