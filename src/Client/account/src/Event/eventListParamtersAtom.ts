import { atom } from 'recoil';

interface EventListParamters {
  limit: number;
  fromDate?: string;
  toDate?: string;
}

export const eventListParamtersAtom = atom<EventListParamters>({
  key: 'eventListParamtersAtom',
  default: {
    limit: 30,
  },
});
