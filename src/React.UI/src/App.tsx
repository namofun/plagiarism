import * as React from 'react';
import { withRouter } from 'react-router';
import { RouteComponentProps, NavLink, Route } from 'react-router-dom';
import { Action, Location } from 'history';
import { Page, Surface, SurfaceBackground } from './AzureDevOpsUI';
import { ProjectHeader } from './Components/ProjectHeader';

interface IAppState {
  input: string;
}

class App extends React.Component<RouteComponentProps, IAppState> {

  private scrollRef : React.RefObject<HTMLDivElement>;

  constructor(props: RouteComponentProps) {
    super(props);
    this.scrollRef = React.createRef<HTMLDivElement>();
    this.state = { input: '' };
    this.props.history.listen(this.onRouteChange);
  }

  public render() {
    return (
      <div className="full-size flex-column">
        <div className="flex-column">
          <ProjectHeader {...this.props} />
        </div>
        <div className="flex-row flex-grow v-scroll-auto">
          <Surface background={SurfaceBackground.neutral}>
            <Page className="flex-grow custom-scrollbar scroll-auto-hide" scrollableContainerRef={this.scrollRef}>
              {this.props.children}
            </Page>
          </Surface>
        </div>
      </div>
    );
  }

  private onRouteChange = (location: Location, action: Action) =>
    this.setState({ input: "" });
}

export default withRouter(App);
