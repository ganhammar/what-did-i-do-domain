import { accessTokenSelector } from '@wdid/shared/src/components/Auth';
import { atom, selector } from 'recoil';
import { currentAccountAtom } from 'src/Account';
import { TagService } from 'src/Tag';

export const tagsAtom = atom({
  key: 'tagsAtom',
  default: selector({
    key: 'tagsAtom/Default',
    get: async ({ get }) => {
      const accessToken = get(accessTokenSelector);
      const account = get(currentAccountAtom);

      if (!accessToken) {
        throw new Error('User not authenticated');
      }

      const tagService = new TagService(accessToken);

      if (account) {
        return await tagService.list(account.id);
      }

      return { result: [], success: true };
    },
  }),
});
