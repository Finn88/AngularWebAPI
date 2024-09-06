import { Component, OnInit, inject, output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { JsonPipe, NgFor, NgIf } from '@angular/common';
import { TextInputComponent } from '../_forms/text-input/text-input.component';
import { DatePickerComponent } from '../_forms/date-picker/date-picker.component';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, JsonPipe, TextInputComponent, NgIf, DatePickerComponent, NgFor],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  private toastr = inject(ToastrService);
  private router = inject(Router);
  cancelReister = output<boolean>();
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  model: any = {};
  registerForm: FormGroup = new FormGroup({});
  maxDate = new Date();
  validationErrors: string[] | undefined;

  ngOnInit() {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      username: ['', Validators.required],
      password: ['',
        [
          Validators.required,
          Validators.minLength(4),
          Validators.maxLength(8)
        ]],
      confirmPassword:['',
        [
          Validators.required,
          this.matchValues("password")
        ]],
    });
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    })
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : { isMatching: true };
    }
  }

  register() {
    const dateOfBirth = this.getDateOnly(this.registerForm.get('dateOfBirth')?.value)
    this.registerForm.patchValue({ dateOfBirth: dateOfBirth });
    this.accountService.register(this.registerForm.value).subscribe({
      next: _ => { this.router.navigateByUrl('/members'); },
      error: error => this.validationErrors = error
    });
  }
  cancel() {
    this.cancelReister.emit(false);
  }

  private getDateOnly(date: string | undefined) {
    if (!date) return;
    return new Date().toISOString().slice(0, 19);
  }
}
