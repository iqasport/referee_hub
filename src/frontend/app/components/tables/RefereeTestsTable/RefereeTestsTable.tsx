import { capitalize, startCase } from "lodash";
import React from "react";

import { getVersion } from "../../../utils/certUtils";
import Table, { CellConfig } from "../Table/Table";
import { RefereeEligibilityResult, RefereeTestAvailableViewModel, useGetAvailableTestsQuery } from "../../../store/serviceApi";
import { getErrorString } from "../../../utils/errorUtils";
import { faBan, faCircleCheck, faClock } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Loader from "../../Loader/Loader";
import { useNavigate } from "../../../utils/navigationUtils";

const HEADER_CELLS = ["title", "level", "rulebook", "language"];

interface RefereeTestsTableProps {
  refId: string;
}

const RefereeTestsTable = (props: RefereeTestsTableProps) => {
  const { refId } = props;
  const navigate = useNavigate();

  const { currentData: tests, isLoading, error: getTestsError } = useGetAvailableTestsQuery();

  // sorts first by level and then by eligibility
  // TODO: improve readability on this and correct sorting by level and add weights to eligibility status
  const testComparer = (a: RefereeTestAvailableViewModel, b: RefereeTestAvailableViewModel): number => {
    const levelComparison = a.level < b.level ? -1 : a.level === b.level ? 0 : 1;
    if (a.isRefereeEligible) {
      if (b.isRefereeEligible) return levelComparison;
      else return -1;
    }
    if (b.isRefereeEligible) return 1;
    return levelComparison;
  }
  const sortedTests = !tests ? undefined : [...tests].sort(testComparer);

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
        return (
          <div className="flex flex-col">
            <span>
              {renderTestEligibilityIcon(item.refereeEligibilityResult)}
              {item.title}
            </span>
            <em className="text-xs text-gray-600">{startCase(item.refereeEligibilityResult)}</em>
          </div>
        );
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
