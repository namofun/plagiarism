import * as React from 'react';
import { withRouter } from 'react-router';
import { RouteComponentProps, NavLink, Route } from 'react-router-dom';
import { Action, Location } from 'history';

interface IAppState {
  input: string;
}

class App extends React.Component<RouteComponentProps, IAppState> {

  constructor(props: RouteComponentProps) {
    super(props);
    this.state = { input: '' };
    this.props.history.listen(this.onRouteChange);
  }

  public render() {
    return this.props.children;
  }

  private onRouteChange = (location: Location, action: Action) =>
    this.setState({ input: "" });
}

export default withRouter(App);
