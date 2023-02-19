import { useState, useCallback } from 'react';

export default function useAsyncError() {
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
