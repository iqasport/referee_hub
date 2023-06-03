import { capitalize } from "lodash";
import React, { useEffect } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { useHistory } from "react-router-dom";

import { GetRefereesFilter } from "../../../apis/referee";
import { getReferees, Referee, updateFilters } from "../../../modules/referee/referees";
import { RootState } from "../../../rootReducer";
import { AssociationType } from "../../../schemas/getRefereesSchema";
import { getVersion } from "../../../utils/certUtils";
import FilterToolbar from "../../FilterToolbar";
import Table, { CellConfig } from "../Table/Table";

const HEADER_CELLS = ["name", "highest certification", "associated teams", "secondary NGB"];
const ADMIN_HEADER_CELLS = ["name", "highest certification", "associated teams", "associated NGBs"];

// sorts the levels by string length resulting in: ['head', 'snitch', 'assistant']
// this also happens to be the hierarchy order of the levels.
const sortByLength = (a: string, b: string): number => {
  return a.length - b.length;
};

const findHighestCert = (referee: Referee): string => {
  const certHashMap: { [version: string]: string[] } = {};
  referee?.certifications.forEach((cert) => {
    if (certHashMap[cert.version]) {
      certHashMap[cert.version].push(cert.level);
    } else {
      certHashMap[cert.version] = [cert.level];
    }
  });
  if (!Object.keys(certHashMap).length) return "Uncertified";

  const highestTwenty = certHashMap.twenty?.sort(sortByLength)[0];
  const highestTwentyTwo = certHashMap.twentytwo?.sort(sortByLength)[0];
  const highestEighteen = certHashMap.eighteen?.sort(sortByLength)[0];

  if (highestTwentyTwo?.length > 0 && highestTwentyTwo?.length < highestTwenty?.length) {
    return `${capitalize(highestTwentyTwo)} ${getVersion("twentytwo")}`;
  }

  if (highestTwenty?.length > 0 && highestTwenty?.length < highestEighteen?.length) {
    return `${capitalize(highestTwenty)} ${getVersion("twenty")}`;
  }

  return `${capitalize(highestEighteen)} ${getVersion("eighteen")}`;
};

type NewRefereeTableProps = {
  ngbId?: number;
  isHeightRestricted?: boolean;
};

const NewRefereeTable = (props: NewRefereeTableProps) => {
  const { ngbId, isHeightRestricted } = props;
  const history = useHistory();
  const dispatch = useDispatch();
  const { referees, isLoading, meta, filters } = useSelector(
    (state: RootState) => state.referees,
    shallowEqual
  );

  const headerCells = ngbId ? HEADER_CELLS : ADMIN_HEADER_CELLS;

  useEffect(() => {
    const filter: GetRefereesFilter = {};
    if (props.ngbId) filter.nationalGoverningBodies = [props.ngbId];

    dispatch(updateFilters(filter));
    dispatch(getReferees(filter));
  }, []);

  const handleRowClick = (id: string) => {
    history.push(`/referees/${id}`);
  };

  const handleClearSearch = () => handleSearch("");

  const handleSearch = (newValue: string) => {
    const newFilters: GetRefereesFilter = { ...filters, q: newValue };
    dispatch(updateFilters(newFilters));
    dispatch(getReferees(newFilters));
  };

  const handlePageSelect = (newPage: number) => {
    const newFilters: GetRefereesFilter = { ...filters, page: newPage };
    dispatch(updateFilters(newFilters));
    dispatch(getReferees(newFilters));
  };

  const renderEmpty = () => {
    return <h2>No referees found.</h2>;
  };

  const rowConfig: CellConfig<Referee>[] = [
    {
      cellRenderer: (item: Referee) => {
        if (!item?.referee.firstName) return "Anonymous Referee";
        return `${item?.referee.firstName} ${item?.referee.lastName}`;
      },
      dataKey: "name",
    },
    {
      cellRenderer: (item: Referee) => {
        return findHighestCert(item);
      },
      dataKey: "certifications",
    },
    {
      cellRenderer: (item: Referee) => {
        return item?.teams.map((team) => team.name).join(", ");
      },
      dataKey: "teams",
    },
  ];

  if (ngbId) {
    rowConfig.push({
      cellRenderer: (item: Referee) => {
        const secondary = item?.locations.filter(
          (location) => location.associationType === AssociationType.Secondary
        );
        const secondaryName =
          secondary.length &&
          item?.ngbs.find((ngb) => {
            return ngb.id === secondary[0].nationalGoverningBodyId.toString();
          })?.name;
        return secondaryName || "N/A";
      },
      dataKey: "locations",
    });
  } else {
    rowConfig.push({
      cellRenderer: (item: Referee) => {
        return item?.ngbs.map((location) => location.name).join(", ");
      },
      dataKey: "locations",
    });
  }

  return (
    <div className="w-full">
      <FilterToolbar
        currentPage={parseInt(meta?.page, 10)}
        onClearSearch={handleClearSearch}
        total={meta?.total}
        onSearchInput={handleSearch}
        onPageSelect={handlePageSelect}
      />
      <Table
        items={referees}
        isLoading={isLoading}
        headerCells={headerCells}
        rowConfig={rowConfig}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        isHeightRestricted={isHeightRestricted}
      />
    </div>
  );
};

export default NewRefereeTable;
