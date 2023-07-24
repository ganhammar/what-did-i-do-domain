import { FetchBase } from '@wdid/shared';

export class TagService extends FetchBase {
  baseUrl = `${process.env.REACT_APP_BASE_API_URL}/tag`;
  private readonly accessToken: string;

  constructor(accessToken: string) {
    super();

    this.accessToken = accessToken;
  }

  async list(accountId: string) {
    return await this.get<Tag[]>(
      `${this.baseUrl}?accountId=${accountId}`,
      this.accessToken
    );
  }
}
