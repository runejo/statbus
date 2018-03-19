import moment from 'moment'

import { hasValue } from 'helpers/validation'

export const dateFormat = 'YYYY-MM-DD'
export const dateTimeFormat = 'YYYY-MM-DD HH:mm'

export function formatDate(x, format = dateFormat) {
  return moment(x).format(format)
}

export function formatDateTime(x, format = dateTimeFormat) {
  return moment(x).format(format)
}

export function getDate(utcString = null) {
  return utcString ? moment(utcString) : moment()
}

export function getDateOrNull(raw) {
  return hasValue(raw) ? moment(raw) : null
}

export function toUtc(value) {
  return moment(value)
    .utcOffset(0, true)
    .format()
}

export function setMomentLocale(locale) {
  moment.locale(locale)
}

export function now() {
  return moment()
    .utcOffset(0, true)
    .startOf('day')
}

export function parse(date, format = dateFormat, exact = true) {
  return moment(date, format, exact)
}

export function isDateInThePast(date) {
  return now().diff(date) > 0
}
