import { RefObject, useEffect } from 'react';

export function useClickOutside(refs: RefObject<any>[], callback: () => void) {
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      let isOutside = true;

      refs.forEach((ref) => {
        if (isOutside && ref.current && ref.current.contains(event.target)) {
          isOutside = false;
        }
      });

      if (isOutside) {
        callback();
      }
    }

    document.addEventListener('mousedown', handleClickOutside);

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [refs, callback]);
}
