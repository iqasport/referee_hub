import { capitalize, startCase } from "lodash";
import React from "react";
import { useNavigate } from "react-router-dom";

import { getVersion } from "../../../utils/certUtils";
import Table, { CellConfig } from "../Table/Table";
import { RefereeEligibilityResult, RefereeTestAvailableViewModel, useGetAvailableTestsQuery } from "../../../store/serviceApi";
import { getErrorString } from "../../../utils/errorUtils";
import { faBan, faCircleCheck, faClock } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Loader from "../../Loader/Loader";

const HEADER_CELLS = ["title", "level", "rulebook", "language"];

interface RefereeTestsTableProps {
  refId: string;
}

const RefereeTestsTable = (props: RefereeTestsTableProps) => {
  const { refId } = props;
  const navigate = useNavigate();

  const { currentData: tests, isLoading, error: getTestsError } = useGetAvailableTestsQuery();

  const sortedTests = !tests ? undefined : [...tests].sort((a, b) =>
    a.refereeEligibilityResult === b.refereeEligibilityResult
      ? 0
      : a.refereeEligibilityResult === "Eligible"
        ? -1
        : a.level === b.level
          ? 0
          : a.level < b.level ? -1 : 1);

  const handleRowClick = (id: string) => {
    navigate(`/referees/${refId}/tests/${id}`);
  };

  const renderEmpty = () => {
    return <h2>No available tests for this referee</h2>;
  };
  
  const renderTestEligibilityIcon = (eligibilityResult: RefereeEligibilityResult) => {
    if (!eligibilityResult || eligibilityResult === "Eligible") return <></>;
    else if (eligibilityResult === "RefereeAlreadyCertified") return <FontAwesomeIcon icon={faCircleCheck} className="mr-2" />;
    else if (eligibilityResult === "InCooldownPeriod") return <FontAwesomeIcon icon={faClock} className="mr-2" />;
    else return <FontAwesomeIcon icon={faBan} className="mr-2" />;
  }

  const rowConfig: CellConfig<RefereeTestAvailableViewModel>[] = [
    {
      cellRenderer: (item) => {
        return (<span title={startCase(item.refereeEligibilityResult)}>
          {renderTestEligibilityIcon(item.refereeEligibilityResult)}
          {item.title}
        </span>);
      },
      dataKey: "name",
    },
    {
      cellRenderer: (item) => {
        return capitalize(item.level);
      },
      dataKey: "level",
    },
    {
      cellRenderer: (item) => {
        return getVersion(item.awardedCertifications[item.awardedCertifications.length-1].version);
      },
      dataKey: "certificationVersion",
    },
    {
      cellRenderer: (item) => {
        return item.language;
      },
      dataKey: "language",
    },
  ];

  if (getTestsError) return <p style={{color: "red"}}>{getErrorString(getTestsError)}</p>;
  if (!tests) return <Loader />;

  return (
    <>
      <h2 className="text-navy-blue text-2xl font-semibold my-4">{`Available Tests(${tests.filter(t => t.isRefereeEligible).length})`}</h2>
      <Table
        items={sortedTests}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        rowConfig={rowConfig}
        isHeightRestricted={false}
        getId={t => t.testId}
        disabled={t => !t.isRefereeEligible}
      />
    </>
  );
};

export default RefereeTestsTable;
