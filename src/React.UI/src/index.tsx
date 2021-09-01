import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import './Components/light-theme.css';
import 'azure-devops-ui/Core/override.css';
import App from './App';
import { initializeIcons } from '@fluentui/font-icons-mdl2';
import { BrowserRouter, Redirect, Route, Switch } from 'react-router-dom';

import PlagSetList from './Pages/PlagSetList';
import PlagSetView from './Pages/PlagSetView';
import PlagSubmitView from './Pages/PlagSubmitView';
import NotFound from './Pages/NotFound';

initializeIcons();

ReactDOM.render(
  <BrowserRouter>
    <App>
      <Switch>
        <Route exact path="/" component={PlagSetList} />
        <Route exact path="/:id" component={PlagSetView} />
        <Route exact path="/:id/submissions/:sid" component={PlagSubmitView} />
        <Route component={NotFound} />
      </Switch>
    </App>
  </BrowserRouter>,
  document.getElementById('react-root')
);
