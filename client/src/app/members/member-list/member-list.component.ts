import { Component, OnInit, inject } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { MemberCardComponent } from '../member-card/member-card.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { NgIf } from '@angular/common';
import { AccountService } from '../../_services/account.service';
import { UserParams } from '../../_models/userParams';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [MemberCardComponent, PaginationModule, NgIf, FormsModule, ButtonsModule],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css'
})
export class MemberListComponent implements OnInit {
  private accountService = inject(AccountService);
  memberService = inject(MembersService);
  genderList = [
    { value: "male", display: "Males" },
    { value: "female", display: "Females" }
  ]

  ngOnInit(): void {
    if (!this.memberService.paginatedResult()) {
      this.loadMembers()
    }
  }

  loadMembers() {
    this.memberService.getMembers();
  }
  
  resetFilters() {
    this.memberService.resetUserParams();
    this.loadMembers();
  }



  pageChanged(event: any) {
    if (this.memberService.userParams().pageNumber != event.page) {
      this.memberService.userParams().pageNumber = event.page;
      this.loadMembers();
    }
  }
}
