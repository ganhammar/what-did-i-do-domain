import { useRecoilState, useRecoilValue } from 'recoil';
import { eventsAtom } from './eventsAtom';
import { ApiResponse } from '@wdid/shared/src/infrastructure/FetchBase';
import { ListResult } from './EventService';
import { eventListParamtersAtom } from './eventListParamtersAtom';

export const useAddEvent = () => {
  const parameters = useRecoilValue(eventListParamtersAtom);
  const [events, setEvents] = useRecoilState(eventsAtom);

  return (newEvent: Event) => {
    const updatedEvents: ApiResponse<ListResult> = {
      ...events,
      result: {
        ...events.result,
        items: [],
      },
    };

    if (
      parameters.tag &&
      !newEvent.tags?.some((tag) => tag === parameters.tag)
    ) {
      return;
    }

    let hasAddedEvent = false;
    events.result?.items.forEach((event) => {
      if (!hasAddedEvent && new Date(newEvent.date) > new Date(event.date)) {
        updatedEvents.result!.items.push(newEvent);
        hasAddedEvent = true;
      }

      updatedEvents.result!.items.push(event);
    });

    if (!hasAddedEvent) {
      updatedEvents.result!.items.push(newEvent);
    }

    setEvents(updatedEvents);
  };
};
