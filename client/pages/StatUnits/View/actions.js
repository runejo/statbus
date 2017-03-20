import { createAction } from 'redux-act'

import dispatchRequest from 'helpers/request'

export const fetchStatUnitSucceeded = createAction('fetch StatUnit succeeded')
export const fetchStatUnit = (type, id) =>
  dispatchRequest({
    url: `/api/StatUnits/${type}/${id}`,
    onSuccess: (dispatch, resp) => {
      dispatch(fetchStatUnitSucceeded(resp))
    },
  })
