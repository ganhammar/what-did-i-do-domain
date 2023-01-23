import { useEffect } from 'react';
import debounce from '../../utils/debounce';

export function useDebounce<Arguments extends any[], Response = void>(
  callback: (args?: Arguments) => Response,
  delay: number
) {
  const [debouncedFunction, teardown] = debounce(callback, delay);

  useEffect(() => () => teardown());

  return debouncedFunction;
}
