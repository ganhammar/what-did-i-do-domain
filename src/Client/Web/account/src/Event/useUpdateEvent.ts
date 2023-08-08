import { useRecoilState } from 'recoil';
import { eventsAtom } from './eventsAtom';
import { ApiResponse } from '@wdid/shared/src/infrastructure/FetchBase';
import { ListResult } from './EventService';

export const useUpdateEvent = () => {
  const [events, setEvents] = useRecoilState(eventsAtom);

  return (id: string, event: Event) => {
    const updatedEvents: ApiResponse<ListResult> = {
      ...events,
      result: {
        ...events.result,
        items: [...(events.result?.items ?? [])],
      },
    };

    const index = updatedEvents.result?.items.findIndex((e) => e.id === id);

    if (index !== undefined && ~index) {
      const newEvent = {
        ...updatedEvents.result!.items[index],
        title: event.title,
        description: event.description,
        tags: event.tags,
      };

      updatedEvents.result?.items.splice(index, 1, newEvent);
    }

    setEvents(updatedEvents);
  };
};
