import { ArrayItemProvider } from "azure-devops-ui/Utilities/Provider";
import MonacoEditor from "react-monaco-editor";
import { Ago, AgoFormat, Card, Header, IStatusProps, ITableBreakpoint, ITableColumn, ObservableValue, React, renderSimpleCell, RouteComponentProps, ScreenBreakpoints, SimpleTableCell, Status, Statuses, StatusSize, Tab, TabBar, Table, TableColumnLayout, TabSize, TitleSize, Tooltip, WithIcon, ZeroData } from "../AzureDevOpsUI";
import { PlagiarismComparison, PlagiarismVertex as Vertex } from "../Models/PlagiarismComparison";

interface PlagSubmitViewProps {
  match: {
    params: {
      id: string;
      sid: number;
    }
  };
}

interface PlagSubmitViewState {
  loading: boolean;
  notFound: boolean;
  submitName?: string;
  reportCount?: number;
  sourceCodeNumber?: number;
  vertex?: Vertex;
}

export default class PlagSubmitView extends React.Component<RouteComponentProps & PlagSubmitViewProps, PlagSubmitViewState> {

  private loadController = new AbortController();
  private tabId = new ObservableValue<string>("tab1");

  constructor(props: RouteComponentProps & PlagSubmitViewProps) {
    super(props);

    this.state = {
      loading: true,
      notFound: false,
    };
  }

  public componentDidMount() {
    fetch('/api/plagiarism/sets/' + encodeURIComponent(this.props.match.params.id) + '/submissions/' + encodeURIComponent(this.props.match.params.sid) + '/comparisons?includeFiles=true', {
      method: 'GET',
      signal: this.loadController.signal
    }).then(resp => {
      return resp.ok ? resp.json() : null;
    }).then(json => {
      const vertex = json as Vertex;
      this.setState({
        ...this.state,
        loading: false,
        vertex: vertex,
        submitName: vertex.name,
        sourceCodeNumber: vertex.files?.length,
        reportCount: vertex.comparisons?.length
      });
    }).catch(reason => {
      console.log(reason);
      this.setState({
        ...this.state,
        loading: false,
        notFound: true
      })
    });
  }

  public componentWillUnmount() {
    this.loadController.abort();
  }

  private static getStatus(model: PlagiarismComparison) : IStatusProps {
    if (model.finished === null) {
      return Statuses.Waiting;
    } else if (!model.finished) {
      return Statuses.Running;
    } else if (model.justification === null) {
      return Statuses.Queued;
    } else if (model.justification) {
      return Statuses.Failed;
    } else {
      return Statuses.Success;
    }
  }

  private static renderPercentage(rowIndex: number, columnIndex: number, tableColumn: ITableColumn<PlagiarismComparison>, item: PlagiarismComparison) {
    const percent = (item as any)[tableColumn.id] as number;
    return (
      <SimpleTableCell columnIndex={columnIndex}>
        <Tooltip overflowOnly={true}>
          <span className="text-ellipsis body-m">{Math.floor(percent)}%</span>
        </Tooltip>
      </SimpleTableCell>
    );
  }

  private columns : ITableColumn<PlagiarismComparison>[] = [
    {
        id: "name",
        name: "Name",
        width: new ObservableValue(0),
        sortProps: {},
        readonly: true,
        columnLayout: TableColumnLayout.singleLinePrefix,
        renderCell: (rowIndex, columnIndex, tableColumn, item) => (
          <SimpleTableCell columnIndex={columnIndex}>
            <Status {...PlagSubmitView.getStatus(item)} size={StatusSize.m} className="margin-right-8" />
            <Tooltip overflowOnly={true}>
              <span className="text-ellipsis body-m">#{item.submitid}: {item.submit}</span>
            </Tooltip>
          </SimpleTableCell>
        ),
    },
    {
        id: "exclusive",
        name: "Exclusive",
        width: new ObservableValue(0),
        sortProps: {},
        readonly: true,
        renderCell: renderSimpleCell as any,
    },
    {
        id: "tokens_matched",
        name: "Matched",
        width: new ObservableValue(0),
        sortProps: {},
        readonly: true,
        renderCell: renderSimpleCell,
    },
    {
        id: "biggest_match",
        name: "Longest",
        width: new ObservableValue(0),
        sortProps: {},
        readonly: true,
        renderCell: renderSimpleCell,
    },
    {
        id: "percent",
        name: "Avg %",
        width: new ObservableValue(0),
        readonly: true,
        sortProps: {},
        renderCell: PlagSubmitView.renderPercentage,
    },
    {
        id: "percent_self",
        name: "This %",
        width: new ObservableValue(0),
        readonly: true,
        sortProps: {},
        renderCell: PlagSubmitView.renderPercentage,
    },
    {
        id: "percent_another",
        name: "That %",
        width: new ObservableValue(0),
        readonly: true,
        sortProps: {},
        renderCell: PlagSubmitView.renderPercentage,
    }
  ];

