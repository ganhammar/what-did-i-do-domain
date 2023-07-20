import { accessTokenSelector } from '@wdid/shared/src/components/Auth';
import { selector } from 'recoil';
import { AccountService } from 'src/Account';

export const accountsSelector = selector({
  key: 'accounts',
  get: async ({ get }) => {
    const accessToken = get(accessTokenSelector);

    if (!accessToken) {
      throw new Error('User not authenticated');
    }

    const accountService = new AccountService(accessToken);
    return await accountService.list();
  },
});
