import React from 'react'
import PropTypes from 'prop-types'
import { Checkbox, Table, List, Label } from 'semantic-ui-react'

import ListWithDnd from 'components/ListWithDnd'
import { sampleFrameFields } from 'helpers/enums'

const listStyle = { display: 'inline-block' }
const floatRight = { float: 'right' }
const fields = [...sampleFrameFields]

const FieldsEditor = ({ value: selected, onChange, localize }) => {
  const onAdd = (_, { id }) => onChange([...selected, id])
  const onRemove = (_, { id }) => onChange(selected.filter(y => y !== id))
  const isAllSelected = selected.length === fields.length
  const selectAllItems = () => onChange(isAllSelected ? [] : fields.map(([key]) => key))

  const allItems = fields.map(([key, value]) => {
    const checked = selected.includes(key)
    const props = { id: key, checked, label: localize(value), onClick: checked ? onRemove : onAdd }
    return { key, content: <Checkbox {...props} /> }
  })
  return (
    <Table basic="very" celled>
      <Table.Header>
        <Table.Row>
          <Table.HeaderCell width={8} textAlign="left">
            <Checkbox
              id="selectAllFields"
              checked={isAllSelected}
              label={localize('SelectAll')}
              onClick={selectAllItems}
            />
            <span style={floatRight}>{localize('FieldsToSelect')}</span>
          </Table.HeaderCell>
          <Table.HeaderCell content={localize('SelectedFields')} width={8} textAlign="center" />
        </Table.Row>
      </Table.Header>
      <Table.Body>
        <Table.Row verticalAlign="top">
          <Table.Cell textAlign="left" width={8}>
            <List items={allItems} className="left aligned" style={listStyle} />
          </Table.Cell>
          <Table.Cell textAlign="center" width={8}>
            <ListWithDnd
              value={selected}
              onChange={onChange}
              renderItem={key => (
                <Label
                  id={key}
                  content={localize(sampleFrameFields.get(key))}
                  onRemove={onRemove}
                  size="large"
                />
              )}
            />
          </Table.Cell>
        </Table.Row>
      </Table.Body>
    </Table>
  )
}

const { arrayOf, func, number } = PropTypes
FieldsEditor.propTypes = {
  value: arrayOf(number).isRequired,
  onChange: func.isRequired,
  localize: func.isRequired,
}

export default FieldsEditor
