import { selector } from 'recoil';
import { AccountService } from './AccountService';
import { accessTokenSelector } from '@wdid/shared/src/components/Auth';

export const accountServiceSelector = selector({
  key: 'AccountService',
  get: async ({ get }) => {
    const accessToken = get(accessTokenSelector);

    if (!accessToken) {
      throw new Error('User not authenticated');
    }

    return new AccountService(accessToken);
  },
});
