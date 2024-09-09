import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class PhotoService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  getPhotosForConform() {
    return this.http.get<Photo[]>(`${this.baseUrl}admin/photos-for-confirm`);
  }

  confirmPhoto(photoId: number) {
    return this.http.put(`${this.baseUrl}admin/confirm-photo/${photoId}`, {})  
  }
}
