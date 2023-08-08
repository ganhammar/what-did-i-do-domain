import { selector } from 'recoil';
import { currentUserAtom } from './currentUserAtom';

export const accessTokenSelector = selector({
  key: 'AccessToken',
  get: async ({ get }) => {
    const user = get(currentUserAtom);

    return user?.access_token;
  },
});
