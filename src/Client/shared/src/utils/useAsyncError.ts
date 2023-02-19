import { useState, useCallback } from 'react';

export function useAsyncError() {
  const [, setError] = useState();

  return useCallback(
    (error: any) => {
      setError(() => {
        throw error;
      });
    },
    [setError]
  );
};
