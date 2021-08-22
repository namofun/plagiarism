import * as React from 'react';
import { Page } from 'azure-devops-ui/Page';
import { Header, TitleSize } from 'azure-devops-ui/Header';
import { IHeaderCommandBarItem } from 'azure-devops-ui/HeaderCommandBar';
import { Surface, SurfaceBackground } from 'azure-devops-ui/Surface';
import { PlagiarismSetModel, PlagiarismSetTable } from './PsetEntities';
import { Card } from "azure-devops-ui/Card";

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

interface PsetListProps {

}

interface PsetListState {
  
}

class PsetList extends React.Component<PsetListProps, PsetListState> {
  private loadController : AbortController;

  constructor(props : PsetListProps) {
    super(props);

    this.loadController = new AbortController();
    this.state = {};
  }

  public componentWillUnmount() {
    this.loadController.abort();
  }

  public componentDidMount() {

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
              <PlagiarismSetTable />
            </Card>
          </div>
        </Page>
      </Surface>
    );
  }
}

export default PsetList;
