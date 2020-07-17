import classnames from 'classnames'
import React, { useState } from 'react'

import NewRefereeTable from '../../components/NewRefereeTable'
import TeamTable from '../../components/TeamTable'

enum SelectedTable {
  Referees = 'referees',
  Teams = 'teams',
}

interface NgbTablesProps {
  ngbId: number;
}

const NgbTables = (props: NgbTablesProps) => {
  const { ngbId } = props
  const [selectedTable, setSelectedTable] = useState(SelectedTable.Referees)
  const isRefereesActive = selectedTable === SelectedTable.Referees
  const isTeamsActive = selectedTable === SelectedTable.Teams

  const handleTableToggle = (table: SelectedTable) => (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    event.preventDefault()
    if (selectedTable === table) return

    setSelectedTable(table)
  }

  return (
    <div className="w-full">
      <div className="flex justify-start w-full mt-8">
        <button
          className={classnames('button-tab', { ['active-button-tab']: isRefereesActive })}
          onClick={handleTableToggle(SelectedTable.Referees)}
        >
          Referees
        </button>
        <button
          className={classnames('button-tab', { ['active-button-tab']: isTeamsActive })}
          onClick={handleTableToggle(SelectedTable.Teams)}
        >
          Teams
        </button>
      </div>
      {isRefereesActive && <NewRefereeTable ngbId={ngbId} />}
      {isTeamsActive && <TeamTable ngbId={ngbId.toString()} />}
    </div>
  )
}

export default NgbTables