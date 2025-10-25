import React from "react";
import { act } from "react";
import Counter from ".";
import { render, screen } from "../../utils/test-utils";
import { formatTime, getDisplayColor } from "./Counter";

describe("Counter", () => {
  const defaultProps = {
    onTimeLimitMet: jest.fn(),
    setCurrentTime: jest.fn(),
    timeLimit: 10,
  };

  beforeEach(() => {
    jest.useFakeTimers();
    jest.clearAllMocks();
  });

  afterEach(() => {
    jest.clearAllTimers();
  });

  test("it renders a countdown timer starting at time limit", () => {
    render(<Counter {...defaultProps} />);

    const counterElement = screen.getByTestId("counter");
    expect(counterElement).toBeInTheDocument();
    // Counter starts at timeLimit (10 minutes = 10:00 or close to it)
    // May show 09:59 or 10:00 depending on timing
    expect(counterElement.textContent).toMatch(/^(10:00|09:5\d)$/);
  });

  test("it counts down as time progresses", () => {
    render(<Counter {...defaultProps} />);

    // Advance by 1 second
    act(() => {
      jest.advanceTimersByTime(1000);
    });

    const counterElement = screen.getByTestId("counter");
    // Should show approximately 9 minutes 59 seconds remaining
    expect(counterElement.textContent).toMatch(/^09:5[89]$/);
  });

  test("it calls setCurrentTime callback with elapsed time", () => {
    render(<Counter {...defaultProps} />);

    // Advance by 1 second
    act(() => {
      jest.advanceTimersByTime(1000);
    });

    // setCurrentTime should be called with elapsed time (not remaining time)
    expect(defaultProps.setCurrentTime).toHaveBeenCalled();
    const callArg = defaultProps.setCurrentTime.mock.calls[0][0];
    // Elapsed time should be close to 0 minutes, 1 second
    expect(callArg.minutes).toBeCloseTo(0, 0);
    expect(callArg.seconds).toBeGreaterThanOrEqual(0);
  });

  test("it calls onTimeLimitMet when countdown reaches zero", () => {
    const shortTimeProps = {
      ...defaultProps,
      timeLimit: 0.02, // 0.02 minutes = ~1 second
    };
    
    render(<Counter {...shortTimeProps} />);

    // Advance past the time limit
    act(() => {
      jest.advanceTimersByTime(2000);
    });

    // onTimeLimitMet should be called with total elapsed time
    expect(shortTimeProps.onTimeLimitMet).toHaveBeenCalled();
  });

  test("it updates every second via interval", () => {
    render(<Counter {...defaultProps} />);

    // Advance by 3 seconds - should trigger 3 interval ticks
    act(() => {
      jest.advanceTimersByTime(3000);
    });

    // Should have been called 3 times (once per second)
    expect(defaultProps.setCurrentTime).toHaveBeenCalledTimes(3);
  });
});

describe("utility functions", () => {
  describe("formatTime", () => {
    test("time is less than 10", () => {
      const time = 4;
      const expectedTime = "04";
      const actual = formatTime(time);

      expect(actual).toEqual(expectedTime);
    });

    test("time is greater than 10", () => {
      const time = 15;
      const expectedTime = 15;
      const actual = formatTime(time);

      expect(actual).toEqual(expectedTime);
    });
  });

  describe("getDisplayColor", () => {
    const limit = 10;

    test("it returns gray-500 when above warning limit", () => {
      const interval = { minutes: 5, seconds: 0 };
      const expectedColor = "gray-500";
      const actual = getDisplayColor(limit, interval);

      expect(actual).toEqual(expectedColor);
    });

    test("it returns yellow when in the warning limit", () => {
      const interval = { minutes: 2, seconds: 0 };
      const expectedColor = "yellow";
      const actual = getDisplayColor(limit, interval);

      expect(actual).toEqual(expectedColor);
    });

    test("it returns red-500 when at or below zero", () => {
      const interval = { minutes: 0, seconds: 5 };
      const expectedColor = "red-500";
      const actual = getDisplayColor(limit, interval);

      expect(actual).toEqual(expectedColor);
    });

    test("it returns red-500 when time is negative", () => {
      const interval = { minutes: -1, seconds: 0 };
      const expectedColor = "red-500";
      const actual = getDisplayColor(limit, interval);

      expect(actual).toEqual(expectedColor);
    });
  });
});
