import * as React from "react";
import { Tooltip } from "azure-devops-ui/TooltipEx";
import { Ago } from "azure-devops-ui/Ago";
import { Duration } from "azure-devops-ui/Duration";
import { IStatusProps, Statuses, Status, StatusSize } from "azure-devops-ui/Status";
import { ITableColumn, ColumnMore, SimpleTableCell, TwoLineTableCell, ColumnSorting, SortOrder, sortItems, Table } from "azure-devops-ui/Table";
import { AgoFormat } from "azure-devops-ui/Utilities/Date";
import { WithIcon } from "./Utilities";
import { ObservableValue } from "azure-devops-ui/Core/Observable";
import { ArrayItemProvider } from "azure-devops-ui/Utilities/Provider";
import { Observer } from "azure-devops-ui/Observer";

export interface PageContentState<T> {
  filtering: boolean;
  getData: boolean;
  items: T[];
}

/**
 * Plagiarism Set Entity
 */
export interface PlagiarismSetModel {

  /**
   * Plagiarism Set Id
   */
  setid: string;

  /**
   * Create time
   */
  create_time: string;

  /**
   * Creator user Id
   */
  creator: number | null | undefined;

  /**
   * Contest Id
   */
  related: number | null | undefined;

  /**
   * Formal name
   */
  formal_name: string;

  /**
   * The count of total reports
   */
  report_count: number;

  /**
   * The count of pending reports
   */
  report_pending: number;

  /**
   * The count of total submissions
   */
  submission_count: number;

  /**
   * The count of compilation failed submissions
   */
  submission_failed: number;

  /**
   * The count of compilation succeeded submissions
   */
  submission_succeeded: number;
}

export class PlagiarismSetTable extends React.Component {
  public render(): JSX.Element {
    return (
      <Observer itemProvider={this.itemProvider}>
        {(observableProps: {
          itemProvider: ArrayItemProvider<PlagiarismSetModel>;
        }) => (
          <Table<PlagiarismSetModel>
            ariaLabel="Advanced table"
            behaviors={[this.sortingBehavior]}
            className="table-example"
            columns={this.columns}
            containerClassName="h-scroll-auto"
            itemProvider={observableProps.itemProvider}
            showLines={true}
            onSelect={(event, data) =>
              console.log("Selected Row - " + data.index)
            }
            onActivate={(event, row) =>
              console.log("Activated Row - " + row.index)
            }
          />
        )}
      </Observer>
    );
  }

  private static getStatusIndicatorData(model: PlagiarismSetModel) : IStatusProps {
    if (model.report_count == 0 || model.submission_count == 0) {
      return { ...Statuses.Skipped, ariaLabel: "Empty" };
    } else if (model.report_pending > 0) {
      return Statuses.Running;
    } else if (model.submission_failed + model.submission_succeeded < model.submission_count) {
      return Statuses.Waiting;
    } else if (model.submission_failed > 0) {
      return Statuses.Warning;
    } else {
      return Statuses.Success;
    }
  }

  private static getPercentage(fz: number, fm: number) : string {
    if (fz == 0) return '0%';
    let ratio = Math.floor(fz * 10000 / fm) / 100;
    return ratio.toString() + '%';
  }

  private columns: ITableColumn<PlagiarismSetModel>[] = [
    {
      id: "name",
      name: "Name",
      renderCell: (rowIndex, columnIndex, tableColumn, tableItem) => (
        <SimpleTableCell
            columnIndex={columnIndex}
            tableColumn={tableColumn}
            key={"col-" + columnIndex}
            contentClassName="scroll-hidden"
        >
          <Status
              {...PlagiarismSetTable.getStatusIndicatorData(tableItem)}
              className="icon-large-margin"
              size={StatusSize.l}
          />
          <div className="flex-row scroll-hidden">
            <div>
              <div className="bolt-table-two-line-cell-item fontWeightSemiBold font-weight-semibold fontSizeM font-size-m">
                <Tooltip overflowOnly={true}>
                  <span className="text-ellipsis">{tableItem.formal_name}</span>
                </Tooltip>
              </div>
              <div className="bolt-table-two-line-cell-item fontSize secondary-text fontSizeS font-size-s">
                <Tooltip overflowOnly={true}>
                  <span>{tableItem.setid}</span>
                </Tooltip>
              </div>
            </div>
          </div>
        </SimpleTableCell>
      ),
      readonly: true,
      sortProps: {
        ariaLabelAscending: "Sorted name A to Z",
        ariaLabelDescending: "Sorted name Z to A",
      },
      width: -33,
    },
    {
      id: "time",
      ariaLabel: "Time",
      readonly: true,
      name: "Time",
      renderCell: (rowIndex, columnIndex, tableColumn, tableItem) => (
        <TwoLineTableCell
          key={"col-" + columnIndex}
          columnIndex={columnIndex}
          tableColumn={tableColumn}
          line1={(
            <WithIcon className="fontSize font-size" iconProps={{iconName: "Calendar"}}>
              <Ago date={new Date(tableItem.create_time)} format={AgoFormat.Extended} />
            </WithIcon>
          )}
          line2={(
            <>
              <WithIcon className="fontSize font-size bolt-table-two-line-cell-item icon-margin"
                        iconProps={{ iconName: "AnalyticsReport" }}
                        tooltipProps={{ text: `Reporting progress: ${tableItem.report_count - tableItem.report_pending} / ${tableItem.report_count}` }}>
                <span>{PlagiarismSetTable.getPercentage(tableItem.report_count - tableItem.report_pending, tableItem.report_count)}</span>
              </WithIcon>
              <WithIcon className="fontSize font-size bolt-table-two-line-cell-item"
                        iconProps={{ iconName: "FileCode" }}
                        tooltipProps={{ text: `Compilation progress: ${tableItem.submission_failed + tableItem.submission_succeeded} / ${tableItem.submission_count}` }}>
                <span>{PlagiarismSetTable.getPercentage(tableItem.submission_failed + tableItem.submission_succeeded, tableItem.submission_count)}</span>
              </WithIcon>
            </>
          )}
      />
      ),
      sortProps: {
        ariaLabelAscending: "Sorted time A to Z",
        ariaLabelDescending: "Sorted time Z to A",
      },
      width: -22,
    }
  ];

  private currentItems : PlagiarismSetModel[] = [
  ];

  private itemProvider = new ObservableValue<ArrayItemProvider<PlagiarismSetModel>>(
    new ArrayItemProvider(this.currentItems));

  private sortingBehavior = new ColumnSorting<PlagiarismSetModel>(
    (columnIndex: number, proposedSortOrder: SortOrder) => {
      this.itemProvider.value = new ArrayItemProvider(
        sortItems(
          columnIndex,
          proposedSortOrder,
          this.sortFunctions,
          this.columns,
          this.currentItems
        )
      );
    }
  );

  private sortFunctions = [
    // Sort on Name column
    (item1: PlagiarismSetModel, item2: PlagiarismSetModel) => {
      return item1.formal_name.localeCompare(item2.formal_name);
    },
    (item1: PlagiarismSetModel, item2: PlagiarismSetModel) => {
      return item1.create_time.localeCompare(item2.create_time);
    },
  ];
}
