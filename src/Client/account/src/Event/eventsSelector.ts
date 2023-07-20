import { accessTokenSelector } from '@wdid/shared/src/components/Auth';
import { selector } from 'recoil';
import { EventService } from './EventService';
import { currentAccountAtom } from 'src/Account';
import { eventListParamtersAtom } from './eventListParamtersAtom';

export const eventsSelector = selector({
  key: 'events',
  get: async ({ get }) => {
    const accessToken = get(accessTokenSelector);

    if (!accessToken) {
      throw new Error('User not authenticated');
    }

    const account = get(currentAccountAtom);
    const parameters = get(eventListParamtersAtom);

    const eventService = new EventService(accessToken);
    return await eventService.list({
      accountId: account.id,
      ...parameters,
    });
  },
});
