import { capitalize } from "lodash";
import React, { useEffect, useState } from "react";

import NgbEditModal from "../../modals/NgbEditModal";
import Table, { CellConfig } from "../Table/Table";
import ActionDropdown from "./ActionDropdown";
import { useNavigate } from "../../../utils/navigationUtils";
import { NgbViewModel, useGetNgbsQuery } from "../../../store/serviceApi";

const HEADER_CELLS = [
  "name",
  "region",
  "membership status",
  "player count",
  // "team count",
  // "referee count",
  "actions",
];

const NgbTable = () => {
  const [activeEdit, setActiveEdit] = useState<string>(null);
  const navigate = useNavigate();

  var { data: nationalGoverningBodies, isLoading } = useGetNgbsQuery({skipPaging: true})

  const handleRowClick = (id: string) => navigate(`/national_governing_bodies/${id}`);
  const handleEditClick = (id: string) => setActiveEdit(id);
  const handleEditClose = () => setActiveEdit(null);

  const renderEmpty = () => <h2>No National Governing Bodies found</h2>;

  const rowConfig: CellConfig<NgbViewModel>[] = [
    {
      cellRenderer: (item: NgbViewModel) => {
        return item.name;
      },
      dataKey: "name",
    },
    {
      cellRenderer: (item: NgbViewModel) => {
        return item.region
          ?.split("_")
          .map((word) => capitalize(word))
          .join(" ");
      },
      dataKey: "region",
    },
    {
      cellRenderer: (item: NgbViewModel) => {
        return item.membershipStatus
          ?.split("_")
          .map((word) => capitalize(word))
          .join(" ");
      },
      dataKey: "membershipStatus",
    },
    {
      cellRenderer: (item: NgbViewModel) => {
        return item.playerCount.toString();
      },
      dataKey: "playerCount",
    },
    // {
    //   cellRenderer: (item: NgbViewModel) => {
    //     return item.teamCount.toString();
    //   },
    //   dataKey: "teamCount",
    // },
    // {
    //   cellRenderer: (item: NgbViewModel) => {
    //     return item?.refereeCount.toString();
    //   },
    //   dataKey: "refereeCount",
    // },
    {
      cellRenderer: (item: NgbViewModel) => {
        return <ActionDropdown ngbId={item.countryCode} onEditClick={handleEditClick} />;
      },
      customStyle: "text-right",
      dataKey: "actions",
    },
  ];

  return (
    <>
      <Table
        items={nationalGoverningBodies?.items}
        rowConfig={rowConfig}
        headerCells={HEADER_CELLS}
        isHeightRestricted={false}
        isLoading={isLoading}
        emptyRenderer={renderEmpty}
        onRowClick={handleRowClick}
        getId={ngb => ngb.countryCode}
      />
      {activeEdit ? (
        <NgbEditModal
          open={!!activeEdit}
          showClose={true}
          ngbId={activeEdit}
          onClose={handleEditClose}
        />
      ) : null}
    </>
  );
};

export default NgbTable;
