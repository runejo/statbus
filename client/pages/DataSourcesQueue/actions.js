import { createAction } from 'redux-act'
import { push } from 'react-router-redux'
import dispatchRequest from 'helpers/request'
import R from 'ramda'

const updateQueueFilter = createAction('update search dataSourcesQueue form')
const fetchQueueStarted = createAction('fetch regions started')
const fetchQueueFailed = createAction('fetch DataSourceQueue failed')
const fetchQueueSucceeded = createAction('fetch DataSourceQueue successed')
const fetchLogStarted = createAction('fetch regions started')
const fetchLogFailed = createAction('fetch DataSourceQueue failed')
const fetchLogSucceeded = createAction('fetch DataSourceQueue successed')
const fetchLogEntryStarted = createAction('fetch log entry started')
const fetchLogEntrySucceeded = createAction('fetch log entry succeeded')
const fetchLogEntryFailed = createAction('fetch log entry failed')
const clear = createAction('clear filter on DataSourceQueue')

const setQuery = pathname => query => (dispatch) => {
  R.pipe(updateQueueFilter, dispatch)(query)
  const status = query.status === 'any' ? undefined : query.status
  R.pipe(push, dispatch)({ pathname, query: { ...query, status } })
}

const fetchQueue = queryParams =>
  dispatchRequest({
    url: '/api/datasourcesqueue',
    queryParams,
    onSuccess: (dispatch, resp) => {
      dispatch(fetchQueueSucceeded({ ...resp, queryObj: queryParams }))
    },
    onFail: (dispatch, errors) => {
      dispatch(fetchQueueFailed(errors))
    },
  })

const fetchLog = dataSourceId => queryParams =>
  dispatchRequest({
    url: `/api/datasourcesqueue/${dataSourceId}/log`,
    queryParams,
    onSuccess: (dispatch, resp) => {
      dispatch(fetchLogSucceeded(resp))
    },
    onFail: (dispatch, errors) => {
      dispatch(fetchLogFailed(errors))
    },
  })

const fetchLogEntry = id =>
  dispatchRequest({
    url: `/api/datasourcesqueue/log/${id}`,
    onSuccess: (dispatch, resp) => {
      dispatch(fetchLogEntrySucceeded(resp))
    },
    onFail: (dispatch, errors) => {
      dispatch(fetchLogEntryFailed(errors))
    },
  })

const submitLogEntry = id => data =>
  dispatchRequest({
    url: `/api/datasourcesqueue/log/${id}`,
    method: 'put',
    body: data,
    onSuccess: (dispatch, resp) => {
      // ???
    },
    onFail: (dispatch, errors) => {
      // ???
    },
  })

export const list = {
  fetchQueue,
  setQuery,
  updateQueueFilter,
  clear,
}

export const log = {
  fetchLog,
  clear,
}

export const details = {
  fetchLogEntry,
  submitLogEntry,
  clear,
}

export default {
  updateQueueFilter,
  fetchQueueStarted,
  fetchQueueFailed,
  fetchQueueSucceeded,
  clear,
  setQuery,
  fetchLogStarted,
  fetchLogFailed,
  fetchLogSucceeded,
  fetchLogEntryStarted,
  fetchLogEntrySucceeded,
  fetchLogEntryFailed,
}
