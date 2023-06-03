import { faCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import { capitalize } from "lodash";
import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { useHistory } from "react-router-dom";

import { getLanguages } from "../../../modules/language/languages";
import { deleteTest, updateTest } from "../../../modules/test/single_test";
import { getTests } from "../../../modules/test/tests";
import { RootState } from "../../../rootReducer";
import { Datum } from "../../../schemas/getTestsSchema";
import { getTestCertVersion } from "../../../utils/certUtils";
import { toDateTime } from "../../../utils/dateUtils";
import { formatLanguage } from "../../../utils/langUtils";
import TestEditModal from "../../modals/TestEditModal";
import WarningModal from "../../modals/WarningModal";
import Table, { CellConfig } from "../Table/Table";

import ActionDropdown from "./ActionDropdown";

const HEADER_CELLS = ["title", "level", "version", "language", "active", "last updated", "actions"];

enum ActiveModal {
  Edit = "edit",
  Delete = "delete",
}

const TestsTable = () => {
  const [activeModal, setActiveModal] = useState<ActiveModal>(null);
  const [activeTest, setActiveTest] = useState<string>(null);
  const history = useHistory();
  const dispatch = useDispatch();
  const { tests, isLoading, certifications } = useSelector(
    (state: RootState) => state.tests,
    shallowEqual
  );
  const { languages } = useSelector((state: RootState) => state.languages, shallowEqual);

  useEffect(() => {
    if (!tests?.length) {
      dispatch(getTests());
    }
    if (!languages?.length) {
      dispatch(getLanguages());
    }
  }, []);

  const handleRowClick = (id: string) => {
    history.push(`/admin/tests/${id}`);
  };

  const handleActiveToggle = (item: Datum) => (testId: string) => {
    const newValue = !item.attributes.active;
    const newTest = { active: newValue };
    dispatch(updateTest(testId, newTest));
  };

  const handleModalClick = (newModal: ActiveModal) => (testId: string) => {
    setActiveTest(testId);
    setActiveModal(newModal);
  };
  const handleModalClose = () => setActiveModal(null);
  const handleDelete = () => dispatch(deleteTest(activeTest));

  const renderEmpty = () => {
    return <h2>No tests found</h2>;
  };

  const rowConfig: CellConfig<Datum>[] = [
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.name;
      },
      dataKey: "name",
    },
    {
      cellRenderer: (item: Datum) => {
        return capitalize(item.attributes.level);
      },
      dataKey: "level",
    },
    {
      cellRenderer: (item: Datum) => {
        return getTestCertVersion(item.attributes.certificationId, certifications);
      },
      dataKey: "certificationId",
    },
    {
      cellRenderer: (item: Datum) => {
        if (!item.attributes.newLanguageId) return "---";

        const language = languages.find(
          (lang) => lang.id === item.attributes.newLanguageId.toString()
        );
        return formatLanguage(language);
      },
      dataKey: "newLanguageId",
    },
    {
      cellRenderer: (item: Datum) => {
        return (
          <FontAwesomeIcon
            icon={faCircle}
            className={classnames("text-gray-500", { "text-green": item.attributes.active })}
          />
        );
      },
      dataKey: "active",
    },
    {
      cellRenderer: (item: Datum) => {
        return toDateTime(item.attributes.updatedAt).toFormat("D");
      },
      dataKey: "updatedAt",
    },
    {
      cellRenderer: (item: Datum) => {
        return (
          <ActionDropdown
            testId={item.id}
            onActiveToggle={handleActiveToggle(item)}
            onEditClick={handleModalClick(ActiveModal.Edit)}
            onDeleteClick={handleModalClick(ActiveModal.Delete)}
          />
        );
      },
      customStyle: "text-right",
      dataKey: "actions",
    },
  ];

  const renderModals = () => {
    switch (activeModal) {
      case ActiveModal.Edit:
        return (
          <TestEditModal
            testId={activeTest}
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
      default:
        return null;
    }
  };

  return (
    <>
      <Table
        items={tests}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        rowConfig={rowConfig}
        isHeightRestricted={false}
      />
      {renderModals()}
    </>
  );
};

export default TestsTable;
