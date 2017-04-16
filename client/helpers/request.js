import 'isomorphic-fetch'
import { push } from 'react-router-redux'

import queryObjToString from './queryHelper'
import { actions as rqstActions } from './requestStatus'
import { actions as notificationActions } from './notification'

const redirectToLogInPage = (onError) => {
  onError()
  window.location = `/account/login?urlReferrer=${encodeURIComponent(window.location.pathname)}`
}

const showForbiddenNotificationAndRedirect = (dispatch) => {
  dispatch(notificationActions.showNotification({ body: 'Error403' }))
  dispatch(push('/'))
}

export const internalRequest = ({
  url = `/api${window.location.pathname}`,
  queryParams = {},
  method = 'get',
  body,
  onSuccess = f => f,
  onFail = f => f,
  onForbidden = f => f,
}) => fetch(
  `${url}?${queryObjToString(queryParams)}`,
  {
    method,
    credentials: 'same-origin',
    body: body ? JSON.stringify(body) : undefined,
    headers: body
      ? { 'Content-Type': 'application/json' }
      : undefined,
  },
).then((r) => {
  switch (r.status) {
    case 204:
      return onSuccess()
    case 401:
      return redirectToLogInPage(onFail)
    case 403:
      return onForbidden()
    default:
      return r.status < 300
        ? method === 'get' || method === 'post'
          ? r.json().then(onSuccess)
          : onSuccess(r)
        : r.json().then(onFail)
  }
})
.catch(onFail)

export default ({
  onStart = _ => _,
  onSuccess = _ => _,
  onFail = _ => _,
  ...rest
}) => (
  dispatch,
) => {
  const startedAction = rqstActions.started()
  const startedId = startedAction.data.id
  onStart(dispatch)
  return new Promise((resolve, reject) => {
    internalRequest({
      ...rest,
      onSuccess: (resp) => {
        onSuccess(dispatch, resp)
        dispatch(rqstActions.succeeded())
        dispatch(rqstActions.dismiss(startedId))
        resolve(resp)
      },
      onFail: (errors) => {
        onFail(dispatch, errors)
        dispatch(rqstActions.failed(errors))
        dispatch(rqstActions.dismiss(startedId))
        reject(errors)
      },
      onForbidden: () => {
        showForbiddenNotificationAndRedirect(dispatch)
        reject()
      },
    })
  })
}
