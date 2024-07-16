import { faCaretLeft } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";

import Loader from "../../components/Loader";
import TestEditModal from "../../components/modals/TestEditModal";
import WarningModal from "../../components/modals/WarningModal";
import QuestionsManager from "../../components/QuestionsManager";
import Toggle from "../../components/Toggle";
import { CurrentUserState } from "../../modules/currentUser/currentUser";
import { exportTest } from "../../modules/job/job";
import { getLanguages } from "../../modules/language/languages";
import { deleteTest, getTest, updateTest } from "../../modules/test/single_test";
import { RootState } from "../../rootReducer";

import ActionsButton from "./ActionsButton";
import Details from "./Details";
import ExportTestModal from "./ExportTestModal";
import { AppDispatch } from "../../store";
import { useNavigate, useNavigationParams } from "../../utils/navigationUtils";

enum SelectedTab {
  Details = "details",
  Questions = "questions",
}

enum ActiveModal {
  Edit = "edit",
  Delete = "delete",
  Export = "export",
}

const Test = () => {
  const { testId } = useNavigationParams<"testId">();

  const [selectedTab, setSelectedTab] = useState<SelectedTab>(SelectedTab.Details);
  const [activeModal, setActiveModal] = useState<ActiveModal>(null);
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { test, isLoading } = useSelector((state: RootState) => state.test, shallowEqual);
  const { roles } = useSelector(
    (state: RootState): CurrentUserState => state.currentUser,
    shallowEqual
  );
  const { languages } = useSelector((state: RootState) => state.languages, shallowEqual);
  if (roles.length && !roles.includes("IqaAdmin")) navigate(-1);

  const isSelected = (tab: SelectedTab) => selectedTab === tab;

  useEffect(() => {
    dispatch(getTest(testId));
  }, [testId]);

  useEffect(() => {
    dispatch(getLanguages());
  }, []);

  const handleBackClick = () => navigate(-1);

  const handleToggle = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newTest = { active: event.currentTarget.checked };
    dispatch(updateTest(testId, newTest));
  };

  const handleImportClick = () => navigate(`/import/test_${testId}`);
  const handleTabClick = (newTab: SelectedTab) => () => {
    if (newTab === selectedTab) return null;

    setSelectedTab(newTab);
  };
  const handleModalClick = (newModal: ActiveModal) => () => setActiveModal(newModal);
  const handleModalClose = () => setActiveModal(null);
  const handleDelete = () => {
    dispatch(deleteTest(testId));
    handleBackClick();
  };
  const handleExport = () => {
    handleModalClose();
    dispatch(exportTest(test.id));
  };

  const renderContent = () => {
    switch (selectedTab) {
      case SelectedTab.Details:
        return <Details test={test} languages={languages} />;
      case SelectedTab.Questions:
        return <QuestionsManager testId={testId} />;
      default:
        return null;
    }
  };

  const renderModals = () => {
    switch (activeModal) {
      case ActiveModal.Edit:
        return (
          <TestEditModal
            testId={test.id}
            open={true}
            showClose={true}
            onClose={handleModalClose}
            shouldUpdateTests={false}
          />
        );
      case ActiveModal.Delete:
        return (
          <WarningModal
            open={true}
            action="delete"
            dataType="test"
            onCancel={handleModalClose}
            onConfirm={handleDelete}
          />
        );
      case ActiveModal.Export:
        return (
          <ExportTestModal
            open={true}
            testName={test.attributes.name}
            onClose={handleModalClose}
            onExport={handleExport}
          />
        );
    }
  };

  return (
    <>
      <div className="w-5/6 mx-auto my-8">
        <button className="block" onClick={handleBackClick}>
          <FontAwesomeIcon icon={faCaretLeft} className="mr-2" />
          back
        </button>
        <div className="w-full flex justify-end items-center">
          <div className="flex w-1/2 items-center">
            <h1 className="text-3xl font-extrabold text-right mr-4">{test?.attributes.name}</h1>
            <Toggle
              name="active"
              onChange={handleToggle}
              checked={test?.attributes.active || false}
            />
          </div>
          <ActionsButton
            onImportClick={handleImportClick}
            onEditClick={handleModalClick(ActiveModal.Edit)}
            onDeleteClick={handleModalClick(ActiveModal.Delete)}
            onExportClick={handleModalClick(ActiveModal.Export)}
          />
        </div>
        <div className="w-5/6 h-screen my-8 mx-auto">
          <div className="tab-row">
            <button
              className={classnames({ "tab-selected": isSelected(SelectedTab.Details) })}
              onClick={handleTabClick(SelectedTab.Details)}
            >
              Details
            </button>
            <button
              className={classnames({ "tab-selected": isSelected(SelectedTab.Questions) })}
              onClick={handleTabClick(SelectedTab.Questions)}
            >
              Question Manager
            </button>
          </div>
          <div className="border border-t-0 p-4">{isLoading ? <Loader /> : renderContent()}</div>
        </div>
      </div>
      {renderModals()}
    </>
  );
};

export default Test;
