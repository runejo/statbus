import React from 'react'
import { Dropdown, Flag } from 'semantic-ui-react'
import { IndexLink, Link } from 'react-router'

import { systemFunction as sF } from '../../helpers/checkPermissions'
import styles from './styles'

// eslint-disable-next-line no-underscore-dangle
const userName = window.__initialStateFromServer.userName || '(name not found)'

export default () => (
  <header className={styles.root}>
    <div className={`ui inverted menu ${styles['menu-root']}`}>
      <div className="ui right aligned container">
        <IndexLink to="/" className={`item ${styles['index-link']}`}>
          <img className="logo" alt="logo" src="logo.png" width="25" height="35" />
          <text>NSC Registry</text>
        </IndexLink>
        {sF('UserListView') && <Link to="/users" className="item">Users</Link>}
        {sF('RoleListView') && <Link to="/roles" className="item">Roles</Link>}
        {sF('StatUnitListView') && <Link to="/statunits" className="item">Stat Units</Link>}
        <div className="right menu">
          <Dropdown simple text="Language" className="item" icon="caret down">
            <Dropdown.Menu>
              <Dropdown.Item><Flag name='gb'/>English</Dropdown.Item>
              <Dropdown.Item><Flag name='ru'/>Русский</Dropdown.Item>
              <Dropdown.Item><Flag name='kg'/>Кыргызча</Dropdown.Item>
            </Dropdown.Menu>
          </Dropdown>
          <Dropdown simple text={userName} className="item" icon="caret down">
            <Dropdown.Menu>
              {sF('AccountView') && <Dropdown.Item
                as={() => <Link to="/account" className="item">Account</Link>}
              />}
              <Dropdown.Item
                as={() => <a href="/account/logout" className="item">Logout</a>}
              />
            </Dropdown.Menu>
          </Dropdown>
        </div>
      </div>
    </div>
  </header>
)
