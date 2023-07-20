import { atom } from 'recoil';

export const currentAccountAtom = atom<Account>({
  key: 'currentAccount',
  default: undefined,
});
