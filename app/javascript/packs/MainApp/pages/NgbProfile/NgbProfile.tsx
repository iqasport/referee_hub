import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps } from 'react-router-dom'

import { RootState } from 'rootReducer'
import NewRefereeTable from '../../components/NewRefereeTable'
import StatsViewer from '../../components/StatsViewer'
import { getNationalGoverningBody, SingleNationalGoverningBodyState } from '../../modules/nationalGoverningBody/nationalGoverningBody'
import Sidebar from './Sidebar'

type IdParams = { id: string }

const NgbAdmin = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id } } } = props
  const dispatch = useDispatch()
  const { ngb, socialAccounts, refereeCount, teamCount, stats } = useSelector((state: RootState): SingleNationalGoverningBodyState => {
    return {
      error: state.nationalGoverningBody.error,
      id: state.nationalGoverningBody.id,
      isLoading: state.nationalGoverningBody.isLoading,
      ngb: state.nationalGoverningBody.ngb,
      refereeCount: state.nationalGoverningBody.refereeCount,
      socialAccounts: state.nationalGoverningBody.socialAccounts,
      stats: state.nationalGoverningBody.stats,
      teamCount: state.nationalGoverningBody.teamCount,
    }
  }, shallowEqual)

  useEffect(() => {
    if (id) {
      dispatch(getNationalGoverningBody(parseInt(id, 10)))
    }
  }, [id, dispatch])

  if (!ngb) return null
  const currentStat = stats[0]

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
        <div className="flex flex-col w-4/5 pl-8">
          <StatsViewer stats={stats} />
          <div className="w-full">
            <NewRefereeTable ngbId={parseInt(id, 10)} />
          </div>
        </div>
      </div>
    </div>
  )
}

export default NgbAdmin
