import { useLayoutEffect, useState } from 'react';

export function useScroll() {
  const [scroll, setScroll] = useState([0, 0]);

  useLayoutEffect(() => {
    function updateScroll() {
      setScroll([window.scrollX, window.scrollY]);
    }

    window.addEventListener('scroll', updateScroll);

    updateScroll();

    return () => window.removeEventListener('scroll', updateScroll);
  }, []);

  return scroll;
}
