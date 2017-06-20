import React from 'react'
import DatePicker from 'react-datepicker'

import { getDate, toUtc, dateFormat } from 'helpers/dateHelper'
import styles from './styles.pcss'

const Calendar = ({ name, value, onChange, labelKey, localize }) => {
  const handleChange = (date) => {
    onChange(undefined, { name, value: date === null ? value : toUtc(date) })
  }
  const label = localize(labelKey)
  return (
    <div className={`field ${styles.datepicker}`}>
      <label htmlFor={name}>{label}</label>
      <DatePicker
        selected={value === '' ? '' : getDate(value)}
        name={name}
        value={value}
        onChange={handleChange}
        dateFormat={dateFormat}
        className="ui input"
      />
    </div>
  )
}

const { func, string } = React.PropTypes
Calendar.propTypes = {
  onChange: func.isRequired,
  localize: func.isRequired,
  name: string.isRequired,
  value: string.isRequired,
  labelKey: string.isRequired,
}

export default Calendar