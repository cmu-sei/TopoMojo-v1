import { Pipe, PipeTransform } from '@angular/core';
/*
 * Return string truncated at '#'
*/
@Pipe({name: 'untagged'})
export class UntaggedStringPipe implements PipeTransform {
    transform(value: string): string {
        let x = value.indexOf('#');
        if (x >= 0)
            return value.substring(0, x);
        else
            return value;
  }
}