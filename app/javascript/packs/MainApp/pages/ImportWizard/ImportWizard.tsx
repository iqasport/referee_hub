import React, { useState } from 'react'
import { useHistory } from 'react-router-dom';
import StepDescriptions from './StepDescriptions';

const stepTextMap: { [stepCount: number]: string } = {
  1: 'Upload',
  2: 'Map',
  3: 'Finish',
}

const ImportWizard = () => {
  const [stepCount, setStepCount] = useState(1)
  const [uploadedFile, setUploadedFile] = useState<File>()
  const history = useHistory();
  const isFinalStep = stepCount === 3;
  const buttonText = isFinalStep ? 'Done' : 'Next';
  const isDisabled = stepCount === 1 && !uploadedFile
  
  const goForward = () => setStepCount(stepCount + 1)
  
  const handleHomeClick = () => history.goBack()
  const handleButtonClick = () => {
    if (isFinalStep) {
      handleHomeClick()
    } else if (stepCount === 2) {
      // submit csv
      goForward()
    } else {
      goForward()
    }
  }

  const renderStepContent = (): JSX.Element | null => {
    switch(stepCount) {
      case 1:
        return <div>upload step</div>
      default:
        return null
    }
  }

  return (
    <div className="w-full px-10 py-4 flex flex-col h-screen items-center">
      <div className="justify-start w-full">
        <button className="py-4 px-8" onClick={handleHomeClick}>Home</button>
      </div>
      <h1 className="font-extrabold text-3xl w-full pl-10">Import</h1>
      <StepDescriptions currentStep={stepCount} scopes={['team']} />
      <div className="rounded-lg bg-green w-3/4 flex justify-between py-4 px-12 text-navy-blue mb-4">
        <h3 className="text-xl font-bold">{stepTextMap[stepCount]}</h3>
        <p>{`Step ${stepCount}/3`}</p>
      </div>
      <div className="rounded border border-gray-300 w-3/4">
        {renderStepContent()}
      </div>
      <div className="justify-end w-full flex">
        <button className="green-button-outline" onClick={handleButtonClick} disabled={isDisabled}>{buttonText}</button>
      </div>
    </div>
  )
}

export default ImportWizard
