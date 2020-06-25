import { faCircleNotch } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import React from 'react'

const Loader = () => {
  return (
    <div className="w-full h-full flex justify-center align-center">
      <FontAwesomeIcon icon={faCircleNotch} spin={true} pulse={true} size="10x" className="text-green" />
    </div>
  )
}

export default Loader
