import { FetchBase } from '../../infrastructure/FetchBase';

export interface RegisterParamters {
  email: string;
  password: string;
  returnUrl: string;
}

export interface LoginParameters {
  email: string;
  userName?: string;
  password: string;
  rememberMe: boolean;
}

export interface SignInResult {
  succeeded: boolean;
  isLockedOut: boolean;
  isNotAllowed: boolean;
  requiresTwoFactor: boolean;
}

export class UserService extends FetchBase {
  baseUrl = `${process.env.REACT_APP_API_URL}/user`;

  async register(data: RegisterParamters) {
    return await this.post<User>(`${this.baseUrl}/register`, data);
  }

  async login(data: LoginParameters) {
    return await this.post<SignInResult>(`${this.baseUrl}/login`, data);
  }
}
