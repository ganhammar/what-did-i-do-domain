import { useRecoilState } from 'recoil';
import { eventsAtom } from './eventsAtom';
import { ApiResponse } from '@wdid/shared/src/infrastructure/FetchBase';

export const useAddEvent = () => {
  const [events, setEvents] = useRecoilState(eventsAtom);

  return (newEvent: Event) => {
    const updatedEvents: ApiResponse<Event[]> = {
      ...events,
      result: [],
    };

    let hasAddedEvent = false;
    events.result?.forEach((event) => {
      if (!hasAddedEvent && new Date(newEvent.date) > new Date(event.date)) {
        updatedEvents.result!.push(newEvent);
        hasAddedEvent = true;
      }

      updatedEvents.result!.push(event);
    });

    if (!hasAddedEvent) {
      updatedEvents.result!.push(newEvent);
    }

    setEvents(updatedEvents);
  };
};
