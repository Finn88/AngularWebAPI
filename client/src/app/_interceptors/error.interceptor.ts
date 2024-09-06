import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastr = inject(ToastrService);
  return next(req).pipe(
    catchError(error => {
      if (error) {
        switch (error.status) {
          case 400:
            if (error.error.errors) {
              const modalStatesErrors = [];
              for (const key in error.error.errors) {
                if (error.error.errors[key]) {
                  modalStatesErrors.push(error.error.errors[key]);
                }
              }
              throw modalStatesErrors.flat();
            }
            else {
              toastr.error(error.error, error.error.status);
            }
            break;
          case 401:
            toastr.error('Nonauthorised', error.status);
            break;
          case 404:
            router.navigateByUrl('/not-found')
            break;
          case 500:
            const navigationsExtras: NavigationExtras = { state: { error: error.error } }
            router.navigateByUrl('/server-error', navigationsExtras);
            break;
          default:
            toastr.error('Unexpected');
            break;
        }
      }
      throw error;
    })
  )
};
