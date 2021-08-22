import * as React from 'react';
import { withRouter } from 'react-router';
import { RouteComponentProps, NavLink, Route } from 'react-router-dom';
import { Action, Location } from 'history';

import logo from './logo.svg';
import './App.css';

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
    return (
      <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo" />
          <p>
            Edit <code>src/App.js</code> and save to reload.
          </p>
          <a
            className="App-link"
            href="https://reactjs.org"
            target="_blank"
            rel="noopener noreferrer"
          >
            Learn React
          </a>
        </header>
      </div>
    );
  }

  private onRouteChange = (location: Location, action: Action) =>
    this.setState({ input: "" });
}

export default withRouter(App);
