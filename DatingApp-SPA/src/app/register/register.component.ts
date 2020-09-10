import { Router } from '@angular/router';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { User } from '../_models/user';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  user: User;
  registerForm: FormGroup;
  constructor(private auth: AuthService, private alertify: AlertifyService, private fb: FormBuilder, private router: Router) { }
  @Output() cancelRegister = new EventEmitter();
  bsConfig: Partial<BsDatepickerConfig>;


  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-red'
    },
   this.createRegisterForm();
  }


  register() {
    if (this.registerForm.valid) {
      this.user = Object.assign({}, this.registerForm.value);
      this.auth.register(this.user).subscribe(() => {
        this.alertify.success('Registration successul');
      }, error => {
        this.alertify.error('Failed to registar');
      }, () => {
        this.auth.login(this.user).subscribe(() => {
          this.router.navigate( ['/members']);
        });
      });

    }

  }

  cancel() {
    this.cancelRegister.emit(false);

  }

  passwordMatchValidator(confirm: FormGroup) {
    return confirm.get('password').value === confirm.get('confirmPassword').value ? null : {'mismatch': true};
  }

  createRegisterForm(){
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator});
  }

}
