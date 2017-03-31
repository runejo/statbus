import { createReducer } from 'redux-act'

import * as actions from './actions'

const initialState = {
  user: undefined,
}

const editUser = createReducer(
  {
    [actions.fetchUserSucceeded]: (state, data) => ({
      ...state,
      user: data,
    }),
    [actions.editForm]: (state, data) => ({
      ...state,
      user: { ...state.user, [data.name]: data.value },
    }),
    [actions.fetchUsersStarted]: state => ({
      ...state,
      user: undefined,
    }),
  },
  initialState,
)

export default {
  editUser,
}
