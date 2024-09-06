import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  standalone: true,
  imports: [],
  templateUrl: './test-errors.component.html',
  styleUrl: './test-errors.component.css'
})
export class TestErrorsComponent {
  baseUrl = "http://localhost:5000/api"
  private http = inject(HttpClient)

  get400Error() {
    this.http.get(`${this.baseUrl}/bugs/bad-request`).subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get401Error() {
    this.http.get(`${this.baseUrl}/bugs/auth`).subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get404Error() {
    this.http.get(`${this.baseUrl}/bugs/not-found`).subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get500Error() {
    this.http.get(`${this.baseUrl}/bugs/server-found`).subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  } 

  get400Validation() {
    this.http.post(`${this.baseUrl}/account/register`, {}).subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  } 
  
}
