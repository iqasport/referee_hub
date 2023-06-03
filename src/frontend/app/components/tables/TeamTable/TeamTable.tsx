import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";

import { GetTeamsFilter } from "../../../apis/team";
import { deleteTeam } from "../../../modules/team/team";
import { getTeams, TeamsState, updateFilters } from "../../../modules/team/teams";
import { RootState } from "../../../rootReducer";
import { Datum } from "../../../schemas/getTeamsSchema";
import { toDateTime } from "../../../utils/dateUtils";

import FilterToolbar from "../../../components/FilterToolbar";
import TeamEditModal from "../../../components/modals/TeamEditModal";
import WarningModal from "../../../components/modals/WarningModal";
import Table, { CellConfig } from "../../../components/tables/Table/Table";

import ActionDropdown from "./ActionDropdown";

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

  const dispatch = useDispatch();
  const { teams, isLoading, meta, filters } = useSelector(
    (state: RootState): TeamsState => state.teams,
    shallowEqual
  );

  useEffect(() => {
    const filter: GetTeamsFilter = {
      nationalGoverningBodies: [parseInt(props.ngbId, 10)],
      nationalGoverningBodyId: ngbId,
      page: 1,
      limit: 25,
    };
    dispatch(updateFilters(filter));
    dispatch(getTeams(filter));
  }, [ngbId]);

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
    dispatch(deleteTeam(activeTeamId, ngbId));
    handleCloseModal();
  };

  const handleClearSearch = () => handleSearch("");

  const handleSearch = (newValue: string) => {
    const newFilters: GetTeamsFilter = { ...filters, q: newValue };
    dispatch(updateFilters(newFilters));
    dispatch(getTeams(newFilters));
  };

  const handlePageSelect = (newPage: number) => {
    const newFilters: GetTeamsFilter = { ...filters, page: newPage };
    dispatch(updateFilters(newFilters));
    dispatch(getTeams(newFilters));
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

  const HEADER_CELLS = ["name", "city", "joined date", "type", "status", "actions"];
  const rowConfig: CellConfig<Datum>[] = [
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.name;
      },
      dataKey: "name",
    },
    {
      cellRenderer: (item: Datum) => {
        const stateField = item.attributes.state ? `, ${item.attributes.state}` : "";
        return `${item.attributes.city}${stateField}`;
      },
      dataKey: "city",
    },
    {
      cellRenderer: (item: Datum) => {
        return toDateTime(item.attributes.joinedAt).toFormat("DDD");
      },
      dataKey: "joinedAt",
    },
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.groupAffiliation;
      },
      dataKey: "groupAffiliation",
    },
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.status;
      },
      dataKey: "status",
    },
    {
      cellRenderer: (item: Datum) => {
        return (
          <ActionDropdown
            teamId={item.id}
            onEditClick={handleEditClick}
            onDeleteClick={handleDeleteClick}
          />
        );
      },
      customStyle: "text-right",
      dataKey: "actions",
    },
  ];

  return (
    <div className="w-full">
      {teams.length > 0 && (
        <FilterToolbar
          currentPage={parseInt(meta?.page, 10)}
          onClearSearch={handleClearSearch}
          total={meta?.total}
          onSearchInput={handleSearch}
          onPageSelect={handlePageSelect}
        />
      )}
      <Table
        items={teams}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        rowConfig={rowConfig}
        emptyRenderer={renderEmpty}
        isHeightRestricted={true}
      />
      {renderModals()}
    </div>
  );
};

export default TeamTable;
