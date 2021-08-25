import { React, IReadonlyObservableValue, ObservableArray, Status, StatusSize, WithIcon, Tooltip, Table, ITableColumn, TableColumnLayout, SimpleTableCell, Ago, AgoFormat, TwoLineTableCell } from "../AzureDevOpsUI";
import { PlagiarismSet as PlagSetModel } from "../DataModels";
import { Helpers } from '../Components/Helpers';

export interface PlagSetTableProps {
  itemProvider: ObservableArray<PlagSetModel | IReadonlyObservableValue<PlagSetModel | undefined>>;
  itemClick: (model: PlagSetModel) => void;
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
              {...Helpers.getStatusIndicatorData(tableItem)}
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
                <span>{Helpers.getPercentage(tableItem.report_count - tableItem.report_pending, tableItem.report_count)}</span>
              </WithIcon>
              <WithIcon className="fontSize font-size bolt-table-two-line-cell-item"
                        iconProps={{ iconName: "FileCode" }}
                        tooltipProps={{ text: `Compilation progress: ${tableItem.submission_failed + tableItem.submission_succeeded} / ${tableItem.submission_count}` }}>
                <span>{Helpers.getPercentage(tableItem.submission_failed + tableItem.submission_succeeded, tableItem.submission_count)}</span>
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
