import React from 'react'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCircleNotch } from '@fortawesome/free-solid-svg-icons'

const Loader = () => {
  return (
    <div className="w-full h-full flex justify-center align-center">
      <FontAwesomeIcon icon={faCircleNotch} spin={true} pulse={true} size="10x" className="text-green" />
    </div>
  )
}

export default Loader
