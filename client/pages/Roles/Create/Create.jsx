import React from 'react'
import { Link } from 'react-router'
import { Button, Form, Icon, Loader } from 'semantic-ui-react'

import DataAccess from 'components/DataAccess'
import rqst from 'helpers/request'
import { wrapper } from 'helpers/locale'
import styles from './styles'

const { func } = React.PropTypes

class CreateForm extends React.Component {

  static propTypes = {
    localize: func.isRequired,
    submitRole: func.isRequired,
  }

  state = {
    data: {
      name: '',
      description: '',
      accessToSystemFunctions: [],
      standardDataAccess: [],
    },
    standardDataAccess: {
      localUnit: [],
      legalUnit: [],
      enterpriseGroup: [],
      enterpriseUnit: [],
    },
    systemFunctions: [],
    fetchingStandardDataAccess: true,
    fetchingSystemFunctions: true,
    standardDataAccessMessage: undefined,
    systemFunctionsFailMessage: undefined,
  }

  componentDidMount() {
    this.fetchStandardDataAccess()
    this.fetchSystemFunctions()
  }

  fetchStandardDataAccess() {
    rqst({
      url: '/api/accessAttributes/dataAttributes',
      onSuccess: (result) => {
        this.setState(({
          standardDataAccess: result,
          fetchingStandardDataAccess: false,
        }))
      },
      onFail: () => {
        this.setState(({
          standardDataAccessMessage: 'failed loading standard data access',
          fetchingStandardDataAccess: false,
        }))
      },
      onError: () => {
        this.setState(({
          standardDataAccessFailMessage: 'error while fetching standard data access',
          fetchingStandardDataAccess: false,
        }))
      },
    })
  }

  fetchSystemFunctions() {
    rqst({
      url: '/api/accessAttributes/systemFunctions',
      onSuccess: (result) => {
        this.setState(({
          systemFunctions: result,
          fetchingSystemFunctions: false,
        }))
      },
      onFail: () => {
        this.setState(({
          systemFunctionsFailMessage: 'failed loading system functions',
          fetchingSystemFunctions: false,
        }))
      },
      onError: () => {
        this.setState(({
          systemFunctionsFailMessage: 'error while fetching system functions',
          fetchingSystemFunctions: false,
        }))
      },
    })
  }

  handleEdit = (e, { name, value }) => {
    this.setState(s => ({ data: { ...s.data, [name]: value } }))
  }

  handleSubmit = (e) => {
    e.preventDefault()
    this.props.submitRole(this.state.data)
  }

  handleDataAccessChange = (data) => {
    this.setState((s) => {
      const item = s.standardDataAccess[data.type].find(x => x.name == data.name)
      const items = s.standardDataAccess[data.type].filter(x => x.name != data.name)
      return ({
        standardDataAccess: { ...s.standardDataAccess, [data.type]: [...items, { ...item, allowed: !item.allowed }] }
      })
    })
  }

  render() {
    const { localize } = this.props
    const {
      data,
      fetchingStandardDataAccess, standardDataAccess,
      fetchingSystemFunctions, systemFunctions,
    } = this.state

    return (
      <div className={styles.rolecreate}>
        <Form className={styles.form} onSubmit={this.handleSubmit}>
          <h2>{localize('CreateNewRole')}</h2>
          <Form.Input
            name="name"
            onChange={this.handleEdit}
            value={data.name}
            label={localize('RoleName')}
            placeholder={localize('WebSiteVisitor')}
            required
          />
          <Form.Input
            name="description"
            onChange={this.handleEdit}
            value={data.description}
            label={localize('Description')}
            placeholder={localize('OrdinaryWebsiteUser')}
            required
          />
          {fetchingStandardDataAccess
            ? <Loader content="fetching standard data access" />
            : <DataAccess
              dataAccess={standardDataAccess}
              label={localize('DataAccess')}
              onChange={this.handleDataAccessChange}
            />}
          {fetchingSystemFunctions
            ? <Loader content="fetching system functions" />
            : <Form.Select
              name="accessToSystemFunctions"
              onChange={this.handleEdit}
              value={data.accessToSystemFunctions}
              options={systemFunctions.map(r => ({ value: r.key, text: localize(r.value) }))}
              label={localize('AccessToSystemFunctions')}
              placeholder={localize('SelectOrSearchSystemFunctions')}
              required
              multiple
              search
            />}
          <Button
            as={Link} to="/roles"
            content={localize('Back')}
            icon={<Icon size="large" name="chevron left" />}
            size="small"
            color="gray"
            type="button"
          />
          <Button className={styles.sybbtn} type="submit" primary>
            {localize('Submit')}
          </Button>
        </Form>
      </div>
    )
  }
}

export default wrapper(CreateForm)
