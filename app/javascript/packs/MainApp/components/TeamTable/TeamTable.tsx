import React, { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'

import { RootState } from 'rootReducer'
import { Datum } from 'schemas/getTeamsSchema'
import { GetTeamsFilter } from '../../apis/team'
import { getTeams, TeamsState, updateFilters } from '../../modules/team/team'

interface TeamTableProps {
  ngbId: number
}

const TeamTable = (props: TeamTableProps) => {
  const dispatch = useDispatch()
  const { teams, meta, isLoading } = useSelector((state: RootState): TeamsState => {
    return {
      error: state.teams.error,
      isLoading: state.teams.isLoading,
      meta: state.teams.meta,
      teams: state.teams.teams,
    }
  })

  useEffect(() => {
    const filter: GetTeamsFilter = { nationalGoverningBodies: [props.ngbId] }
    dispatch(updateFilters(filter))
    dispatch(getTeams(filter))
  }, [])

  const renderRow = (team: Datum) => {
    const teamCity = `${team.attributes.city}, ${team.attributes.state}`
    return (
      <tr key={team?.id} className="border border-gray-300 hover:bg-gray-600">
        <td className="w-1/4 py-4 px-8">{team.attributes.name}</td>
        <td className="w-1/4 py-4 px-8">{teamCity}</td>
        <td className="w-1/4 py-4 px-8">{team.attributes.groupAffiliation}</td>
        <td className="w-1/4 py-4 px-8">{team.attributes.status}</td>
      </tr>
    )
  }

  const renderBody = () => {
    return (
      <tbody>
        {teams.map(renderRow)}
      </tbody>
    )
  }

  const renderEmpty = () => {
    return (
      <tbody>
        <tr>
          <td>
            <h2>{isLoading ? 'Loading...' : 'No teams found'}</h2>
          </td>
        </tr>
      </tbody>
    )
  }

  return (
    <>
      {teams.length && (
        <table className="rounded-table-header">
          <tbody>
            <tr className="text-left">
              <td className="w-1/4 py-4 px-8">name</td>
              <td className="w-1/4 py-4 px-8">city</td>
              <td className="w-1/4 py-4 px-8">type</td>
              <td className="w-1/4 py-4 px-8">status</td>
            </tr>
          </tbody>
        </table>
      )}
      <div className="table-container">
        <table className="rounded-table">
          {teams.length ? renderBody() : renderEmpty()}
        </table>
      </div>
    </>
  )
}

export default TeamTable
