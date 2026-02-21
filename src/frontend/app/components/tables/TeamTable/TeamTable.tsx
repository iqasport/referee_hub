import React, { useState } from "react";
import { toDateTime } from "../../../utils/dateUtils";
import { Link } from "react-router-dom";

import FilterToolbar from "../../../components/FilterToolbar";
import TeamEditModal from "../../../components/modals/TeamEditModal";
import TeamManagersModal from "../../../components/modals/TeamManagersModal";
import WarningModal from "../../../components/modals/WarningModal";
import Table, { CellConfig } from "../../../components/tables/Table/Table";

import ActionDropdown from "./ActionDropdown";
import { NgbTeamViewModel, useDeleteNgbTeamMutation, useGetNgbTeamsQuery } from "../../../store/serviceApi";
import { getErrorString } from "../../../utils/errorUtils";

enum ModalType {
  Edit = "edit",
  Delete = "delete",
  Managers = "managers",
}

interface TeamTableProps {
  ngbId: string;
}

const TeamTable = (props: TeamTableProps) => {
  const { ngbId } = props;
  const [openModal, setOpenModal] = useState<ModalType>();
  const [activeTeamId, setActiveTeamId] = useState<string>();
  const [filter, setFilter] = useState<string | undefined>(undefined);
  const [page, setPage] = useState(1);

  const { data: teams, isLoading } = useGetNgbTeamsQuery({ngb: ngbId, filter, page, pageSize: 25})
  const [deleteTeam, {error: deleteTeamError}] = useDeleteNgbTeamMutation();

  const handleCloseModal = () => setOpenModal(null);
  const handleEditClick = (teamId: string) => {
    setActiveTeamId(teamId);
    setOpenModal(ModalType.Edit);
  };
  const handleDeleteClick = (teamId: string) => {
    setActiveTeamId(teamId);
    setOpenModal(ModalType.Delete);
  };
  const handleManageManagersClick = (teamId: string) => {
    setActiveTeamId(teamId);
    setOpenModal(ModalType.Managers);
  };
  const handleDeleteConfirm = () => {
    deleteTeam({ngb: ngbId, teamId: activeTeamId});
    handleCloseModal();
  };

  const handleClearSearch = () => handleSearch("");

  const handleSearch = (newValue: string) => {
    setFilter(newValue ?? undefined)
  };

  const handlePageSelect = (newPage: number) => {
    setPage(newPage);
  };

  const renderModals = () => {
    switch (openModal) {
      case ModalType.Edit:
        return (
          <TeamEditModal
            teamId={activeTeamId}
            open={true}
            onClose={handleCloseModal}
            showClose={true}
            ngbId={ngbId}
            team={teams.items.filter(t => t.teamId === activeTeamId)[0]}
          />
        );
      case ModalType.Delete:
        return (
          <WarningModal
            open={true}
            onCancel={handleCloseModal}
            action="delete"
            dataType="team"
            onConfirm={handleDeleteConfirm}
          />
        );
      case ModalType.Managers:
        return (
          <TeamManagersModal
            open={true}
            onClose={handleCloseModal}
            showClose={true}
            ngbId={ngbId}
            teamId={activeTeamId}
          />
        );
      default:
        return null;
    }
  };

  const renderEmpty = () => <h2>No teams found</h2>;

  const HEADER_CELLS = ["logo", "name", "city", "joined date", "type", "status", "actions"];
  const rowConfig: CellConfig<NgbTeamViewModel>[] = [
    {
      cellRenderer: (item: NgbTeamViewModel) => {
        return (
          <Link to={`/teams/${item.teamId}`} className="block">
            {item.logoUrl ? (
              <img 
                src={item.logoUrl} 
                alt={`${item.name} logo`} 
                className="w-12 h-12 object-cover rounded hover:opacity-80 transition-opacity"
              />
            ) : (
              <div className="w-12 h-12 bg-gray-200 rounded flex items-center justify-center text-gray-400 hover:bg-gray-300 transition-colors">
                <span className="text-xs">No logo</span>
              </div>
            )}
          </Link>
        );
      },
      dataKey: "logoUrl",
    },
    {
      cellRenderer: (item: NgbTeamViewModel) => {
        return (
          <Link to={`/teams/${item.teamId}`} className="text-blue-600 hover:underline">
            {item.name}
          </Link>
        );
      },
      dataKey: "name",
    },
    {
      cellRenderer: (item: NgbTeamViewModel) => {
        const stateField = item.state ? `, ${item.state}` : "";
        return `${item.city}${stateField}`;
      },
      dataKey: "city",
    },
    {
      cellRenderer: (item: NgbTeamViewModel) => {
        return toDateTime(item.joinedAt).toFormat("DDD");
      },
      dataKey: "joinedAt",
    },
    {
      cellRenderer: (item: NgbTeamViewModel) => {
        return item.groupAffiliation;
      },
      dataKey: "groupAffiliation",
    },
    {
      cellRenderer: (item: NgbTeamViewModel) => {
        return item.status;
      },
      dataKey: "status",
    },
    {
      cellRenderer: (item: NgbTeamViewModel) => {
        return (
          <ActionDropdown
            teamId={item.teamId.toString()}
            onEditClick={handleEditClick}
            onDeleteClick={handleDeleteClick}
            onManageManagersClick={handleManageManagersClick}
          />
        );
      },
      customStyle: "text-right",
      dataKey: "actions",
    },
  ];

  return (
    <div className="w-full">
      {deleteTeamError && <span style={{color: "red"}}>{getErrorString(deleteTeamError)}</span>}
      {teams?.items?.length > 0 && (
        <FilterToolbar
          currentPage={page}
          onClearSearch={handleClearSearch}
          total={teams.metadata?.totalCount}
          onSearchInput={handleSearch}
          onPageSelect={handlePageSelect}
        />
      )}
      <Table
        items={teams?.items}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        rowConfig={rowConfig}
        emptyRenderer={renderEmpty}
        isHeightRestricted={true}
        getId={team => team.teamId.toString()}
      />
      {renderModals()}
    </div>
  );
};

export default TeamTable;
