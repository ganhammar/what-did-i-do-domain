import { FetchBase } from '@wdid/shared';

export interface ListParamters {
  accountId: string;
  limit: number;
  fromDate?: string;
  toDate?: string;
}

export class EventService extends FetchBase {
  baseUrl = `${process.env.REACT_APP_BASE_API_URL}/event`;
  private readonly accessToken: string;

  constructor(accessToken: string) {
    super();

    this.accessToken = accessToken;
  }

  async list({ accountId, limit, fromDate, toDate }: ListParamters) {
    let url = `${this.baseUrl}?accountId=${accountId}&limit=${limit}`;

    if (fromDate && toDate) {
      url += `&fromDate=${fromDate}&toDate=${toDate}`;
    }

    return await this.get<Event[]>(url, this.accessToken);
  }
}
