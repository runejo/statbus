import React from 'react'
import { Link } from 'react-router'
import { Button, Form, Loader, Icon } from 'semantic-ui-react'

import { systemFunction as sF } from 'helpers/checkPermissions'
import { wrapper } from 'helpers/locale'
import styles from './styles'

class EditDetails extends React.Component {
  componentDidMount() {
    this.props.fetchAccount()
  }
  render() {
    const { account, editForm, submitAccount, localize } = this.props
    const handleSubmit = (e) => {
      e.preventDefault()
      if (sF('AccountEdit')) submitAccount(account)
    }
    const handleEdit = propName => (e) => { editForm({ propName, value: e.target.value }) }
    return (
      <div className={styles.accountEdit}>
        <h2>{localize('EditAccount')}</h2>
        {account === undefined
          ? <Loader active />
          : <Form className={styles.form} onSubmit={handleSubmit}>
            <Form.Input
              value={account.name}
              onChange={handleEdit('name')}
              name="name"
              label={localize('Name')}
              placeholder={localize('NameValueRequired')}
              required
            />
            <Form.Input
              value={account.currentPassword || ''}
              onChange={handleEdit('currentPassword')}
              name="currentPassword"
              type="password"
              label={localize('CurrentPassword')}
              placeholder={localize('CurrentPassword')}
              required
            />
            <Form.Input
              value={account.newPassword || ''}
              onChange={handleEdit('newPassword')}
              name="newPassword"
              type="password"
              label={localize('NewPassword_LeaveItEmptyIfYouWillNotChangePassword')}
              placeholder={localize('NewPassword')}
            />
            <Form.Input
              value={account.confirmPassword || ''}
              onChange={handleEdit('confirmPassword')}
              name="confirmPassword"
              type="password"
              label={localize('ConfirmPassword')}
              placeholder={localize('ConfirmPassword')}
              error={account.newPassword
                && account.newPassword.length > 0
                && account.newPassword !== account.confirmPassword}
            />
            <Form.Input
              value={account.phone}
              onChange={handleEdit('phone')}
              name="phone"
              type="tel"
              label={localize('Phone')}
              placeholder={localize('PhoneValueRequired')}
            />
            <Form.Input
              value={account.email}
              onChange={handleEdit('email')}
              name="email"
              type="email"
              label={localize('Email')}
              placeholder={localize('EmailValueRequired')}
              required
            />
            <Button
              as={Link} to="/"
              content={localize('Back')}
              icon={<Icon size="large" name="chevron left" />}
              size="small"
              color="gray"
              type="button"
            />
            <Button className={styles.sybbtn} type="submit" primary>{localize('Submit')}</Button>
          </Form>}
      </div>
    )
  }
}

EditDetails.propTypes = { localize: React.PropTypes.func.isRequired }

export default wrapper(EditDetails)
