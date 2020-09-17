import { catchError } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { AlertifyService } from '../_services/alertify.service';
import { UserService } from '../_services/user.service';
import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router';

@Injectable()
export class MemberListResolver implements Resolve<User[]> {

    pageNumber = 1;
    paggeSize = 5;

    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService){}

    resolve(route: ActivatedRouteSnapshot): Observable<User[]>{
      return  this.userService.getUsers(this.pageNumber, this.paggeSize).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving data');
                this.router.navigate(['/home']);

                return of(null);
            })
        );
    }
}