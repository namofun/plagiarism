import * as React from 'react';
import { Page } from 'azure-devops-ui/Page';
import { Header, TitleSize } from 'azure-devops-ui/Header';
import { IHeaderCommandBarItem } from 'azure-devops-ui/HeaderCommandBar';
import { Surface, SurfaceBackground } from 'azure-devops-ui/Surface';
import { PlagSetModel, PlagSetTable, PlagSetListProps } from './PlagSetEntities';
import { Card } from "azure-devops-ui/Card";
import { IReadonlyObservableValue, ObservableArray, ObservableValue } from 'azure-devops-ui/Core/Observable';

const commandBarItems: IHeaderCommandBarItem[] = [
  {
    iconProps: { iconName: "Add" },
    id: "test1",
    isPrimary: true,
    text: "Create"
  },
  {
    iconProps: { iconName: "Refresh" },
    id: "test2",
    text: "Rescue stopped service",
    important: false
  }
];

interface PlagSetListState {
  loading: boolean;
}

class PlagSetList extends React.Component<PlagSetListProps, PlagSetListState> {

  private loadController : AbortController;
  private observableArray : ObservableArray<PlagSetModel | IReadonlyObservableValue<PlagSetModel | undefined>>;

  constructor(props : PlagSetListProps) {
    super(props);

    this.loadController = new AbortController();
    this.state = { loading: true };

    this.observableArray = new ObservableArray<PlagSetModel | IReadonlyObservableValue<PlagSetModel | undefined>>(
      new Array(5).fill(new ObservableValue<PlagSetModel | undefined>(undefined))
    );
  }

  public componentWillUnmount() {
    this.loadController.abort();
  }

  public async componentDidMount() {
    var val = await fetch('/api/plagiarism/sets')
      .then(resp => resp.ok ? resp.json() : null)
      .then(json => json as PlagSetModel[]);

    this.observableArray.splice(0, 5);
    this.observableArray.push(...val);
    this.setState({ loading: false });
  }

  public render() {
    return (
      <Surface background={SurfaceBackground.neutral}>
        <Page>
          <Header
              title="Plagiarism Set"
              commandBarItems={commandBarItems}
              titleSize={TitleSize.Large}
          />
          <div className="page-content page-content-top">
            <Card className="flex-grow bolt-table-card" contentProps={{ contentPadding: false }}>
              <PlagSetTable itemProvider={this.observableArray} />
            </Card>
          </div>
        </Page>
      </Surface>
    );
  }
}

export default PlagSetList;
