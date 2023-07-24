import { accessTokenSelector } from '@wdid/shared/src/components/Auth';
import { selector } from 'recoil';
import { currentAccountAtom } from 'src/Account';
import { TagService } from 'src/Tag';

export const tagsSelector = selector({
  key: 'tags',
  get: async ({ get }) => {
    const accessToken = get(accessTokenSelector);
    const account = get(currentAccountAtom);

    if (!accessToken) {
      throw new Error('User not authenticated');
    }

    const tagService = new TagService(accessToken);
    return await tagService.list(account.id);
  },
});
