import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import 'azure-devops-ui/Core/override.css';
import App from './App';
import { initializeIcons } from '@fluentui/font-icons-mdl2';
import { BrowserRouter, Route } from 'react-router-dom';

import PlagSetList from './Pages/PlagSetList';
import PlagSetView from './Pages/PlagSetView';

initializeIcons();

ReactDOM.render(
  <BrowserRouter>
    <App>
      <Route exact path="/" component={PlagSetList} />
      <Route exact path="/:id" component={PlagSetView} />
    </App>
  </BrowserRouter>,
  document.getElementById('react-root')
);
