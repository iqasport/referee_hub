import classnames from 'classnames'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'

import Toggle from 'MainApp/components/Toggle'
import { getTest, updateTest } from 'MainApp/modules/test/test'
import { RootState } from 'MainApp/rootReducer'
import { IdParams } from '../RefereeProfile/types'

enum SelectedTab {
  Details = 'details',
  Questions = 'questions'
}

const NewTest = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id } } } = props

  const [selectedTab, setSelectedTab] = useState<SelectedTab>(SelectedTab.Details)
  const dispatch = useDispatch()
  const history = useHistory()
  const { test } = useSelector((state: RootState) => state.test, shallowEqual)
  const isSelected = (tab: SelectedTab) => selectedTab === tab

  useEffect(() => {
    dispatch(getTest(id))
  }, [id])

  const handleBackClick = () => history.goBack()

  const handleToggle = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newTest = { active: event.currentTarget.checked }
    dispatch(updateTest(id, newTest))
  }

  const handleImportClick = () => history.push(`/import/test_${id}`)
  const handleTabClick = (newTab: SelectedTab) => () => {
    if (newTab === selectedTab) return null

    setSelectedTab(newTab)
  }

  const renderContent = () => {
    switch(selectedTab) {
      case SelectedTab.Details:
        return <div>Details content</div>
      case SelectedTab.Questions:
        return <div>Questions content</div>
    }
  }

  return (
    <div className="w-5/6 mx-auto my-8">
      <button className="block" onClick={handleBackClick}>back</button>
      <div className="w-full flex justify-end items-center">
        <div className="flex w-1/2 items-center">
          <h1 className="text-3xl font-extrabold text-right mr-4">{test?.attributes.name}</h1>
          <Toggle name="active" onChange={handleToggle} checked={test?.attributes.active} />
        </div>
        <button className="green-button-outline" onClick={handleImportClick}>Import Questions</button>
      </div>
      <div className="w-5/6 h-screen my-8 mx-auto">
        <div className="tab-row">
          <button
            className={classnames({ 'tab-selected': isSelected(SelectedTab.Details) })}
            onClick={handleTabClick(SelectedTab.Details)}
          >
            Details
          </button>
          <button
            className={classnames({ 'tab-selected': isSelected(SelectedTab.Questions) })}
            onClick={handleTabClick(SelectedTab.Questions)}
          >
            Question Manager
          </button>
        </div>
        <div className="border border-t-0 p-4">
          {renderContent()}
        </div>
      </div>
    </div>
  )
}

export default NewTest
