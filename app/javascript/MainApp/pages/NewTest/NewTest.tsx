import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'

import Toggle from 'MainApp/components/Toggle'
import { getTest, updateTest } from 'MainApp/modules/test/test'
import { RootState } from 'MainApp/rootReducer'
import { IdParams } from '../RefereeProfile/types'

const NewTest = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id } } } = props

  const dispatch = useDispatch()
  const history = useHistory()
  const { test } = useSelector((state: RootState) => state.test, shallowEqual)

  useEffect(() => {
    dispatch(getTest(id))
  }, [id])

  const handleBackClick = () => history.goBack()

  const handleToggle = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newTest = { active: event.currentTarget.checked }
    dispatch(updateTest(id, newTest))
  }

  const handleImportClick = () => history.push(`/import/test_${id}`)

  return (
    <div className="w-5/6 mx-auto my-8">
      <button onClick={handleBackClick}>back</button>
      <div className="w-full flex justify-end items-center">
        <div className="flex w-1/2 items-center">
          <h1 className="text-3xl font-extrabold text-right mr-4">{test?.attributes.name}</h1>
          <Toggle name="active" onChange={handleToggle} checked={test?.attributes.active} />
        </div>
        <button className="green-button-outline" onClick={handleImportClick}>Import Questions</button>
      </div>
    </div>
  )
}

export default NewTest
