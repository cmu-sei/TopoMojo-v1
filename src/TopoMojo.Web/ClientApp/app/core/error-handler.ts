import { ErrorHandler } from '@angular/core';
import { Router } from '@angular/router';

export class AppErrorHandler implements ErrorHandler {

    constructor (
        private router : Router
    ) { }

    handleError(err) {
        console.log(err);
        if (err.status === 401) {
            console.log(this.router.url);
            this.router.navigate(['/login'], { queryParams: { url: this.router.url }});
        }
    }
}
