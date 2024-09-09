import { Component, OnInit, inject, signal } from '@angular/core';
import { Photo } from '../../_models/photo';
import { PhotoService } from '../../_services/photo.service';
import { MembersService } from '../../_services/members.service';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {
  photoService = inject(PhotoService);
  memberService = inject(MembersService);
  photos = signal<Photo[]>([]);

  ngOnInit() { this.getPhotosForConform(); }

  getPhotosForConform() {
    this.photoService.getPhotosForConform()
      .subscribe({
        next: response => {
          this.photos.set(response);
        }
      });
  }

  confirmPhoto(photoId: number) {
    this.photoService.confirmPhoto(photoId)
      .subscribe({
        next: _ => { this.photos.update(list => [...list.filter(p => p.id != photoId)]); }
    })
  }

  deletePhoto(photoId: number) {
    this.memberService.deletePhoto(photoId).subscribe({
      next: _ => { this.photos.update(list => [...list.filter(p => p.id != photoId)]); }
    })
  }
}
