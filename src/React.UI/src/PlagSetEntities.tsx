import * as React from "react";
import { Tooltip } from "azure-devops-ui/TooltipEx";
import { Ago } from "azure-devops-ui/Ago";
import { IStatusProps, Statuses, Status, StatusSize } from "azure-devops-ui/Status";
import { ITableColumn, ColumnMore, SimpleTableCell, TwoLineTableCell, ColumnSorting, SortOrder, sortItems, Table, TableColumnLayout } from "azure-devops-ui/Table";
import { AgoFormat } from "azure-devops-ui/Utilities/Date";
import { WithIcon } from "./Utilities";
import { IReadonlyObservableValue, ObservableArray } from "azure-devops-ui/Core/Observable";
import { Card } from "azure-devops-ui/Card";

/**
 * Plagiarism Set Entity
 */
export interface PlagSetModel {

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

export interface PlagSetListProps {
  creator?: number;
  related?: number;
}

interface PlagSetTableProps {
  itemProvider: ObservableArray<PlagSetModel | IReadonlyObservableValue<PlagSetModel | undefined>>;
  itemClick: (model: PlagSetModel) => void;
}

export function getStatusIndicatorData(model: PlagSetModel) : IStatusProps {
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

export function getPercentage(fz: number, fm: number) : string {
  if (fz == 0) return '0%';
  let ratio = Math.floor(fz * 10000 / fm) / 100;
  return ratio.toString() + '%';
}

export class PlagSetTable extends React.Component<PlagSetTableProps> {

  constructor(props: PlagSetTableProps) {
    super(props);
  }

  public render(): JSX.Element {
    return (
      <Table<PlagSetModel>
        ariaLabel="Advanced table"
        //behaviors={[this.sortingBehavior]}
        className="table-example"
        columns={this.columns}
        containerClassName="h-scroll-auto"
        itemProvider={this.props.itemProvider}
        showLines={true}
        onActivate={(event, data) => this.props.itemClick(data.data)}
      />
    );
  }

  private columns: ITableColumn<PlagSetModel>[] = [
    {
      id: "name",
      name: "Name",
      columnLayout: TableColumnLayout.twoLinePrefix,
      renderCell: ((rowIndex, columnIndex, tableColumn, tableItem) => (
        <SimpleTableCell
            columnIndex={columnIndex}
            tableColumn={tableColumn}
            key={"col-" + columnIndex}
            contentClassName="scroll-hidden"
        >
          <Status
              {...getStatusIndicatorData(tableItem)}
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
      )),
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
      columnLayout: TableColumnLayout.twoLine,
      renderCell: ((rowIndex, columnIndex, tableColumn, tableItem) => (
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
                <span>{getPercentage(tableItem.report_count - tableItem.report_pending, tableItem.report_count)}</span>
              </WithIcon>
              <WithIcon className="fontSize font-size bolt-table-two-line-cell-item"
                        iconProps={{ iconName: "FileCode" }}
                        tooltipProps={{ text: `Compilation progress: ${tableItem.submission_failed + tableItem.submission_succeeded} / ${tableItem.submission_count}` }}>
                <span>{getPercentage(tableItem.submission_failed + tableItem.submission_succeeded, tableItem.submission_count)}</span>
              </WithIcon>
            </>
          )}
        />
      )),
      sortProps: {
        ariaLabelAscending: "Sorted time A to Z",
        ariaLabelDescending: "Sorted time Z to A",
      },
      width: -22,
    }
  ];
/*
  private sortingBehavior = new ColumnSorting<PlagiarismSetModel>(
    (columnIndex: number, proposedSortOrder: SortOrder) => {
      this.props.itemProvider.value = new ArrayItemProvider(
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
  ];*/
}

export function PlagSetInfoCard(props: { model: PlagSetModel }) : JSX.Element {
  return (
    <Card className="flex-grow" titleProps={{ text: "Summary", ariaLevel: 3 }}>
      <div className="flex-row summary-card-content" style={{ flexWrap: "wrap" }}>
        <div className="flex-column" style={{ minWidth: "372px", width: '50%' }} key={0}>
          <div className="body-m secondary-text summary-line-non-link">Create time and usage</div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "Calendar"}}>
              <Ago date={new Date(props.model.create_time)} format={AgoFormat.Extended} />
            </WithIcon>
          </div>
          <div className="body-m flex-row primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "Trophy2"}}>
              <span>Contest {props.model.related?.toLocaleString() ?? 'N/A'}</span>
            </WithIcon>
            <div style={{width: '12px'}} />
            <WithIcon iconProps={{iconName: "Contact"}}>
              <span>User {props.model.creator?.toLocaleString() ?? 'N/A'}</span>
            </WithIcon>
          </div>
        </div>
        <div className="flex-column" style={{ minWidth: "186px", width: '25%' }} key={1}>
          <div className="body-m secondary-text summary-line-non-link">Submissions</div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "FileCode"}}>
              <span>{props.model.submission_count} total</span>
            </WithIcon>
          </div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "Archive"}}>
              <span>{props.model.submission_succeeded} ok, {props.model.submission_failed} fail</span>
            </WithIcon>
          </div>
        </div>
        <div className="flex-column" style={{ minWidth: "186px", width: '25%' }} key={2}>
          <div className="body-m secondary-text summary-line-non-link">Reports</div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "CRMReport"}}>
              <span>{props.model.report_count} total</span>
            </WithIcon>
          </div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "Diagnostic"}}>
              <span>{props.model.report_count - props.model.report_pending} finished</span>
            </WithIcon>
          </div>
        </div>
      </div>
    </Card>
  );
}
