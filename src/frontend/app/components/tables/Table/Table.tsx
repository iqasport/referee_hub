import classnames from "classnames";
import React from "react";

import Loader from "../../../components/Loader";
import { Datum as Ngb } from "../../../schemas/getNationalGoverningBodiesSchema";
import { Datum as Team } from "../../../schemas/getTeamsSchema";
import { Datum as Test } from "../../../schemas/getTestsSchema";
import { Referee } from "../../../modules/referee/referees";

export interface CellConfig<T> {
  dataKey: string;
  cellRenderer: (item: T) => JSX.Element | string;
  customStyle?: string;
}

interface TableProps<T> {
  rowConfig: CellConfig<T>[];
  headerCells: string[];
  items: T[];
  emptyRenderer: () => JSX.Element;
  isLoading: boolean;
  onRowClick?: (id: string) => void;
  isHeightRestricted: boolean;
  getId: (item: T) => string;
  disabled?: (item: T) => boolean;
}

const Table = <T extends any>(props: TableProps<T>) => {
  const {
    items,
    isLoading,
    headerCells,
    rowConfig,
    emptyRenderer,
    onRowClick,
    isHeightRestricted,
  } = props;

  const handleRowClick = (id: string) => () => {
    if (onRowClick) onRowClick(id);
  };

  const disabledTextClass = "text-gray-500"

  const renderRow = (item: T) => {
    const disabled = props.disabled && props.disabled(item);
    return (
      <tr key={props.getId(item)} className={`border border-gray-300 hover:bg-gray-300 ${disabled ? disabledTextClass : ""}`}>
        {rowConfig.map((cell) => {
          const handleClick = cell.dataKey === "actions" ? null : handleRowClick(props.getId(item));

          return (
            <td
              key={cell.dataKey}
              className={`w-1/4 py-4 px-8 ${cell.customStyle}`}
              onClick={disabled ? undefined : handleClick}
            >
              {cell.cellRenderer(item)}
            </td>
          );
        })}
      </tr>
    );
  };

  const renderBody = () => {
    return <tbody>{items.map(renderRow)}</tbody>;
  };

  const renderLoading = () => <Loader />;

  const renderEmpty = () => {
    return (
      <tbody>
        <tr>
          <td>{isLoading ? renderLoading() : emptyRenderer()}</td>
        </tr>
      </tbody>
    );
  };

  return (
    <>
      {items?.length > 0 && (
        <table className="rounded-table-header">
          <tbody>
            <tr className="text-left">
              {headerCells.map((header) => (
                <td
                  key={header}
                  className={classnames("w-1/4 py-4 px-8", { "text-right": header === "actions" })}
                >
                  {header}
                </td>
              ))}
            </tr>
          </tbody>
        </table>
      )}
      <div className={classnames("table-container", { "full-height-table": !isHeightRestricted })}>
        <table className="rounded-table">{items?.length ? renderBody() : renderEmpty()}</table>
      </div>
    </>
  );
};

export default Table;
