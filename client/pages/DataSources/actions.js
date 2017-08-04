import { createAction } from 'redux-act'
import { push } from 'react-router-redux'
import { pipe } from 'ramda'

import schemaHelpers from 'helpers/schema'
import dispatchRequest from 'helpers/request'
import { actions as rqstActions } from 'helpers/requestStatus'
import schema from './schema'

export const clear = createAction('clear filter on DataSources')

const updateFilter = createAction('update data sources search form')
const setQuery = pathname => query => (dispatch) => {
  pipe(updateFilter, dispatch)(query)
  pipe(push, dispatch)({ pathname, query })
}

const fetchDataSourcesSucceeded = createAction('fetched data sources')
export const fetchDataSources = queryParams => dispatchRequest({
  queryParams,
  onSuccess: (dispatch, response) => {
    const { page, pageSize, ...formData } = queryParams
    dispatch(updateFilter(formData))
    dispatch(fetchDataSourcesSucceeded(response))
  },
})

const fetchDataSourcesListSucceeded = createAction('fetched data sources list')
export const fetchDataSourcesList = () => dispatchRequest({
  url: '/api/datasources',
  method: 'get',
  onSuccess: (dispatch, response) => {
    dispatch(fetchDataSourcesListSucceeded(response))
  },
})

const uploadFileSucceeded = createAction('upload file')
const uploadFileError = createAction('upload file error')
export const uploadFile = (body, callback) => (dispatch) => {
  const startedAction = rqstActions.started()
  const startedId = startedAction.data.id
  const xhr = new XMLHttpRequest()
  xhr.onload = (response) => {
    dispatch(uploadFileSucceeded(response))
    callback()
    dispatch(rqstActions.succeeded())
    dispatch(rqstActions.dismiss(startedId))
  }
  xhr.onerror = (error) => {
    dispatch(uploadFileError(error))
    callback()
    dispatch(rqstActions.failed(error))
    dispatch(rqstActions.dismiss(startedId))
  }
  xhr.open('post', '/api/datasourcesqueue', true)
  xhr.send(body)
}

const fetchColumnsSucceeded = createAction('fetched columns')
const fetchColumns = () => dispatchRequest({
  url: '/api/datasources/MappingProperties',
  onSuccess: (dispatch, response) =>
    dispatch(fetchColumnsSucceeded(response)),
})

const createDataSource = data => dispatchRequest({
  url: '/api/datasources',
  method: 'post',
  body: data,
  onSuccess: dispatch =>
    dispatch(push('/datasources')),
})

const fetchDataSourceSucceeded = createAction('fetched datasource')

const cast = resp => schema.cast(schemaHelpers.nullsToUndefined(resp))
const fetchDataSource = id => dispatchRequest({
  url: `api/datasources/${id}`,
  onSuccess: (dispatch, response) =>
    pipe(cast, fetchDataSourceSucceeded, dispatch)(response),
})

const editDataSource = id => data => dispatchRequest({
  url: `/api/datasources/${id}`,
  method: 'put',
  body: data,
  onSuccess: dispatch =>
    dispatch(push('/datasources')),
})

export const deleteDataSource = id => dispatchRequest({
  url: `/api/datasources/${id}`,
  method: 'delete',
  onSuccess: () => {
    window.location.reload()
  },
})

export const search = {
  setQuery,
  updateFilter,
}

export const create = {
  fetchColumns,
  submitData: createDataSource,
}

export const edit = {
  fetchDataSource,
  fetchColumns,
  submitData: editDataSource,
}

export default {
  updateFilter,
  fetchColumnsSucceeded,
  fetchDataSourcesSucceeded,
  fetchDataSourcesListSucceeded,
  fetchDataSourceSucceeded,
  uploadFileSucceeded,
  uploadFileError,
}
