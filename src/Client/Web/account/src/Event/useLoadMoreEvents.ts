import { useRecoilState, useRecoilValue } from 'recoil';
import { eventsAtom } from './eventsAtom';
import { eventServiceSelector } from './eventServiceSelector';
import { eventListParamtersAtom } from './eventListParamtersAtom';
import { currentAccountAtom } from 'src/Account';

export const useLoadMoreEvents = () => {
  const eventService = useRecoilValue(eventServiceSelector);
  const account = useRecoilValue(currentAccountAtom);
  const parameters = useRecoilValue(eventListParamtersAtom);
  const [events, setEvents] = useRecoilState(eventsAtom);

  return async () => {
    const moreEvents = await eventService.list({
      accountId: account.id,
      paginationToken: events.result?.paginationToken,
      ...parameters,
    });

    setEvents({
      success: moreEvents.success,
      errors: moreEvents.errors,
      result: {
        paginationToken: moreEvents.result?.paginationToken,
        items: [
          ...(events.result?.items ?? []),
          ...(moreEvents.result?.items ?? []),
        ],
      },
    });
  };
};
