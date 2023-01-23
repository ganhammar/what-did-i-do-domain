import { FetchBase } from '../../infrastructure/FetchBase';

export interface DomainRequirementsResult {
  requiredOidcProviderId?: string,
}

export class ApplicationService extends FetchBase {
  baseUrl = `${process.env.REACT_APP_API_URL}/application`;

  async domainRequirements(domain: string) {
    return await this.get<DomainRequirementsResult>(
      `${this.baseUrl}/domainrequirements?domain=${domain}`
    );
  }
}
