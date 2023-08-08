import { useEffect } from 'react';

export const useKeyPress = (
  keys: string[],
  callback: (key: string) => void
) => {
  useEffect(() => {
    const handleEsc = (event: KeyboardEvent) => {
      if (keys.some((key) => event.key === key)) {
        callback(event.key);
      }
    };
    window.addEventListener('keydown', handleEsc);

    return () => {
      window.removeEventListener('keydown', handleEsc);
    };
  }, [keys, callback]);
};
