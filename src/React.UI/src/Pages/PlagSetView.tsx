import { Card, CustomHeader, HeaderDescription, HeaderIcon, HeaderTitle, HeaderTitleArea, HeaderTitleRow, Page, React, RouteComponentProps, Status, Statuses, StatusSize, Surface, SurfaceBackground, TitleSize } from "../AzureDevOpsUI";
import { PlagSetInfoCard } from "../Views/PlagSetInfoCard";
import { PlagiarismSet as PlagSetModel } from "../Models/PlagiarismSet";

//import { Header, HeaderBackButton } from 'azure-devops-ui/Header';
//import { HeaderCommandBar, IHeaderCommandBarItem } from 'azure-devops-ui/HeaderCommandBar';

interface PlagSetViewProps {
  match: {
    params: {
      id: string;
    }
  };
}

interface PlagSetViewState {
  loading: boolean;
  model?: PlagSetModel;
}

const stats = [
  {
      label: "Points",
      value: 340
  },
  {
      label: "3PM",
      value: 23
  },
  {
      label: "Rebounds",
      value: 203
  },
  {
      label: "Assists",
      value: 290
  },
  {
      label: "Steals",
      value: 56
  }
];

class PlagSetView extends React.Component<PlagSetViewProps & RouteComponentProps, PlagSetViewState> {

  constructor(props: PlagSetViewProps & RouteComponentProps) {
    super(props);

    this.state = {
      loading: false,
      model: {
        "setid": "20a1b2d0-b9ed-7a46-131a-39facbea4d21",
        "create_time": "2021-02-19T18:02:17.2490617+08:00",
        "creator": null,
        "related": null,
        "formal_name": "2020级吉林大学XCPC集训队选拔赛 - 第一轮",
        "report_count": 34,
        "report_pending": 0,
        "submission_count": 20,
        "submission_failed": 6,
        "submission_succeeded": 14
      }
    };
  }

  private renderStatus = (className?: string) => {
      return <Status {...Statuses.Success} className={className} size={StatusSize.l} />;
  };

  public render() {
    return (
      <Surface background={SurfaceBackground.neutral}>
        <Page>
          <CustomHeader className="bolt-header-with-commandbar">
            <HeaderIcon
                className="bolt-table-status-icon-large"
                iconProps={{ render: this.renderStatus }}
                titleSize={TitleSize.Large} />
            <HeaderTitleArea>
              <HeaderTitleRow>
                <HeaderTitle ariaLevel={3} className="text-ellipsis" titleSize={TitleSize.Large}>
                  Plagiarism Set: {this.state.model?.formal_name}
                </HeaderTitle>
              </HeaderTitleRow>
              <HeaderDescription>
                #{this.state.model?.setid}
              </HeaderDescription>
            </HeaderTitleArea>
          </CustomHeader>
          <div className="page-content page-content-top">
            {this.state.model &&
              <PlagSetInfoCard model={this.state.model} />
            }
            <Card className="margin-top-16"
                titleProps={{ text: 'Submissions' }}
                headerCommandBarItems={[]}>
              Page content
            </Card>
          </div>
        </Page>
      </Surface>
    );
  }
}

export default PlagSetView;
