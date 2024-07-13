import React, { useState } from "react";
import { DateTime } from "luxon";

import useInterval from "../../hooks";

export const formatTime = (time: number): string | number => {
  time = Math.floor(time);
  return time < 10 ? `0${time}` : time;
};

export const getDisplayColor = (timeLimit: number, interval: { minutes: number; seconds: number }): string => {
  const goodLimit = Math.floor(timeLimit * 0.25);

  if (interval.minutes > goodLimit) return "gray-500";
  // last 10 seconds are red
  if (interval.minutes < 0 || (interval.minutes === 0 && interval.seconds < 11)) return "red-500";
  // last quarter of time is yellow
  if (interval.minutes <= goodLimit) return "yellow"; // TODO: figure out why yellow isn't scaled

  return "gray-500";
};

interface CounterProps {
  timeLimit: number;
  onTimeLimitMet: (minutes: number, seconds: number) => void;
  setCurrentTime: (currentTime: { minutes: number; seconds: number }) => void;
}

const Counter = (props: CounterProps) => {
  const { timeLimit, onTimeLimitMet, setCurrentTime } = props;
  const [testStart] = useState(DateTime.now())
  const [interval, setInterval] = useState({ minutes: timeLimit, seconds: 0 })
  const displayMinute = formatTime(interval.minutes);
  const displaySecond = formatTime(interval.seconds);

  const handleTick = () => {
    const testEnd = testStart.plus({minutes: timeLimit});
    const currentInterval = testEnd.diffNow(["minutes", "seconds"]).toObject();

    // if testEnd is less than now() in milliseconds
    if (testEnd.diffNow("milliseconds").toObject().milliseconds < 0) {
      const { minutes, seconds } = DateTime.now().diff(testStart, ["minutes", "seconds"]).toObject();
      onTimeLimitMet(minutes, seconds);
    }

    setInterval(currentInterval as any);

    setCurrentTime(DateTime.now().diff(testStart, ["minutes", "seconds"]).toObject() as any);
  };

  useInterval(handleTick, 1000);

  return (
    <span data-testid="counter" className={`text-center text-lg font-bold text-${getDisplayColor(timeLimit, interval)}`}>
      {`${displayMinute}:${displaySecond}`}
    </span>
  );
};

export default Counter;
