import React, { useState } from "react";

import Loader from "../../components/Loader";
import ExportModal, { ExportType } from "../../components/modals/ExportModal/ExportModal";
import NgbEditModal from "../../components/modals/NgbEditModal";
import TeamEditModal from "../../components/modals/TeamEditModal";
import StatsViewer from "../../components/StatsViewer";

import ActionsButton from "./ActionsButton";
import NgbTables from "./NgbTables";
import Sidebar from "./Sidebar";
import { useNavigate, useNavigationParams } from "../../utils/navigationUtils";
import { useExportRefereesForNgbMutation, useExportTeamsForNgbMutation, useGetNgbInfoQuery } from "../../store/serviceApi";

enum ModalType {
  Export = "export",
  Team = "team",
  Edit = "edit",
}

const NgbProfile = () => {
  const { ngbId } = useNavigationParams<"ngbId">();
  const [openModal, setOpenModal] = useState<ModalType>();
  const navigate = useNavigate();

  const [exportNgbReferees, {data: exportRefereesData, error: exportRefereesError}] = useExportRefereesForNgbMutation();
  const [exportNgbTeams, {data: exportTeamsData, error: exportTeamsError}] = useExportTeamsForNgbMutation();

  const {data: ngb, isLoading} = useGetNgbInfoQuery({ ngb: ngbId });

  const handleOpenModal = (type: ModalType) => () => setOpenModal(type);
  const handleCloseModal = () => setOpenModal(null);
  const handleExport = (type: ExportType) => {
    handleCloseModal();

    switch (type) {
      case ExportType.Team:
        exportNgbTeams({ ngb: ngbId });
        break;
      case ExportType.Referee:
        exportNgbReferees({ ngb: ngbId });
        break;
    }
  };
  const handleImportClick = () => navigate(`/import/team_${ngbId}`);

  const renderModals = () => {
    switch (openModal) {
      case ModalType.Export:
        return <ExportModal open={true} onClose={handleCloseModal} onExport={handleExport} />;
      case ModalType.Team:
        return <TeamEditModal open={true} onClose={handleCloseModal} showClose={true} ngbId={ngbId} />;
      case ModalType.Edit:
        return (
          <NgbEditModal
            open={true}
            onClose={handleCloseModal}
            showClose={true}
            ngbId={ngbId}
          />
        );
      default:
        return null;
    }
  };

  const renderProfile = () => {
    return (
      <>
        <div className="flex justify-between w-full mb-8">
          <h1 className="w-full text-4xl text-navy-blue font-extrabold">{ngb.name}</h1>
          <ActionsButton
            onEditClick={handleOpenModal(ModalType.Edit)}
            onImportClick={handleImportClick}
            onExportClick={handleOpenModal(ModalType.Export)}
            onCreateTeamClick={handleOpenModal(ModalType.Team)}
          />
        </div>
        <div className="flex w-full flex-col md:flex-row">
          <Sidebar
            ngb={ngb}
          />
          <div className="flex flex-col w-full md:w-4/5 md:pl-8">
            <StatsViewer stats={[ngb.currentStats, ...ngb.historicalStats]} />
            <NgbTables ngbId={ngbId} />
          </div>
        </div>
      </>
    );
  };
  if (!ngb) return null;
  return (
    <div className="w-5/6 mx-auto my-8">
      {isLoading ? <Loader /> : renderProfile()}
      {renderModals()}
    </div>
  );
};

export default NgbProfile;
