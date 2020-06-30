import { capitalize } from 'lodash'
import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { useHistory } from 'react-router-dom'

import { GetRefereesFilter } from '../../apis/referee'
import { getReferees, Referee, RefereesState, updateFilters } from '../../modules/referee/referees'
import { RootState } from '../../rootReducer'
import { AssociationType } from '../../schemas/getRefereesSchema'

type NewRefereeTableProps = {
  ngbId: number;
}

const NewRefereeTable = (props: NewRefereeTableProps) => {
  const history = useHistory()
  const dispatch = useDispatch()
  const { referees, isLoading } = useSelector((state: RootState) => state.referees, shallowEqual)

  useEffect(() => {
    const filter: GetRefereesFilter = { nationalGoverningBodies: [props.ngbId] }
    dispatch(updateFilters(filter))
    dispatch(getReferees(filter))
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
    const highestCertText = highestCert ? capitalize(highestCert?.level) : 'Uncertified';
    const fullName = `${referee?.referee.firstName} ${referee?.referee.lastName}`
    const handleClick = () => history.push(`/referees/${referee.id}`)

    return (
      <tr key={referee?.id} className="border border-gray-300 hover:bg-gray-300" onClick={handleClick}>
        <td className="w-1/4 py-4 px-8">{fullName}</td>
        <td className="w-1/4 py-4 px-8">{highestCertText}</td>
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
      <div className="table-container">
        <table className="rounded-table">
          {referees.length ? renderBody() : renderEmpty()}
        </table>
      </div>
    </>
  )
}

export default NewRefereeTable
