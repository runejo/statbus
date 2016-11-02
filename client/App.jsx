import React from 'react'
import { Provider } from 'react-redux'
import { Router, browserHistory, IndexRoute, IndexLink, Link, Route } from 'react-router'
import { syncHistoryWithStore } from 'react-router-redux'

import Home from './pages/Home'
import RolesRoutes from './pages/Roles'
import UsersRoutes from './pages/Users'
import About from './pages/About'
import NotFound from './pages/NotFound'

// Layout component
const Layout = props => (
  <div>
    <IndexLink to="/">Home</IndexLink>
    {' | '}
    <Link to="/users">Users</Link>
    {' | '}
    <Link to="/roles">Roles</Link>
    {' | '}
    <Link to="/about">About</Link>
    <br />
    {props.children}
  </div>
)

// Router component
const Routes = (
  <Route path="/" component={Layout}>
    <IndexRoute component={Home} />
    {RolesRoutes}
    {UsersRoutes}
    <Route path="about" component={About} />
    <Route path="*" component={NotFound} />
  </Route>
)

// App component
export default ({ store }) => (
  <Provider store={store}>
    <Router
      key={Math.random()}
      history={syncHistoryWithStore(browserHistory, store)}
      routes={Routes}
    />
  </Provider>
)
