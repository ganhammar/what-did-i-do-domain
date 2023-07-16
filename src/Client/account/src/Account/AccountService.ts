import { FetchBase } from '@wdid/shared';

export class AccountService extends FetchBase {
  baseUrl = process.env.REACT_APP_BASE_API_URL as string;
  private readonly accessToken: string;

  constructor(accessToken: string) {
    super();

    this.accessToken = accessToken;
  }

  async accounts() {
    return await this.get<Account[]>(this.baseUrl, this.accessToken);
  }

  async create(name: string) {
    return await this.post<Account>(this.baseUrl, { name }, this.accessToken);
  }
}
