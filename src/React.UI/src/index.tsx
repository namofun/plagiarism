import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import { initializeIcons } from '@fluentui/font-icons-mdl2';
import { BrowserRouter, Route } from 'react-router-dom';

import PlagSetList from './PlagSetList';
import PlagSetView from './PlagSetView';

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
