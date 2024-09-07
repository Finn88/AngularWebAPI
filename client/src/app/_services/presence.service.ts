import { Injectable, inject, signal } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { User } from '../_models/user';
import { take } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  private hubUrl = environment.hubsUrl;
  private hubConnections?: HubConnection;
  private toasts = inject(ToastrService);
  private router = inject(Router);
  onlineUsers = signal<string[]>([]);

  createHubConnnection(user: User) {
    this.hubConnections = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}presence`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnections.start().catch(er => console.log(er));
    this.hubConnections.on('UserIsOnline', username => {
      this.onlineUsers.update(users => [...users, username]);
    });
    this.hubConnections.on('UserIsOffline', username => {
      this.onlineUsers.update(users => users.filter(x => x !== username));
    });
    this.hubConnections.on('GetOnlineUsers', usernames => {
      this.onlineUsers.set(usernames);
    });
    this.hubConnections.on('NewMessageRecieved', (username, knownAs) => {
      this.toasts.info(`${knownAs} has sent you a knew message. Click me to see it`)
        .onTap
        .pipe(take(1))
        .subscribe(() => this.router.navigateByUrl(`/members/${username}?tab=Messages`));
    });
  }

  stopHubConnection() {
    if (this.hubConnections?.state === HubConnectionState.Connected) {
      this.hubConnections.stop().catch(er => console.log(er));
    }
  }
}
