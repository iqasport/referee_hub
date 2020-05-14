import React from 'react'
import { RouteComponentProps } from 'react-router-dom'

type IdParams = { id: string }

const NgbAdmin = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id } } } = props

  return (
    <div>
      {`Welcome to the profile of NGB ${id}`}
    </div>
  )
}

export default NgbAdmin
