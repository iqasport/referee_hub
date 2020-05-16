import { capitalize } from 'lodash'
import React, { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { RootState } from 'rootReducer'

import { getReferees, Referee, RefereesState } from '../../modules/referee/referees'
import { AssociationType } from '../../schemas/getRefereesSchema'

const NewRefereeTable = () => {
  const dispatch = useDispatch()
  const { referees, meta, isLoading } = useSelector((state: RootState): RefereesState => {
    return {
      error: state.referees.error,
      isLoading: state.referees.isLoading,
      meta: state.referees.meta,
      referees: state.referees.referees,
    }
  })

  useEffect(() => {
    dispatch(getReferees())
  }, [])

  const renderRow = (referee: Referee): JSX.Element => {
    const highestCert = referee?.certifications.find((cert) => {
      if (cert.level === 'head') {
        return true
      } else if (cert.level === 'assistant') {
        return true
      } else if (cert.level === 'snitch') {
        return true
      }

      return false
    })
    const teamNames = referee?.teams.map((team) => team.name).join(', ')
    const secondary = referee?.locations.filter((location) => location.associationType === AssociationType.Secondary)
    const secondaryName = secondary.length && referee?.ngbs.find(ngb => ngb.id === secondary[0].nationalGoverningBodyId.toString())?.name
  
    return (
      <tr key={referee?.id} className="border border-gray-300">
        <td className="w-1/4 py-4 px-8">{`${referee?.referee.firstName} ${referee?.referee.lastName}`}</td>
        <td className="w-1/4 py-4 px-8">{`${capitalize(highestCert?.level)}`}</td>
        <td className="w-1/4 py-4 px-8">{teamNames}</td>
        <td className="w-1/4 py-4 px-8">{secondaryName || 'N/A'}</td>
      </tr>
    )
  }

  const renderBody = () => {
    return (
      <tbody>
        {referees.map(renderRow)}
      </tbody>
    )
  }

  const renderEmpty = () => {
    return (
      <tbody>
        <tr>
          <td>
            <h2>{isLoading ? 'Loading...' : 'No referees found'}</h2>
          </td>
        </tr>
      </tbody>
    )
  }

  return (
    <>
      {referees.length && (
        <table className="rounded-table-header">
          <tbody>
            <tr className="text-left">
              <td className="w-1/4 py-4 px-8">name</td>
              <td className="w-1/4 py-4 px-8">highest certification</td>
              <td className="w-1/4 py-4 px-8">associated teams</td>
              <td className="w-1/4 py-4 px-8">secondary NGB</td>
            </tr>
          </tbody>
        </table>
      )}
      <table className="rounded-table">
        {referees.length ? renderBody() : renderEmpty()}
      </table>
    </>
  )
}

export default NewRefereeTable
