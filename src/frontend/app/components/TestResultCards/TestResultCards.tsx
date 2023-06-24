import { orderBy } from "lodash";
import React, { useState } from "react";

import TestResultCard from "./TestResultCard";
import { TestAttemptViewModel } from "../../store/serviceApi";

type TestResultsProps = {
  testResults: TestAttemptViewModel[];
};

const TestResultCards = (props: TestResultsProps) => {
  const [expandedId, setExpandedId] = useState<string>();

  const renderTestResult = (result: TestAttemptViewModel): JSX.Element => {
    return (
      <TestResultCard
        key={result.attemptId}
        isExpanded={expandedId === result.attemptId}
        onExpandClick={setExpandedId}
        testResult={result}
        isDisabled={expandedId && expandedId !== result.attemptId}
      />
    );
  };

  const sortedResults = orderBy(props.testResults, (testResult) => testResult.startedAt, [
    "desc",
  ]);

  return <div>{sortedResults.map(renderTestResult)}</div>;
};

export default TestResultCards;
