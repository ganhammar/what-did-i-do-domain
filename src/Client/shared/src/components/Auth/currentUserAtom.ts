import { atom, selector } from 'recoil';
import { userManager } from './userManager';

const currentUserDefault = selector({
  key: 'currentUserDefault',
  get: async () => {
    const result =  await userManager.getUser();

    if (result && result.expired === false) {
      return result;
    } else {
      return undefined;
    }
  },
  set: ({ set }, value) => {
    set(currentUserAtom, value);
  },
});

export const currentUserAtom = atom({
  key: 'currentUser',
  default: currentUserDefault,
});
