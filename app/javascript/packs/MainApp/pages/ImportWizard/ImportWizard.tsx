import { faEnvelopeOpenText, faRoute, faUpload, IconDefinition } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import React, { useState } from 'react'
import { useHistory } from 'react-router-dom';
import StepDescriptions from './StepDescriptions';
import UploadStep from './UploadStep';

type StepConfig = {
  title: string;
  icon: IconDefinition
}

const stepTextMap: { [stepCount: number]: StepConfig } = {
  1: {
    icon: faUpload,
    title: 'Upload',
  },
  2: {
    icon: faRoute,
    title: 'Map',
  },
  3: {
    icon: faEnvelopeOpenText,
    title: 'Finish'
  },
}

const ImportWizard = () => {
  const [stepCount, setStepCount] = useState(1)
  const [uploadedFile, setUploadedFile] = useState<File>()
  const history = useHistory();
  const isFinalStep = stepCount === 3;
  const buttonText = isFinalStep ? 'Done' : 'Next';
  const isDisabled = stepCount === 1 && !uploadedFile
  const currentStepConfig = stepTextMap[stepCount]

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
  const handleFileUpload = (selectedFile: File) => setUploadedFile(selectedFile)

  const renderStepContent = (): JSX.Element | null => {
    switch(stepCount) {
      case 1:
        return <UploadStep onFileUpload={handleFileUpload} uploadedFile={uploadedFile} />
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
        <h3 className="text-xl font-bold flex items-center">
          {currentStepConfig.title}
          <FontAwesomeIcon icon={currentStepConfig.icon} className="ml-4" />
        </h3>
        <p className="uppercase">{`Step ${stepCount}/3`}</p>
      </div>
      <div className="rounded border border-gray-400 w-3/4">
        {renderStepContent()}
      </div>
      <div className="justify-end w-full flex mt-8">
        <button className="green-button-outline" onClick={handleButtonClick} disabled={isDisabled}>{buttonText}</button>
      </div>
    </div>
  )
}

export default ImportWizard
