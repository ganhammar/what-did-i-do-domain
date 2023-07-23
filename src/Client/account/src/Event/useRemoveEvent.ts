import { useRecoilState } from 'recoil';
import { eventsAtom } from './eventsAtom';
import { ApiResponse } from '@wdid/shared/src/infrastructure/FetchBase';

export const useRemoveEvent = () => {
  const [events, setEvents] = useRecoilState(eventsAtom);

  return (id: string) => {
    const updatedEvents: ApiResponse<Event[]> = {
      ...events,
      result: events.result?.filter((event) => event.id !== id),
    };

    setEvents(updatedEvents);
  };
};
