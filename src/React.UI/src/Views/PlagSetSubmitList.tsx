import { Ago, AgoFormat, IReadonlyObservableValue, IStatusProps, ITableBreakpoint, ITableColumn, ObservableArray, ObservableValue, React, renderSimpleCell, ScreenBreakpoints, SimpleTableCell, Status, Statuses, StatusSize, Table, TableColumnLayout, Tooltip, ZeroData, ZeroDataActionType } from "../AzureDevOpsUI";
import { PlagiarismSubmission as PlagSubmitModel } from "../Models/PlagiarismSubmission";

interface PlagSetSubmitListCardProps {
  observableArray: ObservableArray<PlagSubmitModel | IReadonlyObservableValue<PlagSubmitModel | undefined>>;
  zeroDataAction: () => void;
  zeroDataActionText: string;
  onActivate: (model: PlagSubmitModel) => void;
}

export class PlagSetSubmitList extends React.Component<PlagSetSubmitListCardProps> {

  private static getStatus(token_produced: boolean | null) : IStatusProps {
    if (token_produced === null) {
      return Statuses.Waiting;
    } else if (token_produced) {
      return Statuses.Success;
    } else {
      return Statuses.Failed;
    }
  }

  private columns : ITableColumn<PlagSubmitModel>[] = [
    {
        id: "name",
        name: "Name",
        width: new ObservableValue(0),
        sortProps: {},
        readonly: true,
        columnLayout: TableColumnLayout.singleLinePrefix,
        renderCell: (rowIndex, columnIndex, tableColumn, item) => (
          <SimpleTableCell columnIndex={columnIndex}>
            <Status {...PlagSetSubmitList.getStatus(item.token_produced)} size={StatusSize.m} className="margin-right-8" />
            <Tooltip overflowOnly={true}>
              <span className="text-ellipsis body-m">#{item.submitid}: {item.name}</span>
            </Tooltip>
          </SimpleTableCell>
        ),
    },
    {
        id: "upload_time",
        name: "Upload time",
        width: new ObservableValue(0),
        sortProps: {},
        readonly: true,
        renderCell: (rowIndex, columnIndex, tableColumn, item) => (
          <SimpleTableCell columnIndex={columnIndex}>
            <Ago date={new Date(item.upload_time)} format={AgoFormat.Extended} />
          </SimpleTableCell>
        ),
    },
    {
        id: "exclusive_category",
        name: "Exclusive",
        width: new ObservableValue(0),
        sortProps: {},
        readonly: true,
        renderCell: renderSimpleCell as any,
    },
    {
        id: "inclusive_category",
        name: "Inclusive",
        width: new ObservableValue(0),
        sortProps: {},
        readonly: true,
        renderCell: renderSimpleCell,
    },
    {
        id: "language",
        name: "Language",
        width: new ObservableValue(0),
        readonly: true,
        sortProps: {},
        renderCell: (rowIndex, columnIndex, tableColumn, item) => (
          <SimpleTableCell columnIndex={columnIndex}>
            <Tooltip overflowOnly={true}>
              <span className="text-ellipsis body-m text-cap-like">{item.language}</span>
            </Tooltip>
          </SimpleTableCell>
        ),
    },
    {
        id: "max_percent",
        name: "Max %",
        width: new ObservableValue(0),
        readonly: true,
        sortProps: {},
        renderCell: (rowIndex, columnIndex, tableColumn, item) => (
          <SimpleTableCell columnIndex={columnIndex}>
            <Tooltip overflowOnly={true}>
              <span className="text-ellipsis body-m">{item.max_percent !== undefined ? Math.floor(item.max_percent) + '%' : 'N/A'}</span>
            </Tooltip>
          </SimpleTableCell>
        ),
    }
  ];
  
  private tableBreakpoints: ITableBreakpoint[] = [
    {
        breakpoint: ScreenBreakpoints.xsmall,
        columnWidths: [-100, 0, 0, 0, 0, 90],
    },
    {
        breakpoint: ScreenBreakpoints.small,
        columnWidths: [-40, -20, 0, 0, 0, 90],
    },
    {
        breakpoint: ScreenBreakpoints.medium,
        columnWidths: [-40, -30, 80, 80, 80, 90],
    },
    {
        breakpoint: ScreenBreakpoints.large,
        columnWidths: [-40, -25, -8, -8, -8, -8],
    },
  ];

  public render() {
    return this.props.observableArray.length > 0 ? (
      <Table<PlagSubmitModel>
          ariaLabel="Table with sorting"
          //behaviors={[sortingBehavior]}
          className="table-example"
          columns={this.columns}
          containerClassName="h-scroll-auto"
          itemProvider={this.props.observableArray}
          role="table"
          tableBreakpoints={this.tableBreakpoints}
          onActivate={(event, row) => this.props.onActivate(row.data)}
      />
    ) : (
      <ZeroData
          className="flex-grow vss-ZeroData-fullsize"
          primaryText="No uploaded submissions"
          secondaryText="Upload codes as zip archives to compare their similarity."
          imageAltText="No items"
          imagePath="https://cdn.vsassets.io/ext/ms.vss-code-web/tags-view-content/Content/no-results.YsM6nMXPytczbbtz.png"
          actionText={this.props.zeroDataActionText}
          actionType={ZeroDataActionType.ctaButton}
          onActionClick={(event, item) => this.props.zeroDataAction()}
      />
    );
  }
}
