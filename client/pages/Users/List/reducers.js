import { createReducer } from 'redux-act'

import { defaultPageSize } from 'components/Paginate/utils'
import * as actions from './actions'

const users = createReducer(
  {
    [actions.fetchUsersSucceeded]: (state, data) => ({
      ...state,
      users: data.result,
      totalCount: data.totalCount,
      totalPages: data.totalPages,
      filter: data.filter,
      isLoading: false,
    }),
    [actions.fetchUsersStarted]: (state, filter) => ({
      ...state,
      isLoading: true,
      filter,
    }),
    [actions.fetchUsersFailed]: state => ({
      ...state,
      users: [],
      isLoading: false,
    }),
  },
  {
    isLoading: false,
    users: [],
    totalCount: 0,
    totalPages: 0,
    filter: {
      page: 1,
      pageSize: defaultPageSize,
      sortColumn: 'name',
      sortAscending: true,
    },
  },
)

export default {
  users,
}
