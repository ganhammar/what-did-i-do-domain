import { atom } from 'recoil';
import { userManager } from './userManager';

const currentUserDefault = async () => {
  const result = await userManager.getUser();

  if (result && result.expired === false) {
    return result;
  } else {
    return undefined;
  }
};

export const currentUserAtom = atom({
  key: 'currentUser',
  default: currentUserDefault(),
});
