import { createReducer } from 'redux-act'

import * as actions from './actions'

const initialState = {
  filter: undefined,
}

const viewLinks = createReducer(
  {
    [actions.linkSearchStarted]: (state, filter) => ({
      ...state,
      filter,
    }),
  },
  initialState,
)

export default {
  viewLinks,
}
