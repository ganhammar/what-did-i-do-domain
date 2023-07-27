import { FetchBase } from '@wdid/shared';

export interface ListParamters {
  accountId: string;
  limit: number;
  fromDate?: string;
  toDate?: string;
  tags?: string[];
}

export interface CreateParameters {
  accountId: string;
  title: string;
  description?: string;
  date?: string;
  tags?: string[];
}

export class EventService extends FetchBase {
  baseUrl = `${process.env.REACT_APP_BASE_API_URL}/event`;
  private readonly accessToken: string;

  constructor(accessToken: string) {
    super();

    this.accessToken = accessToken;
  }

  async list({ accountId, limit, fromDate, toDate, tags }: ListParamters) {
    let url = `${this.baseUrl}?accountId=${accountId}&limit=${limit}`;

    if (tags?.length) {
      tags.forEach((tag) => {
        url += `&tags=${tag}`;
      });
    }

    if (fromDate && toDate) {
      url += `&fromDate=${fromDate}&toDate=${toDate}`;
    }

    return await this.get<Event[]>(url, this.accessToken);
  }

  async create(data: CreateParameters) {
    return await this.post<Event>(this.baseUrl, data, this.accessToken);
  }

  async remove(id: string) {
    return await this.delete(`${this.baseUrl}?id=${id}`, this.accessToken);
  }
}
