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
  });

  afterEach(() => {
    jest.clearAllTimers();
  });

  test("it renders a counter", () => {
    render(<Counter {...defaultProps} />);

    // Counter shows remaining time, starting at timeLimit (10 minutes)
    const counterElement = screen.getByTestId("counter");
    expect(counterElement).toBeInTheDocument();
    // Time format should be MM:SS
    expect(counterElement.textContent).toMatch(/^\d{2}:\d{2}$/);
  });

  test("it handles a second tick", () => {
    render(<Counter {...defaultProps} />);

    const initialText = screen.getByTestId("counter").textContent;

    act(() => {
      jest.advanceTimersByTime(1000);
    });

    const afterTickText = screen.getByTestId("counter").textContent;
    // After one second, the counter should update (time should decrease)
    expect(afterTickText).not.toBe(initialText);
    expect(screen.getByTestId("counter")).toBeInTheDocument();
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
