import classnames from "classnames";
import React, { useEffect, useMemo, useState } from "react";

import NgbEditModal from "../../components/modals/NgbEditModal";
import TestEditModal from "../../components/modals/TestEditModal";
import NgbTable from "../../components/tables/NgbTable";
import NewRefereeTable from "../../components/tables/RefereeTable";
import TestsTable from "../../components/tables/TestsTable";

import ActionsButton from "./ActionsButton";
import { useGetCurrentUserQuery } from "../../store/serviceApi";
import { useNavigate, useSearchParam } from "../../utils/navigationUtils";
import { useFeatureGates } from "../../utils/featureGateUtils";

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
  const [selectedTab, setSelectedTab] = useSearchParam<SelectedTab>("tab", { defaultValue: SelectedTab.Ngbs });
  const navigate = useNavigate();
  const { currentData: currentUser } = useGetCurrentUserQuery();
  const roles = currentUser?.roles?.map(r => r.roleType) ?? [];
  const { isTestFlag } = useFeatureGates();
  const isIqaAdmin = roles.includes("IqaAdmin");
  const canManageTests = isIqaAdmin || roles.includes("TestAdmin");
  const visibleTabs = useMemo(
    () => (isIqaAdmin ? [SelectedTab.Ngbs, SelectedTab.Referees, SelectedTab.Tests] : [SelectedTab.Tests]),
    [isIqaAdmin],
  );

  useEffect(() => {
    if (roles.length > 0 && !canManageTests) {
      navigate(-1);
      return;
    }

    if (!isIqaAdmin && selectedTab !== SelectedTab.Tests) {
      setSelectedTab(SelectedTab.Tests);
    }
  }, [canManageTests, isIqaAdmin, navigate, roles.length, selectedTab, setSelectedTab]);

  const isSelected = (tab: SelectedTab) => selectedTab === tab;

  const handleImportClick = () => navigate("/import/ngb/");
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
            onTestClick={handleOpenModal(SelectedModal.Test)}
            onImportClick={isIqaAdmin ? handleImportClick : undefined}
            onNgbClick={isIqaAdmin ? handleOpenModal(SelectedModal.Ngb) : undefined}
          />
        </div>
        <div className="tab-row">
          {visibleTabs.includes(SelectedTab.Ngbs) && (
            <button
              className={classnames({ "tab-selected": isSelected(SelectedTab.Ngbs) })}
              onClick={handleTabClick(SelectedTab.Ngbs)}
            >
              National Governing Bodies
            </button>
          )}
          {visibleTabs.includes(SelectedTab.Referees) && (
            <button
              className={classnames({ "tab-selected": isSelected(SelectedTab.Referees) })}
              onClick={handleTabClick(SelectedTab.Referees)}
            >
              Referees
            </button>
          )}
          {visibleTabs.includes(SelectedTab.Tests) && (
            <button
              className={classnames({ "tab-selected": isSelected(SelectedTab.Tests) })}
              onClick={handleTabClick(SelectedTab.Tests)}
            >
              Tests
            </button>
          )}
        </div>
        <div className="border border-t-0 p-4">{renderContent()}</div>
        {isTestFlag && (
          <div className="mt-4 p-4 bg-blue-100 border border-blue-400 rounded">
            <p className="text-sm text-blue-800">
              🚀 Test feature flag is enabled! This is a demonstration of the feature gates system.
            </p>
          </div>
        )}
      </div>
      {renderModals()}
    </>
  );
};

export default Admin;
