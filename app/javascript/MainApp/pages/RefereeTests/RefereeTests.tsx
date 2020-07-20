import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps } from 'react-router-dom'

import RefereeTestsTable from 'MainApp/components/RefereeTestsTable'
import { fetchReferee, RefereeState } from 'MainApp/modules/referee/referee'
import { RootState } from 'MainApp/rootReducer'

type IdParams = {
  refereeId: string
}

const RefereeTests = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { refereeId } } } = props

  const dispatch = useDispatch()
  const { referee, id } = useSelector((state: RootState): RefereeState => state.referee, shallowEqual)

  useEffect(() => {
    if (!referee || id !== refereeId) {
      dispatch(fetchReferee(refereeId))
    }
  }, [refereeId])

  return (
    <div className="w-5/6 mx-auto my-8">
      <div className="w-full flex justify-between items-center my-8">
        <h1 className="text-4xl font-extrabold">Certifications</h1>
        <div>
          <button
            className="bg-green text-white rounded py-2 px-4"
          >
            Submit Head Referee Payment
          </button>
        </div>
      </div>
      <RefereeTestsTable refId={refereeId} />
    </div>
  )
}

export default RefereeTests
