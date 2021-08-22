import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import { initializeIcons } from '@fluentui/font-icons-mdl2';
import { BrowserRouter, Route } from 'react-router-dom';

import PsetList from './PsetList';

initializeIcons();

ReactDOM.render(
  <BrowserRouter>
    <App>
      <Route path="/" component={PsetList} />
    </App>
  </BrowserRouter>,
  document.getElementById('react-root')
);
