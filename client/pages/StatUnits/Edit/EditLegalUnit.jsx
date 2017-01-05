import React from 'react'
import { Form } from 'semantic-ui-react'

import { dataAccessAttribute as check } from 'helpers/checkPermissions'
import { wrapper } from 'helpers/locale'
import DatePicker from './DatePicker'

const EditLocalUnit = ({ statUnit, handleEdit, handleDateEdit, localize }) => (<div>
  {check('enterpriseRegId') && <Form.Input
    value={statUnit.enterpriseRegId}
    onChange={handleEdit('enterpriseRegId')}
    name="enterpriseRegId"
    label={localize('EnterpriseRegId')}
  />}
  {check('entRegIdDate') &&
  <DatePicker
    value={statUnit.entRegIdDate}
    label={localize('EntRegIdDate')}
    handleDateEdit={handleDateEdit('entRegIdDate')}
  />}
  {check('founders') && <Form.Input
    value={statUnit.founders}
    onChange={handleEdit('founders')}
    name="founders"
    label={localize('Founders')}
  />}
  {check('owner') && <Form.Input
    value={statUnit.owner}
    onChange={handleEdit('owner')}
    name="owner"
    label={localize('Owner')}
  />}
  {check('market') && <Form.Input
    value={statUnit.market}
    onChange={handleEdit('market')}
    name="market"
    label={localize('Market')}
  />}
  {check('legalForm') && <Form.Input
    value={statUnit.legalForm}
    onChange={handleEdit('legalForm')}
    name="legalForm"
    label={localize('LegalForm')}
  />}
  {check('instSectorCode') && <Form.Input
    value={statUnit.instSectorCode}
    onChange={handleEdit('instSectorCode')}
    name="instSectorCode"
    label={localize('InstSectorCode')}
  />}
  {check('totalCapital') && <Form.Input
    value={statUnit.totalCapital}
    onChange={handleEdit('totalCapital')}
    name="totalCapital"
    label={localize('TotalCapital')}
  />}
  {check('munCapitalShare') && <Form.Input
    value={statUnit.munCapitalShare}
    onChange={handleEdit('munCapitalShare')}
    name="munCapitalShare"
    label={localize('MunCapitalShare')}
  />}
  {check('stateCapitalShare') && <Form.Input
    value={statUnit.stateCapitalShare}
    onChange={handleEdit('stateCapitalShare')}
    name="stateCapitalShare"
    label={localize('StateCapitalShare')}
  />}
  {check('privCapitalShare') && <Form.Input
    value={statUnit.privCapitalShare}
    onChange={handleEdit('privCapitalShare')}
    name="privCapitalShare"
    label={localize('PrivCapitalShare')}
  />}
  {check('foreignCapitalShare') && <Form.Input
    value={statUnit.foreignCapitalShare}
    onChange={handleEdit('foreignCapitalShare')}
    name="foreignCapitalShare"
    label={localize('ForeignCapitalShare')}
  />}
  {check('foreignCapitalCurrency') && <Form.Input
    value={statUnit.foreignCapitalCurrency}
    onChange={handleEdit('foreignCapitalCurrency')}
    name="foreignCapitalCurrency"
    label={localize('ForeignCapitalCurrency')}
  />}
  {check('actualMainActivity1') && <Form.Input
    value={statUnit.actualMainActivity1}
    onChange={handleEdit('actualMainActivity1')}
    name="actualMainActivity1"
    label={localize('ActualMainActivity1')}
  />}
  {check('actualMainActivity2') && <Form.Input
    value={statUnit.actualMainActivity2}
    onChange={handleEdit('actualMainActivity2')}
    name="actualMainActivity2"
    label={localize('ActualMainActivity2')}
  />}
  {check('actualMainActivityDate') &&
  <DatePicker
    value={statUnit.actualMainActivityDate}
    label={localize('ActualMainActivityDate')}
    handleDateEdit={handleDateEdit('actualMainActivityDate')}
  />}
</div>)

const { func } = React.PropTypes

EditLocalUnit.propTypes = {
  handleEdit: func.isRequired,
}

EditLocalUnit.propTypes = { localize: React.PropTypes.func.isRequired }

export default wrapper(EditLocalUnit)
