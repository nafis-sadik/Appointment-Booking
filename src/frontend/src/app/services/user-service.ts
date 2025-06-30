import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { environment } from '../../environments/environment';
import { UserModel } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseUrl = environment.apiBaseUrl;
  constructor(private http: HttpClient) {}

  login(userModel: UserModel): Observable<any> {
    const url = `${this.baseUrl}/api/login`; // Replace with your actual endpoint
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.http.post(url, userModel, { headers });
  }
}
