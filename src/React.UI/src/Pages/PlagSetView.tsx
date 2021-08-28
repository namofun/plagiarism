import { Card, CustomHeader, HeaderDescription, HeaderIcon, HeaderTitle, HeaderTitleArea, HeaderTitleRow, IHeaderCommandBarItem, IReadonlyObservableValue, ObservableArray, ObservableValue, Page, React, RouteComponentProps, Status, StatusSize, Surface, SurfaceBackground, TitleSize } from "../AzureDevOpsUI";
import { PlagSetInfoCard } from "../Views/PlagSetInfoCard";
import { PlagSetSubmitList } from "../Views/PlagSetSubmitList";
import { PlagiarismSet as PlagSetModel } from "../Models/PlagiarismSet";
import { PlagiarismSubmission as PlagSubmitModel } from "../Models/PlagiarismSubmission";
import { Helpers } from "../Components/Helpers";

interface PlagSetViewProps {
  match: {
    params: {
      id: string;
    }
  };
}

interface PlagSetViewState {
  loading: boolean;
  formal_name: string;
  setid: string;
  model?: PlagSetModel;
  submissions?: PlagSubmitModel[];
}

class PlagSetView extends React.Component<PlagSetViewProps & RouteComponentProps, PlagSetViewState> {

  private loadController = new AbortController();

  constructor(props: PlagSetViewProps & RouteComponentProps) {
    super(props);

    this.state = {
      loading: false,
      formal_name: 'Loading...',
      setid: props.match.params.id
    };
  }

  private commandBarItems : IHeaderCommandBarItem[] = [
    {
      iconProps: { iconName: "Upload" },
      id: "upload-button",
      isPrimary: true,
      text: "Upload",
      onActivate: () => {}
    }
  ];

  public componentDidMount() {
    fetch('/api/plagiarism/sets/' + encodeURIComponent(this.props.match.params.id), {
      method: 'GET',
      signal: this.loadController.signal
    }).then(resp => {
      return resp.ok ? resp.json() : null;
    }).then(json => {
      const set = json as PlagSetModel;
      fetch('/api/plagiarism/sets/' + encodeURIComponent(set.setid) + '/submissions', {
        method: 'GET',
        signal: this.loadController.signal
      }).then(resp => {
        return resp.ok ? resp.json() : null;
      }).then(json => {
        const submits = json as PlagSubmitModel[];
        this.setState({
          ...this.state,
          model: set,
          setid: set.setid,
          formal_name: set.formal_name,
          submissions: submits
        });
      });
    });
  }

  public componentWillUnmount() {
    this.loadController.abort();
  }

  public render() {
    const arr = new ObservableArray<PlagSubmitModel | IReadonlyObservableValue<PlagSubmitModel | undefined>>();
    if (this.state.submissions !== null && this.state.submissions !== undefined) arr.push(...this.state.submissions);
    else for (let i = 0; i < 5; i++) arr.push(new ObservableValue<PlagSubmitModel | undefined>(undefined));
    return (
      <Surface background={SurfaceBackground.neutral}>
        <Page>
          <CustomHeader className="bolt-header-with-commandbar">
            <HeaderIcon
                className="bolt-table-status-icon-large"
                iconProps={{ render: className => <Status {...Helpers.getStatusIndicatorData(this.state.model)} className={className} size={StatusSize.l} /> }}
                titleSize={TitleSize.Large} />
            <HeaderTitleArea>
              <HeaderTitleRow>
                <HeaderTitle ariaLevel={3} className="text-ellipsis" titleSize={TitleSize.Large}>
                  Plagiarism Set: {this.state.formal_name}
                </HeaderTitle>
              </HeaderTitleRow>
              <HeaderDescription>
                #{this.state.setid}
              </HeaderDescription>
            </HeaderTitleArea>
          </CustomHeader>
          <div className="page-content page-content-top">
            <PlagSetInfoCard model={this.state.model} />
            <Card className="margin-top-16 flex-grow bolt-table-card"
                titleProps={{ text: 'Submissions' }}
                contentProps={{ contentPadding: false }}
                headerCommandBarItems={this.commandBarItems}>
              <PlagSetSubmitList observableArray={arr} />
            </Card>
          </div>
        </Page>
      </Surface>
    );
  }
}

export default PlagSetView;
