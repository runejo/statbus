import React from 'react'
import { Select } from 'semantic-ui-react'
import { shape, func, number } from 'prop-types'

import { statUnitTypes } from 'helpers/enums'
import { stripNullableFields } from 'helpers/schema'
import ConnectedForm from './ConnectedForm'
import styles from './styles.pcss'

// TODO: should be configurable
const stripStatUnitFields = stripNullableFields([
  'enterpriseUnitRegId',
  'enterpriseGroupRegId',
  'foreignParticipationCountryId',
  'legalUnitId',
  'entGroupId',
])

export default class CreateStatUnitPage extends React.Component {

  static propTypes = {
    type: number.isRequired,
    actions: shape({
      changeType: func.isRequired,
      submitStatUnit: func.isRequired,
      fetchModel: func.isRequired,
    }).isRequired,
    localize: func.isRequired,
  }

  componentDidMount() {
    this.props.actions.fetchModel(this.props.type)
  }

  componentWillReceiveProps(newProps) {
    const { actions: { fetchModel }, type } = this.props
    const { type: newType } = newProps
    if (newType !== type) fetchModel(newType)
  }

  handleTypeEdit = (_, { value }) => {
    if (this.props.type !== value) this.props.actions.changeType(value)
  }

  handleSubmit = (statUnit) => {
    const { type, actions: { submitStatUnit } } = this.props
    const processedStatUnit = stripStatUnitFields(statUnit)
    const data = { ...processedStatUnit, type }
    submitStatUnit(data)
  }

  render() {
    const { type, localize } = this.props

    const statUnitTypeOptions =
      [...statUnitTypes].map(([key, value]) => ({ value: key, text: localize(value) }))

    return (
      <div className={styles.edit}>
        <Select
          name="type"
          options={statUnitTypeOptions}
          value={type}
          onChange={this.handleTypeEdit}
        />
        <br />
        <ConnectedForm onSubmit={this.handleSubmit} />
      </div>
    )
  }
}
