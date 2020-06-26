import React, { useState } from 'react'

import { IdAttributes } from '../../apis/referee'
import TestResultCard from './TestResultCard'

type TestResultsProps = {
  testResults: IdAttributes[]
}

const TestResultCards = (props: TestResultsProps) => {
  const [expandedId, setExpandedId] = useState<string>()

  const renderTestResult = (result: IdAttributes): JSX.Element => {
    return (
      <TestResultCard
        key={result.id} 
        isExpanded={expandedId === result.id} 
        onExpandClick={setExpandedId} 
        testResult={result}
        isDisabled={expandedId && expandedId !== result.id}
      />
    )
  }

  return (
    <div>
      {props.testResults.map(renderTestResult)}
    </div>
  )
}

export default TestResultCards
