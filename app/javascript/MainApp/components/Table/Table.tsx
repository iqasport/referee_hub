import classnames from 'classnames'
import React from 'react'

import Loader from 'MainApp/components/Loader'
import { Datum as Ngb } from 'MainApp/schemas/getNationalGoverningBodiesSchema'
import { Datum as Team } from 'MainApp/schemas/getTeamsSchema'
import { Datum as Test } from 'MainApp/schemas/getTestsSchema'
import { Referee } from '../../modules/referee/referees'

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
}

const Table = <T extends Referee | Test | Team | Ngb>(props: TableProps<T>) => {
  const { items, isLoading, headerCells, rowConfig, emptyRenderer, onRowClick, isHeightRestricted } = props

  const handleRowClick = (id: string) => () => {
    if(onRowClick) onRowClick(id)
  }

  const renderRow = (item: T) => {
    return (
      <tr key={item.id} className="border border-gray-300 hover:bg-gray-300">
        {rowConfig.map((cell) => {
          const handleClick = cell.dataKey === 'actions' ? null : handleRowClick(item.id)

          return (
            <td
              key={cell.dataKey}
              className={`w-1/4 py-4 px-8 ${cell.customStyle}`}
              onClick={handleClick}
            >
              {cell.cellRenderer(item)}
            </td>
          )
        })}
      </tr>
    )
  }

  const renderBody = () => {
    return (
      <tbody>
        {items.map(renderRow)}
      </tbody>
    )
  }

  const renderLoading = () => <Loader />

  const renderEmpty = () => {
    return (
      <tbody>
        <tr>
          <td>
            {isLoading ? renderLoading() : emptyRenderer()}
          </td>
        </tr>
      </tbody>
    )
  }

  return (
    <>
      {items.length > 0 && (
        <table className="rounded-table-header">
          <tbody>
            <tr className="text-left">
              {headerCells.map((header) => (
                <td
                  key={header}
                  className={classnames("w-1/4 py-4 px-8", {'text-right': header === 'actions'})}
                >
                  {header}
                </td>
              ))}
            </tr>
          </tbody>
        </table>
      )}
      <div className={classnames("table-container", { 'full-height-table': !isHeightRestricted })}>
        <table className="rounded-table">
          {items.length ? renderBody() : renderEmpty()}
        </table>
      </div>
    </>
  )
}

export default Table
