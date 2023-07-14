import accessTokenSelector from '@wdid/shared/src/components/Auth/accessTokenSelector';
import { selector } from 'recoil';
import { AccountService } from 'src/Account';

const accountsSelector = selector({
  key: 'accounts',
  get: async ({ get }) => {
    const accessToken = get(accessTokenSelector);

    if (!accessToken) {
      throw new Error('User not authenticated');
    }

    const accountService = new AccountService(accessToken);
    return await accountService.accounts();
  },
});

export default accountsSelector;
