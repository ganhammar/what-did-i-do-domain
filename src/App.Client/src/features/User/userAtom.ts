import { atom } from 'recoil';
import { User as OidcUser } from 'oidc-client-ts';

export const userAtom = atom<OidcUser>({
  key: 'user',
  default: undefined,
});
