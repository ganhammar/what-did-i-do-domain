interface Request {
  method: 'get' | 'post' | 'put' | 'delete';
  url: string;
  body?: object;
  accessToken?: string;
}

export interface ApiError {
  errorCode: string;
  errorMessage: string;
  attemptedValue?: string | null;
  propertyName?: string;
}

export interface ApiResponse<TResult> {
  errors?: ApiError[];
  success: boolean;
  result?: TResult;
}

export abstract class FetchBase {
  protected async get<T>(url: string, accessToken?: string) {
    return await this.request<T>({ method: 'get', url, accessToken });
  }

  protected async post<T>(url: string, body: object, accessToken?: string) {
    return await this.request<T>({ method: 'post', url, body, accessToken });
  }

  protected async put<T>(url: string, body: object, accessToken?: string) {
    return await this.request<T>({ method: 'put', url, body, accessToken });
  }

  protected async delete<T>(url: string, accessToken?: string) {
    return await this.request<T>({ method: 'delete', url, accessToken });
  }

  private async request<T>({ method, url, body, accessToken }: Request) {
    const headers = new Headers();
    const options: RequestInit = { method, headers };

    if (body) {
      headers.set('Content-Type', 'application/json');
      options.body = JSON.stringify(body);
    }

    if (accessToken) {
      headers.set('Authorization', `Bearer ${accessToken}`);
    }

    const result = await fetch(url, options);

    if (!result.ok) {
      let message = await result.text();
      const response: ApiResponse<T> = {
        success: false,
      };

      try {
        response.errors = JSON.parse(message);
      } catch (_) {
        response.errors = [
          {
            errorCode: 'UnexpectedResponse',
            errorMessage: message,
          },
        ];
      }

      return response;
    }

    let data = null;
    if (result.status === 200) {
      data = await result.json();
    }
    const response: ApiResponse<T> = {
      success: true,
      result: data,
    };
    return response;
  }
}
