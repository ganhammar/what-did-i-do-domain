import { selector } from 'recoil';
import { currentUserAtom } from './currentUserAtom';

const accessTokenSelector = selector({
  key: 'AccessToken',
  get: async ({ get }) => {
    const user = get(currentUserAtom);

    return user?.access_token;
  },
});

export default accessTokenSelector;
