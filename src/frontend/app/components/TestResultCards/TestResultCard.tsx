import {
  faCaretDown,
  faCaretUp,
  faCheckCircle,
  faTimesCircle,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import { capitalize } from "lodash";
import React from "react";

import { toDateTime } from "../../utils/dateUtils";

import ResultChart from "./ResultChart";
import { TestAttemptViewModelRead } from "../../store/serviceApi";

type CardProps = {
  testResult: TestAttemptViewModelRead;
  isExpanded: boolean;
  onExpandClick: (id: string) => void;
  isDisabled: boolean;
};

const TestResultCard = (props: CardProps) => {
  const {
    testResult: { level: testLevel, startedAt: createdAt, passPercentage: minimumPassPercentage, score: percentage, attemptId: id, duration, passed },
    onExpandClick,
    isExpanded,
    isDisabled,
  } = props;
  const formattedDate = toDateTime(createdAt).toLocaleString();
  const expandText = isExpanded ? "less" : "more";
  const expandIcon = isExpanded ? faCaretUp : faCaretDown;
  const resultText = passed ? "Passed" : "Failed";
  const resultIcon = passed ? faCheckCircle : faTimesCircle;

  const handleClick = () => {
    const idToChange = isExpanded ? "" : id;

    onExpandClick(idToChange);
  };

  return (
    <div
      className={classnames("w-full p-4 bg-white my-4", {
        "opacity-50 pointer-events-none": isDisabled,
        "z-1 shadow": isExpanded,
      })}
    >
      <div className="flex h-20 items-center">
        <table className="table-fixed w-1/2">
          <thead>
            <tr>
              <th className="w-1/2 text-left">test type</th>
              <th className="w-1/2 text-left">completion date</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>{capitalize(testLevel)}</td>
              <td>{formattedDate}</td>
            </tr>
          </tbody>
        </table>
        <div className="flex items-center h-full justify-between w-1/2">
          <ResultChart minimum={minimumPassPercentage} actual={percentage} />
          <div className="clickable-icon-label" onClick={handleClick}>
            {expandText}
            <FontAwesomeIcon icon={expandIcon} />
          </div>
        </div>
      </div>
      {isExpanded && (
        <table className="table-fixed w-3/4 mt-6">
          <thead>
            <tr>
              <th className="w-1/3 text-left">duration</th>
              <th className="w-1/3 text-left">result</th>
              <th className="w-1/3 text-left">minimum pass %</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>{duration || "N/A"}</td>
              <td>
                {resultText}{" "}
                <FontAwesomeIcon
                  icon={resultIcon}
                  className={classnames("text-red-500", { "text-green-darker": passed })}
                />
              </td>
              <td>{`${minimumPassPercentage}%`}</td>
            </tr>
          </tbody>
        </table>
      )}
    </div>
  );
};

export default TestResultCard;
