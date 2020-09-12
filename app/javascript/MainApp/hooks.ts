/* eslint-disable consistent-return */
import { useEffect, useRef } from 'react';

function useInterval(callback: () => void, delay?: number) {
  const savedCallback = useRef(callback);

  // Remember the latest callback.
  useEffect(() => {
    savedCallback.current = callback;
  }, [callback]);

  // Set up the interval.
  useEffect(() => {
    function tick() {
      savedCallback.current();
    }
    if (delay !== null) {
      const id = setInterval(tick, delay);
      return () => clearInterval(id);
    }
    return null
  }, [delay]);
}

export default useInterval
