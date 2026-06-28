import classnames from "classnames";
import React, { useState } from "react";

import NewRefereeTable from "../../components/tables/RefereeTable";
import TeamTable from "../../components/tables/TeamTable";

enum SelectedTable {
  Referees = "referees",
  Teams = "teams",
  Transfers = "transfers",
}

interface NgbTablesProps {
  ngbId: string;
}

const NgbTables = (props: NgbTablesProps) => {
  const { ngbId } = props;
  const [selectedTable, setSelectedTable] = useState(SelectedTable.Referees);
  const isRefereesActive = selectedTable === SelectedTable.Referees;
  const isTeamsActive = selectedTable === SelectedTable.Teams;
  const isTransfersActive = selectedTable === SelectedTable.Transfers;

  const handleTableToggle = (table: SelectedTable) => (
    event: React.MouseEvent<HTMLButtonElement, MouseEvent>
  ) => {
    event.preventDefault();
    if (selectedTable === table) return;

    setSelectedTable(table);
  };

  return (
    <section className="card">
      <div className="flex items-center justify-between mb-4">
        <h2 className="card-title" style={{ marginBottom: 0 }}>NGB Management</h2>
      </div>
      <div className="flex justify-start w-full mb-4">
        <button
          className={classnames("button-tab", { ["active-button-tab"]: isRefereesActive })}
          onClick={handleTableToggle(SelectedTable.Referees)}
        >
          Members
        </button>
        <button
          className={classnames("button-tab", { ["active-button-tab"]: isTeamsActive })}
          onClick={handleTableToggle(SelectedTable.Teams)}
        >
          Teams
        </button>
        <button
          className={classnames("button-tab", { ["active-button-tab"]: isTransfersActive })}
          onClick={handleTableToggle(SelectedTable.Transfers)}
        >
          Transfers
        </button>
      </div>
      {isRefereesActive && <NewRefereeTable ngbId={ngbId} isHeightRestricted={true} />}
      {isTeamsActive && <TeamTable ngbId={ngbId} />}
      {isTransfersActive && (
        <div className="w-full rounded-md border border-gray-300 bg-gray-100 p-4 text-gray-700">
          No transfers to show yet.
        </div>
      )}
    </section>
  );
};

export default NgbTables;
