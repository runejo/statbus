import React from 'react'
import { bindActionCreators } from 'redux'
import { connect } from 'react-redux'
import { createSelector } from 'reselect'
import { pipe } from 'ramda'

import createSchemaFormHoc from 'components/createSchemaFormHoc/'
import FormBody from 'components/StatUnitFormBody'
import withSpinnerUnless from 'components/withSpinnerUnless'
import createSchema from 'helpers/createStatUnitSchema'
import { getText } from 'helpers/locale'
import {
  createFieldsMeta,
  createModel,
  createValues,
  updateProperties,
} from 'helpers/modelProperties'
import { getDate, toUtc } from 'helpers/dateHelper'
import { actionCreators } from './actions'

const getSchema = props => props.schema
const mapPropsToValues = props => createValues(props.updatedProperties)

const createMapStateToProps = () =>
  createSelector(
    [
      state => state.createStatUnit.permissions,
      state => state.createStatUnit.properties,
      state => state.locale,
      (_, props) => props.type,
    ],
    (permissions, properties, locale, type) => {
      if (properties === undefined || permissions === undefined) {
        return { spinner: true }
      }
      const schema = createSchema(type, permissions, properties, null)
      const updatedProperties = updateProperties(
        schema.cast(createModel(permissions, properties)),
        properties,
      )
      return {
        schema,
        permissions,
        updatedProperties,
        fieldsMeta: createFieldsMeta(type, updatedProperties),
        localize: getText(locale),
        locale,
      }
    },
  )

const mapDispatchToProps = (dispatch, { type }) =>
  bindActionCreators(
    {
      onSubmit: (statUnit, formActions) =>
        actionCreators.submitStatUnit(type, statUnit, formActions),
      onCancel: actionCreators.navigateBack,
    },
    dispatch,
  )

const assert = props => !props.spinner

// console.log('getSchema', getSchema)
// console.log('mapPropsToValues', mapPropsToValues)

const enhance = pipe(
  createSchemaFormHoc(getSchema, mapPropsToValues),
  withSpinnerUnless(assert),
  connect(
    createMapStateToProps,
    mapDispatchToProps,
  ),
)

export default enhance((props) => {
  const { values } = props
  const currentDate = toUtc(getDate().toDate())
  if (values.taxRegId) {
    values.taxRegDate = values.taxRegDate || currentDate
  }
  if (values.externalId) {
    values.externalIdDate = values.externalIdDate || currentDate
  }
  if (values.entGroupId) {
    values.entGroupIdDate = values.entGroupIdDate || currentDate
  }
  if (values.legalUnitId) {
    values.legalUnitIdDate = values.legalUnitIdDate || currentDate
  }
  return <FormBody {...{ ...props }} />
})
