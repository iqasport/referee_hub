import { isBoolean, kebabCase } from 'lodash'
import React from 'react'

import { Data } from 'MainApp/schemas/getTestSchema';

const EXCLUDED_ATTRIBUTES = ['active', 'updatedAt', 'certificationId']
interface DetailsProps {
  test: Data;
}

const Details = (props: DetailsProps) => {
  const { test } = props
  const dataToRender = test ? test.attributes : [];

  const renderData = (entry: [string, string | boolean]) => {
    if (EXCLUDED_ATTRIBUTES.includes(entry[0])) return null

    const labelText = kebabCase(entry[0]).split('-').join(' ')
    const dataText = isBoolean(entry[1]) ? String(entry[1]) : entry[1]

    return (
      <div key={entry[0]} className="my-4">
        <label className="uppercase text-md font-hairline text-gray-400">{labelText}</label>
        <p>{dataText}</p>
      </div>
    )
  }

  return (
    <div>
      {Object.entries(dataToRender).map(renderData)}
    </div>
  )
}

export default Details
