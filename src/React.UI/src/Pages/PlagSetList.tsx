import { Card, Dialog, Header, IHeaderCommandBarItem, IReadonlyObservableValue, ObservableArray, ObservableValue, Page, React, RouteComponentProps, Spinner, SpinnerOrientation, TitleSize, ZeroData, ZeroDataActionType } from "../AzureDevOpsUI";
import { PlagiarismSet as PlagSetModel } from '../Models/PlagiarismSet';
import { PlagSetTable } from "../Views/PlagSetTable";
import { PlagSetCreatePanel } from "../Views/PlagSetCreatePanel";
import Q from 'q';

interface PlagSetListProps {
  creator?: number;
  related?: number;
}

interface PlagSetListState {
  loading: boolean;
  busy: boolean;
  creating: boolean;
  rescuing: boolean;
  canCreate: boolean;
}

class PlagSetList extends React.Component<RouteComponentProps & PlagSetListProps, PlagSetListState> {

  private loadController : AbortController;
  private newPsetName = new ObservableValue<string | undefined>("");
  private newPsetDescription = new ObservableValue<string | undefined>("");
  private observableArray : ObservableArray<PlagSetModel | IReadonlyObservableValue<PlagSetModel | undefined>>;
  private commandBarItems : IHeaderCommandBarItem[] = [
    {
      iconProps: { iconName: "Add" },
      id: "test1",
      isPrimary: true,
      text: "Create",
      onActivate: () => this.openCreatePanel()
    },
    {
      iconProps: { iconName: "Refresh" },
      id: "test2",
      text: "Rescue stopped service",
      important: false,
      onActivate: () => {
        if (!this.state.busy) {
          this.setState({ ...this.state, rescuing: true });
        }
      }
    }
  ];

  private openCreatePanel() {
    if (!this.state.busy) {
      this.newPsetName.value = "";
      this.newPsetDescription.value = "";
      this.setState({ ...this.state, creating: true });
    }
  }

  constructor(props: RouteComponentProps & PlagSetListProps) {
    super(props);

    this.loadController = new AbortController();
    this.state = { loading: true, busy: false, creating: false, rescuing: false, canCreate: false };

    this.observableArray = new ObservableArray<PlagSetModel | IReadonlyObservableValue<PlagSetModel | undefined>>(
      new Array(5).fill(new ObservableValue<PlagSetModel | undefined>(undefined))
    );

    this.newPsetName.subscribe(value => {
      let shouldCanCreate = value !== '';
      if (this.state.canCreate !== shouldCanCreate) {
        this.setState({ ...this.state, canCreate: shouldCanCreate });
      }
    });
  }

  public componentWillUnmount() {
    this.loadController.abort();
  }

  public componentDidMount() {
    fetch('/api/plagiarism/sets', {
      method: 'GET',
      signal: this.loadController.signal
    }).then(resp => {
      return resp.ok ? resp.json() : null;
    }).then(json => {
      const val = json as PlagSetModel[];
      this.observableArray.splice(0, 5);
      this.observableArray.push(...val);
      this.setState({ ...this.state, loading: false });
    });
  }

  private closeRescueDialog() {
    if (!this.state.busy) {
      this.setState({ ...this.state, rescuing: false });
    }
  }

  private sendRescueRequest() {
    this.setState({ ...this.state, busy: true });
    fetch('/api/plagiarism/rescue', {
      method: 'POST',
      signal: this.loadController.signal
    }).then(resp => {
      return resp.ok ? Q.delay(1000) : null;
    }).then(_ => {
      this.setState({ ...this.state, busy: false, rescuing: false });
    });
  }

  private closeCreatePanel() {
    if (!this.state.busy) {
      this.setState({ ...this.state, creating: false });
    }
  }

  private sendCreateRequest() {
    this.setState({ ...this.state, busy: true });
    fetch('/api/plagiarism/sets', {
      method: 'POST',
      body: JSON.stringify({ formal_name: this.newPsetName.value }),
      signal: this.loadController.signal,
      headers: (() => {
        let header = new Headers();
        header.append('Content-Type', 'application/json');
        return header;
      })(),
    }).then(resp => {
      return resp.ok ? resp.json() : null;
    }).then(json => {
      const newPset = json as PlagSetModel;
      this.setState({ ...this.state, busy: false, creating: false });
      this.props.history.push('/' + newPset.setid);
    });
  }

  public render() {
    return (
      <>
        <>
          <Header
              title="Plagiarism Set"
              commandBarItems={this.observableArray.length > 0 ? this.commandBarItems : [this.commandBarItems[1]]}
              titleSize={TitleSize.Large}
          />
          {this.observableArray.length > 0 ? (
          <div className="page-content page-content-top">
            <Card className="flex-grow bolt-table-card" contentProps={{ contentPadding: false }}>
              <PlagSetTable
                  itemProvider={this.observableArray}
                  itemClick={(model => this.props.history.push('/' + model.setid))}
                  canSort={!this.state.loading}
              />
            </Card>
          </div>
          ) : (
          <div className="page-content page-content-top flex-grow flex-column">
            <div className="flex-grow" />
            <div className="flex-cell flex-grow">
              <ZeroData
                className="flex-grow vss-ZeroData-fullsize"
                primaryText="Create your first plagiarism set"
                secondaryText="Use plagiarism set to detect code similarity and manage homework integrity."
                imageAltText="No items"
                imagePath="https://cdn.vsassets.io/ext/ms.vss-environments-web/environments-landing/ZeroData.pKQ45bYFZi6gXJbV.svg"
                actionText="Create plagiarism set"
                actionType={ZeroDataActionType.ctaButton}
                onActionClick={(event, item) => this.openCreatePanel()}
              />
            </div>
            <div className="flex-grow" style={{ flexGrow: 2.5 }} />
          </div>
          )}
        </>
        {this.state.rescuing &&
          <Dialog
              titleProps={{ text: "Plagiarism service" }}
              onDismiss={() => this.closeRescueDialog()}
              footerButtonProps={[
                { text: "Cancel", disabled: this.state.busy, onClick: () => this.closeRescueDialog() },
                { text: "Continue", disabled: this.state.busy, onClick: () => this.sendRescueRequest(), danger: true }
              ]}>
            {this.state.busy
              ? <div style={{ margin: '0 auto 0 0' }}><Spinner label="Trying to rescue the service..." orientation={SpinnerOrientation.row} /></div>
              : <>If the service isn't running, use this function to rescue.</>}
          </Dialog>
        }
        {this.state.creating &&
          <PlagSetCreatePanel
              closeCreatePanel={() => this.closeCreatePanel()}
              busy={this.state.busy}
              valid={this.state.canCreate}
              sendCreateRequest={() => this.sendCreateRequest()}
              newPsetName={this.newPsetName}
              newPsetDescription={this.newPsetDescription}
          />
        }
      </>
    );
  }
}

export default PlagSetList;
