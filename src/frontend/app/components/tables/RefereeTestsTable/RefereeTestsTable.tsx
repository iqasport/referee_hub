import { capitalize } from "lodash";
import React, { useEffect } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { useHistory } from "react-router-dom";

import { getRefereeTests } from "../../../modules/test/tests";
import { RootState } from "../../../rootReducer";
import { Datum } from "../../../schemas/getTestsSchema";
import { getTestCertVersion } from "../../../utils/certUtils";
import Table, { CellConfig } from "../Table/Table";

const HEADER_CELLS = ["title", "level", "rulebook", "language"];

interface RefereeTestsTableProps {
  refId: string;
}

const RefereeTestsTable = (props: RefereeTestsTableProps) => {
  const { refId } = props;
  const history = useHistory();
  const dispatch = useDispatch();
  const { tests, isLoading, certifications } = useSelector(
    (state: RootState) => state.tests,
    shallowEqual
  );

  useEffect(() => {
    dispatch(getRefereeTests(refId));
  }, []);

  const handleRowClick = (id: string) => {
    history.push(`/referees/${refId}/tests/${id}`);
  };

  const renderEmpty = () => {
    return <h2>No available tests for this referee</h2>;
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
        return item.attributes.language;
      },
      dataKey: "language",
    },
  ];

  return (
    <>
      <h2 className="text-navy-blue text-2xl font-semibold my-4">{`Available Tests(${tests.length})`}</h2>
      <Table
        items={tests}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        rowConfig={rowConfig}
        isHeightRestricted={false}
      />
    </>
  );
};

export default RefereeTestsTable;