  private tableBreakpoints: ITableBreakpoint[] = [
    {
        breakpoint: ScreenBreakpoints.xsmall,
        columnWidths: [-100, 0, 0, 0, 90, 0, 0],
    },
    {
        breakpoint: ScreenBreakpoints.small,
        columnWidths: [-40, 90, 0, 0, 90, 0, 0],
    },
    {
        breakpoint: ScreenBreakpoints.medium,
        columnWidths: [-40, 90, 90, 90, 90, 90, 90],
    },
    {
        breakpoint: ScreenBreakpoints.large,
        columnWidths: [-52, -8, -8, -8, -8, -8, -8],
    },
  ];

  public render() {
    return (
      <>
        <Header
          backButtonProps={{ onClick: () => this.props.history.goBack() }}
          title={this.state.submitName ? `Submission: ${this.state.submitName}` : 'Submission'}
          titleAriaLevel={3}
          titleSize={TitleSize.Large}
        />
        <TabBar
          onSelectedTabChanged={newTabId => {
            document.getElementById(this.tabId.value + '-content')!.style.display = 'none';
            document.getElementById(newTabId + '-content')!.style.display = 'flex';
            this.tabId.value = newTabId;
          }}
          selectedTabId={this.tabId}
          tabSize={TabSize.Tall}
        >
          <Tab name="Summary" id="tab1" badgeCount={this.state.reportCount} iconProps={{ iconName: 'BranchMerge' }} />
          <Tab name="Source Code" id="tab3" badgeCount={this.state.sourceCodeNumber} iconProps={{ iconName: 'FileCode' }} />
        </TabBar>
        <div className="page-content page-content-top" id="tab-contents">
          <div className="flex-column flex-grow card-list" id="tab1-content" style={{ display: 'flex' }} >
            <Card titleProps={{ text: 'Summary', ariaLevel: 3 }}>
              <div className="summary-card-content flex-grow" style={{ flexWrap: "wrap" }}>
                <div className="flex-column summary-card-content-item flex-grow" key={0}>
                  <div className="body-m secondary-text summary-line-non-link">Overview</div>
                  {this.state.vertex
                    ? <>
                        <div className="body-m flex-row primary-text summary-line-non-link">
                          <WithIcon iconProps={{iconName: "Calendar"}}>
                            <Ago date={new Date(this.state.vertex.upload_time)} format={AgoFormat.Extended} />
                          </WithIcon>
                          <WithIcon iconProps={{iconName: "CalculatorPercentage"}}>
                            <span>{Math.floor(this.state.vertex.max_percent ?? 0)}% max</span>
                          </WithIcon>
                        </div>
                        <div className="body-m flex-row primary-text summary-line-non-link">
                          <WithIcon iconProps={{iconName: "FabricSyncFolder"}}>
                            <span>{this.state.vertex.setid}</span>
                          </WithIcon>
                        </div>
                        <div className="body-m flex-row primary-text summary-line-non-link">
                          <WithIcon iconProps={{iconName: "Accounts"}}>
                            <span>{this.state.vertex.externalid}</span>
                          </WithIcon>
                        </div>
                      </>
                    : <>
                        <div className="shimmer shimmer-line" style={{ width: '18em', margin: '3px' }}>&nbsp;</div>
                        <div className="shimmer shimmer-line" style={{ width: '24em', margin: '3px' }}>&nbsp;</div>
                        <div className="shimmer shimmer-line" style={{ width: '21em', margin: '3px' }}>&nbsp;</div>
                      </>
                  }
                </div>
                <div className="flex-column summary-card-content-item flex-grow" key={0}>
                  <div className="body-m secondary-text summary-line-non-link">Category</div>
                  {this.state.vertex
                    ? <>
                        <div className="body-m flex-row primary-text summary-line-non-link">
                          <WithIcon iconProps={{iconName: "Contact"}}>
                            <span>Exclusive category {this.state.vertex.exclusive_category}</span>
                          </WithIcon>
                        </div>
                        <div className="body-m flex-row primary-text summary-line-non-link">
                          <WithIcon iconProps={{iconName: "IssueTracking"}}>
                            <span>Non-exclusive category {this.state.vertex.inclusive_category}</span>
                          </WithIcon>
                        </div>
                        <div className="body-m flex-row primary-text summary-line-non-link">
                          <WithIcon iconProps={{iconName: "LocaleLanguage"}}>
                            <span>Language {this.state.vertex.language}</span>
                          </WithIcon>
                        </div>
                      </>
                    : <>
                        <div className="shimmer shimmer-line" style={{ width: '13em', margin: '3px' }}>&nbsp;</div>
                        <div className="shimmer shimmer-line" style={{ width: '12em', margin: '3px' }}>&nbsp;</div>
                        <div className="shimmer shimmer-line" style={{ width: '14em', margin: '3px' }}>&nbsp;</div>
                      </>
                  }
                </div>
              </div>
            </Card>
            {this.state.vertex?.comparisons && (this.state.vertex.comparisons.length ? (
              <Card className="bolt-table-card flex-grow" contentProps={{ contentPadding: false }}>
                <Table<PlagiarismComparison>
                  //behaviors={[sortingBehavior]}
                  className="table-example"
                  columns={this.columns}
                  containerClassName="h-scroll-auto"
                  itemProvider={new ArrayItemProvider(this.state.vertex.comparisons)}
                  role="table"
                  tableBreakpoints={this.tableBreakpoints}
                  //onActivate={(event, row) => this.props.onActivate(row.data)}
                />
              </Card>
            ) : (
              <Card
                className="bolt-table-card flex-grow"
                contentProps={{ contentPadding: false }}
                titleProps={{ text: ' ' }}
              >
                <ZeroData
                  className="flex-grow vss-ZeroData-fullsize"
                  primaryText="No comparison reports"
                  secondaryText="Either the submission cannot be compiled by system, or no submission matches the exclusive category, non-exclusive category and language."
                  imageAltText="No items"
                  imagePath="https://cdn.vsassets.io/ext/ms.vss-code-web/tags-view-content/Content/no-results.YsM6nMXPytczbbtz.png"
                />
              </Card>
            ))}
          </div>
          <div className="flex-column flex-grow card-list" id="tab3-content" style={{ display: 'none' }}>
            {this.state.vertex?.files?.map((value, index, array) => {
              const collapsed = new ObservableValue<boolean>(false);
              return (
                <Card
                  titleProps={{ text: `#${index + 1}: ${value.path}` }}
                  contentProps={{ contentPadding: false }}
                  collapsible={true}
                  collapsed={collapsed}
                  onCollapseClick={() => collapsed.value = !collapsed.value}
                >
                  <MonacoEditor
                    language={this.state.vertex?.language}
                    theme="vs-light"
                    value={value.content}
                    height="560px"
                    options={{
                      selectOnLineNumbers: false,
                      readOnly: true,
                      automaticLayout: true,
                      scrollBeyondLastLine: false,
                      minimap: { enabled: false }
                    }}
                  />
                </Card>
              );
            })}
          </div>
        </div>
      </>
    );
  }
}
