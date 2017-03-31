import React from 'react'
import { Button, Item, Confirm } from 'semantic-ui-react'
import { Link } from 'react-router'
import R from 'ramda'

import { systemFunction as sF } from 'helpers/checkPermissions'
import Paginate from 'components/Paginate'
import { wrapper } from 'helpers/locale'
import SearchForm from '../SearchForm'
import ListItem from './ListItem'
import styles from './styles'

const { arrayOf, func, number, oneOfType, shape, string } = React.PropTypes
class Search extends React.Component {
  static propTypes = {
    actions: shape({
      updateFilter: func.isRequired,
      setQuery: func.isRequired,
      fetchData: func.isRequired,
      deleteStatUnit: func.isRequired,
    }).isRequired,
    formData: shape({}).isRequired,
    statUnits: arrayOf(shape({
      regId: number.isRequired,
      name: string.isRequired,
    })),
    query: shape({
      wildcard: string,
      includeLiquidated: string,
    }),
    totalCount: oneOfType([number, string]),
    localize: func.isRequired,
  }

  static defaultProps = {
    query: shape({
      wildcard: '',
      includeLiquidated: false,
    }),
    statUnits: [],
    totalCount: 0,
  }

  state = {
    showConfirm: false,
    selectedUnit: undefined,
  }

  componentDidMount() {
    this.props.actions.fetchData(this.props.query)
  }

  componentWillReceiveProps(nextProps) {
    if (!R.equals(nextProps.query, this.props.query)) {
      nextProps.actions.fetchData(nextProps.query)
    }
  }

  handleChangeForm = (name, value) => {
    this.props.actions.updateFilter({ [name]: value })
  }

  handleSubmitForm = (e) => {
    e.preventDefault()
    const { actions: { setQuery }, query, formData } = this.props
    setQuery({ ...query, ...formData })
  }

  handleConfirm = () => {
    const unit = this.state.selectedUnit
    this.setState({ selectedUnit: undefined, showConfirm: false })
    const { query, formData } = this.props
    const queryParams = { ...query, ...formData }
    this.props.actions.deleteStatUnit(unit.type, unit.regId, queryParams)
  }

  handleCancel = () => {
    this.setState({ showConfirm: false })
  }

  displayConfirm = (statUnit) => {
    this.setState({ selectedUnit: statUnit, showConfirm: true })
  }

  renderRow = item => (
    <ListItem
      key={`${item.regId}_${item.type}`}
      statUnit={item}
      deleteStatUnit={this.displayConfirm}
      localize={this.props.localize}
    />
  )

  renderConfirm = () => (
    <Confirm
      open={this.state.showConfirm}
      header={`${this.props.localize('AreYouSure')}?`}
      content={`${this.props.localize('DeleteStatUnitMessage')} "${this.state.selectedUnit.name}"?`}
      onConfirm={this.handleConfirm}
      onCancel={this.handleCancel}
    />
  )

  render() {
    const { statUnits, formData, localize, totalCount } = this.props
    return (
      <div className={styles.root}>
        <h2>{localize('SearchStatisticalUnits')}</h2>
        {this.state.showConfirm && this.renderConfirm()}
        {sF('StatUnitCreate')
          && <Button
            as={Link} to="/statunits/create"
            content={localize('CreateStatUnit')}
            icon="add square"
            size="medium"
            color="green"
            className={styles.add}
          />}
        <SearchForm
          formData={formData}
          onChange={this.handleChangeForm}
          onSubmit={this.handleSubmitForm}
        />
        <Paginate totalCount={Number(totalCount)}>
          <Item.Group divided className={styles.items}>
            {statUnits.map(this.renderRow)}
          </Item.Group>
        </Paginate>
      </div>
    )
  }
}

export default wrapper(Search)
