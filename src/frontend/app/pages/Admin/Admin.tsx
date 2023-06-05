import classnames from "classnames";
import React, { useState } from "react";
import { shallowEqual, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";

import NgbEditModal from "../../components/modals/NgbEditModal";
import TestEditModal from "../../components/modals/TestEditModal";
import NgbTable from "../../components/tables/NgbTable";
import NewRefereeTable from "../../components/tables/RefereeTable";
import TestsTable from "../../components/tables/TestsTable";
import { CurrentUserState } from "../../modules/currentUser/currentUser";
import { RootState } from "../../rootReducer";

import ActionsButton from "./ActionsButton";
import { useGetCurrentUserQuery } from "../../store/serviceApi";

enum SelectedModal {
  Test = "test",
  Ngb = "ngb",
}

enum SelectedTab {
  Ngbs = "ngbs",
  Referees = "referees",
  Tests = "tests",
}

const Admin = () => {
  const [selectedModal, setSelectedModal] = useState<SelectedModal>();
  const [selectedTab, setSelectedTab] = useState<SelectedTab>(SelectedTab.Ngbs);
  const navigate = useNavigate();
  const { currentData: currentUser } = useGetCurrentUserQuery()
  const roles = currentUser?.roles?.map(r => r.roleType);

  if (roles.length && !roles.includes("IqaAdmin")) navigate(-1);

  const isSelected = (tab: SelectedTab) => selectedTab === tab;

  const handleImportClick = () => navigate("/import/ngb");
  const handleOpenModal = (modal: SelectedModal) => () => setSelectedModal(modal);
  const handleCloseModal = () => setSelectedModal(null);
  const handleTabClick = (tab: SelectedTab) => () => setSelectedTab(tab);

  const renderModals = () => {
    switch (selectedModal) {
      case SelectedModal.Test:
        return <TestEditModal open={true} showClose={true} onClose={handleCloseModal} />;
      case SelectedModal.Ngb:
        return <NgbEditModal open={true} showClose={true} onClose={handleCloseModal} />;
    }
  };

  const renderContent = () => {
    switch (selectedTab) {
      case SelectedTab.Ngbs:
        return <NgbTable />;
      case SelectedTab.Referees:
        return <NewRefereeTable isHeightRestricted={false} />;
      case SelectedTab.Tests:
        return <TestsTable />;
    }
  };

  return (
    <>
      <div className="w-5/6 mx-auto my-8">
        <div className="w-full flex justify-between items-center my-8">
          <h1 className="text-4xl font-extrabold">Admin Portal</h1>
          <ActionsButton
            onImportClick={handleImportClick}
            onTestClick={handleOpenModal(SelectedModal.Test)}
            onNgbClick={handleOpenModal(SelectedModal.Ngb)}
          />
        </div>
        <div className="tab-row">
          <button
            className={classnames({ "tab-selected": isSelected(SelectedTab.Ngbs) })}
            onClick={handleTabClick(SelectedTab.Ngbs)}
          >
            National Governing Bodies
          </button>
          <button
            className={classnames({ "tab-selected": isSelected(SelectedTab.Referees) })}
            onClick={handleTabClick(SelectedTab.Referees)}
          >
            Referees
          </button>
          <button
            className={classnames({ "tab-selected": isSelected(SelectedTab.Tests) })}
            onClick={handleTabClick(SelectedTab.Tests)}
          >
            Tests
          </button>
        </div>
        <div className="border border-t-0 p-4">{renderContent()}</div>
      </div>
      {renderModals()}
    </>
  );
};

export default Admin;
