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
import NgbAdminsModal from "../../components/modals/NgbAdminsModal/NgbAdminsModal";

enum ModalType {
  Export = "export",
  Team = "team",
  Edit = "edit",
  Admins = "admins",
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
  const handleImportClick = () => navigate(`/import/team/${ngbId}`);

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
      case ModalType.Admins:
        return <NgbAdminsModal open={true} onClose={handleCloseModal} showClose={true} ngbId={ngbId} />;
      default:
        return null;
    }
  };

  const renderProfile = () => {
    return (
      <>
        <div className="flex items-start justify-between gap-4 mb-6">
          <h1 className="text-3xl font-bold text-gray-900">{ngb.name}</h1>
          <ActionsButton
            onEditClick={handleOpenModal(ModalType.Edit)}
            onImportClick={handleImportClick}
            onExportClick={handleOpenModal(ModalType.Export)}
            onCreateTeamClick={handleOpenModal(ModalType.Team)}
            onManageAdminsClick={handleOpenModal(ModalType.Admins)}
          />
        </div>

        <div className="tournament-details-grid" style={{ marginTop: "1.5rem" }}>
          <div>
            <StatsViewer stats={[ngb.currentStats, ...ngb.historicalStats]} />
            <NgbTables ngbId={ngbId} />
          </div>
          <div>
            <Sidebar ngb={ngb} />
          </div>
        </div>
      </>
    );
  };
  if (!ngb) return null;
  return (
    <section className="tournament-details-section">
      <div className="tournament-details-wrapper" style={{ maxWidth: "90rem" }}>
        {isLoading ? <Loader /> : renderProfile()}
        {renderModals()}
      </div>
    </section>
  );
};

export default NgbProfile;
