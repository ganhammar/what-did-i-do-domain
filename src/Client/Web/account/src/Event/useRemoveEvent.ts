import { useRecoilState } from 'recoil';
import { eventsAtom } from './eventsAtom';
import { ApiResponse } from '@wdid/shared/src/infrastructure/FetchBase';
import { ListResult } from './EventService';

export const useRemoveEvent = () => {
  const [events, setEvents] = useRecoilState(eventsAtom);

  return (id: string) => {
    const updatedEvents: ApiResponse<ListResult> = {
      ...events,
      result: {
        ...events.result,
        items: events.result?.items.filter((event) => event.id !== id) ?? [],
      },
    };

    setEvents(updatedEvents);
  };
};
