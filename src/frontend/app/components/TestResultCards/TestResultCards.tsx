import { orderBy } from "lodash";
import React, { useState } from "react";

import { IdAttributes } from "../../apis/referee";
import TestResultCard from "./TestResultCard";

type TestResultsProps = {
  testResults: IdAttributes[];
};

const TestResultCards = (props: TestResultsProps) => {
  const [expandedId, setExpandedId] = useState<string>();

  const renderTestResult = (result: IdAttributes): JSX.Element => {
    return (
      <TestResultCard
        key={result.id}
        isExpanded={expandedId === result.id}
        onExpandClick={setExpandedId}
        testResult={result}
        isDisabled={expandedId && expandedId !== result.id}
      />
    );
  };

  const sortedResults = orderBy(props.testResults, (testResult) => parseInt(testResult.id, 10), [
    "desc",
  ]);

  return <div>{sortedResults.map(renderTestResult)}</div>;
};

export default TestResultCards;
