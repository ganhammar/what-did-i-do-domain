import { selector } from 'recoil';
import { EventService } from './EventService';
import { accessTokenSelector } from '@wdid/shared/src/components/Auth';

export const eventServiceSelector = selector({
  key: 'EventService',
  get: async ({ get }) => {
    const accessToken = get(accessTokenSelector);

    if (!accessToken) {
      throw new Error('User not authenticated');
    }

    return new EventService(accessToken);
  },
});
