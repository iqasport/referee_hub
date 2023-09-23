import React, { useState } from "react";
import { toDateTime } from "../../../utils/dateUtils";

import FilterToolbar from "../../../components/FilterToolbar";
import TeamEditModal from "../../../components/modals/TeamEditModal";
import WarningModal from "../../../components/modals/WarningModal";
import Table, { CellConfig } from "../../../components/tables/Table/Table";

import ActionDropdown from "./ActionDropdown";
import { NgbTeamViewModel, useGetNgbTeamsQuery } from "../../../store/serviceApi";

enum ModalType {
  Edit = "edit",
  Delete = "delete",
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

  const handleCloseModal = () => setOpenModal(null);
  const handleEditClick = (teamId: string) => {
    setActiveTeamId(teamId);
    setOpenModal(ModalType.Edit);
  };
  const handleDeleteClick = (teamId: string) => {
    setActiveTeamId(teamId);
    setOpenModal(ModalType.Delete);
  };
  const handleDeleteConfirm = () => {
    //dispatch(deleteTeam(activeTeamId, ngbId)); TODO
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
      default:
        return null;
    }
  };

  const renderEmpty = () => <h2>No teams found</h2>;

  const HEADER_CELLS = ["name", "city", "joined date", "type", "status"/*, "actions"*/];
  const rowConfig: CellConfig<NgbTeamViewModel>[] = [
    {
      cellRenderer: (item: NgbTeamViewModel) => {
        return item.name;
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
    /*{
      cellRenderer: (item: NgbTeamViewModel) => {
        return (
          <ActionDropdown
            teamId={item.teamId.id.toString()}
            onEditClick={handleEditClick}
            onDeleteClick={handleDeleteClick}
          />
        );
      },
      customStyle: "text-right",
      dataKey: "actions",
    },*/
  ];

  return (
    <div className="w-full">
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
