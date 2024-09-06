import { User } from './user'
export class UserParams {
  gender?: string;
  orderBy = "lastActivity"
  minAge = 18;
  maxAge = 150;
  pageNumber = 1;
  pageSize = 5;

  constructor(user: User | null) {
    this.gender = user?.gender === 'female' ? 'male' : 'female';
  }
}
