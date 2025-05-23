import { faCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import { capitalize } from "lodash";
import React, { useMemo, useState } from "react";

import { getTestCertVersion, getVersion } from "../../../utils/certUtils";
import { toDateTime } from "../../../utils/dateUtils";
import { formatLanguage } from "../../../utils/langUtils";
import TestEditModal from "../../modals/TestEditModal";
import WarningModal from "../../modals/WarningModal";
import Table, { CellConfig } from "../Table/Table";

import ActionDropdown from "./ActionDropdown";
import { useNavigate } from "../../../utils/navigationUtils";
import { TestViewModel, useGetAllTestsQuery, useGetLanguagesQuery, useSetTestActiveMutation } from "../../../store/serviceApi";

const HEADER_CELLS = ["title", "level", "version", "language", "active", /*"last updated",*/ "actions"];

enum ActiveModal {
  Edit = "edit",
  Delete = "delete",
}

const TestsTable = () => {
  const [activeModal, setActiveModal] = useState<ActiveModal>(null);
  const [activeTest, setActiveTest] = useState<string>(null);

  const [versionFilter, setVersionFilter] = useState<string>("*");
  const [langFilter, setLangFilter] = useState<string>("*");
 
  const navigate = useNavigate();

  const { data: tests, isLoading } = useGetAllTestsQuery();
  const [updateTestActive] = useSetTestActiveMutation();

  const filteredTests = useMemo(() => tests?.filter(t => {
    if (versionFilter !== "*" && t.awardedCertification.version !== versionFilter) {
      return false;
    }
    if (langFilter !== "*" && !t.language.startsWith(langFilter)) {
      return false;
    }
    return true;
  }), [versionFilter, langFilter, tests]);

  const handleRowClick = (id: string) => {
    navigate(`/admin/tests/${id}`);
  };

  const handleActiveToggle = (currentValue: boolean) => (testId: string) => {
    updateTestActive({testId, body: !currentValue});
  };

  const handleModalClick = (newModal: ActiveModal) => (testId: string) => {
    setActiveTest(testId);
    setActiveModal(newModal);
  };
  const handleModalClose = () => setActiveModal(null);
  //const handleDelete = () => dispatch(deleteTest(activeTest));

  const renderEmpty = () => {
    return <h2>No tests found</h2>;
  };

  const rowConfig: CellConfig<TestViewModel>[] = [
    {
      cellRenderer: (item: TestViewModel) => {
        return item.title;
      },
      dataKey: "name",
    },
    {
      cellRenderer: (item: TestViewModel) => {
        const level = item.awardedCertification.level == 'snitch' ? 'flag' : item.awardedCertification.level;
        return capitalize(level);
      },
      dataKey: "level",
    },
    {
      cellRenderer: (item: TestViewModel) => {
        return getVersion(item.awardedCertification.version);
      },
      dataKey: "certificationId",
    },
    {
      cellRenderer: (item: TestViewModel) => {
        return item.language;
      },
      dataKey: "newLanguageId",
    },
    {
      cellRenderer: (item: TestViewModel) => {
        return (
          <FontAwesomeIcon
            icon={faCircle}
            className={classnames("text-gray-500", { "text-green": item.active })}
          />
        );
      },
      dataKey: "active",
    },
    /*{
      cellRenderer: (item: TestViewModel) => {
        return toDateTime(item.attributes.updatedAt).toFormat("D");
      },
      dataKey: "updatedAt",
    },*/
    {
      cellRenderer: (item: TestViewModel) => {
        return (
          <ActionDropdown
            testId={item.testId}
            onActiveToggle={handleActiveToggle(item.active)}
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
      /*case ActiveModal.Delete:
        return (
          <WarningModal
            open={true}
            action="delete"
            dataType="test"
            onCancel={handleModalClose}
            onConfirm={handleDelete}
          />
        );*/
      default:
        return null;
    }
  };

  return (
    <>
      <select onChange={(evt) => setVersionFilter(evt.target.value)}>
        <option value="*">Any version</option>
        <option value="eighteen">{getVersion("eighteen")}</option>
        <option value="twenty">{getVersion("twenty")}</option>
        <option value="twentytwo">{getVersion("twentytwo")}</option>
        <option value="twentyfour">{getVersion("twentyfour")}</option>
      </select>
      <select onChange={(evt) => setLangFilter(evt.target.value)}>
        <option value="*">Any language</option>
        <option value="ca">Catalan</option>
        <option value="en">English</option>
        <option value="de">German</option>
        <option value="it">Italian</option>
        <option value="fr">French</option>
        <option value="pt">Portuguese</option>
        <option value="es">Spanish</option>
      </select>
      <Table
        items={filteredTests}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        rowConfig={rowConfig}
        isHeightRestricted={false}
        getId={test => test.testId}
      />
      {renderModals()}
    </>
  );
};

export default TestsTable;
