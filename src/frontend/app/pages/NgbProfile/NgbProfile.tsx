import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { useParams, useNavigate } from "react-router-dom";

import Loader from "../../components/Loader";
import ExportModal, { ExportType } from "../../components/modals/ExportModal/ExportModal";
import NgbEditModal from "../../components/modals/NgbEditModal";
import TeamEditModal from "../../components/modals/TeamEditModal";
import StatsViewer from "../../components/StatsViewer";
import { CurrentUserState } from "../../modules/currentUser/currentUser";
import { exportNgbReferees, exportNgbTeams } from "../../modules/job/job";
import {
  getNationalGoverningBody,
  SingleNationalGoverningBodyState,
} from "../../modules/nationalGoverningBody/nationalGoverningBody";
import { RootState } from "../../rootReducer";

import ActionsButton from "./ActionsButton";
import NgbTables from "./NgbTables";
import Sidebar from "./Sidebar";
import { AppDispatch } from "../../store";

type IdParams = { id: string };

enum ModalType {
  Export = "export",
  Team = "team",
  Edit = "edit",
}

const NgbProfile = () => {
  const { id } = useParams<IdParams>();
  const [openModal, setOpenModal] = useState<ModalType>();
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { ngb, socialAccounts, refereeCount, teamCount, stats, isLoading } = useSelector(
    (state: RootState): SingleNationalGoverningBodyState => state.nationalGoverningBody,
    shallowEqual
  );
  const { currentUser, roles } = useSelector(
    (state: RootState): CurrentUserState => state.currentUser,
    shallowEqual
  );

  useEffect(() => {
    if (id) {
      dispatch(getNationalGoverningBody(parseInt(id, 10)));
    }
  }, [id, dispatch]);

  if (!ngb) return null;
  const isUserReferee = roles.length === 1 && roles.includes("referee");
  const isUserNgbAdmin = roles.includes("ngb_admin") && Number(id) === currentUser.ownedNgbId;

  if (isUserReferee) {
    navigate(-1);
  } else if (!roles.includes("iqa_admin") && !isUserNgbAdmin) {
    navigate(-1);
  }

  const handleOpenModal = (type: ModalType) => () => setOpenModal(type);
  const handleCloseModal = () => setOpenModal(null);
  const handleExport = (type: ExportType) => {
    handleCloseModal();

    switch (type) {
      case ExportType.Team:
        dispatch(exportNgbTeams(id));
        break;
      case ExportType.Referee:
        dispatch(exportNgbReferees(id));
        break;
    }
  };
  const handleImportClick = () => navigate(`/import/team_${id}`);

  const renderModals = () => {
    switch (openModal) {
      case ModalType.Export:
        return <ExportModal open={true} onClose={handleCloseModal} onExport={handleExport} />;
      case ModalType.Team:
        return <TeamEditModal open={true} onClose={handleCloseModal} showClose={true} ngbId={id} />;
      case ModalType.Edit:
        return (
          <NgbEditModal
            open={true}
            onClose={handleCloseModal}
            showClose={true}
            ngbId={parseInt(id, 10)}
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
            onTeamClick={handleOpenModal(ModalType.Team)}
          />
        </div>
        <div className="flex w-full flex-col md:flex-row">
          <Sidebar
            ngb={ngb}
            socialAccounts={socialAccounts}
            refereeCount={refereeCount}
            teamCount={teamCount}
            isEditing={false}
            ngbId={id}
          />
          <div className="flex flex-col w-full md:w-4/5 md:pl-8">
            <StatsViewer stats={stats} />
            <NgbTables ngbId={parseInt(id, 10)} />
          </div>
        </div>
      </>
    );
  };
  return (
    <div className="w-5/6 mx-auto my-8">
      {isLoading ? <Loader /> : renderProfile()}
      {renderModals()}
    </div>
  );
};

export default NgbProfile;
