import React, { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps } from 'react-router-dom'

import { RootState } from 'rootReducer'
import { getNationalGoverningBody, SingleNationalGoverningBodyState } from '../../modules/nationalGoverningBody/nationalGoverningBody'
import Sidebar from './Sidebar'

type IdParams = { id: string }

const NgbAdmin = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id } } } = props
  const dispatch = useDispatch()
  const { ngb, socialAccounts, refereeCount, teamCount } = useSelector((state: RootState): SingleNationalGoverningBodyState => {
    return {
      error: state.nationalGoverningBody.error,
      id: state.nationalGoverningBody.id,
      isLoading: state.nationalGoverningBody.isLoading,
      ngb: state.nationalGoverningBody.ngb,
      refereeCount: state.nationalGoverningBody.refereeCount,
      socialAccounts: state.nationalGoverningBody.socialAccounts,
      teamCount: state.nationalGoverningBody.teamCount,
    }
  })

  useEffect(() => {
    if (id) {
      dispatch(getNationalGoverningBody(parseInt(id, 10)))
    }
  }, [id, dispatch])

  if (!ngb) return null

  return (
    <div className="w-5/6 mx-auto my-8">
      <div className="flex justify-between w-full mb-8">
        <h1 className="w-full text-4xl text-navy-blue font-extrabold">{ngb.name}</h1>
        <button className="rounded bg-white border-2 border-green text-green py-2 px-4 uppercase">Actions</button>
      </div>
      <div className="flex w-full flex-row">
        <Sidebar 
          ngb={ngb} 
          socialAccounts={socialAccounts} 
          refereeCount={refereeCount} 
          teamCount={teamCount} 
          isEditing={false} 
          ngbId={id} 
        />
        <div className="flex flex-col w-3/4 px-8">
          <div>
            <p>NGB Stats</p>
          </div>
          <div>
            <p>Ref table</p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default NgbAdmin
