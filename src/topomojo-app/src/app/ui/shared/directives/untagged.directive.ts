import { Directive, PipeTransform, Pipe } from '@angular/core';

@Pipe({name: 'untagged'})
export class UntaggedStringPipe implements PipeTransform {
    transform(value: string): string {
      return value.split('#').shift();
  }
}
