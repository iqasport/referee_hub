import React from 'react'

interface ProgressBarProps {
  currentIndex: number;
  total: number;
}

const ProgressBar = (props: ProgressBarProps) => {
  const { currentIndex, total } = props

  const percentage = (currentIndex / total) * 100

  return (
    <div className="shadow w-full bg-grey-light rounded-lg">
      <div
        className="bg-green text-xs leading-none py-1 text-center text-white"
        style={{ width: `${percentage}%`}}
      >
        {`${currentIndex}/${total}`}
      </div>
    </div>
  )
}

export default ProgressBar
