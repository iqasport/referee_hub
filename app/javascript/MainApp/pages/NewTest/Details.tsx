import { kebabCase } from 'lodash'
import React from 'react'

import { Data } from 'MainApp/schemas/getTestSchema';

const EXCLUDED_ATTRIBUTES = ['active', 'updatedAt', 'certificationId']
interface DetailsProps {
  test: Data;
}

const Details = (props: DetailsProps) => {
  const { test } = props
  if (!test) return null

  const renderData = (entry: [string, string]) => {
    if (EXCLUDED_ATTRIBUTES.includes(entry[0])) return null

    const labelText = kebabCase(entry[0]).split('-').join(' ')

    return (
      <div key={entry[0]} className="my-4">
        <label className="uppercase text-md font-hairline text-gray-400">{labelText}</label>
        <p>{entry[1]}</p>
      </div>
    )
  }

  return (
    <div>
      {Object.entries(test.attributes).map(renderData)}
    </div>
  )
}

export default Details
