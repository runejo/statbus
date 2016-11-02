import { createReducer } from 'redux-act'
import * as actions from './actions'

export const roles = createReducer(
  {
    [actions.fetchRolesStarted]: state => ({
      ...state,
      status: 1,
      message: undefined,
    }),
    [actions.fetchRolesSucceeded]: (state, data) => ({
      ...state,
      roles: data.result,
      totalCount: data.totalCount,
      totalPages: data.totalPages,
      status: 2,
      message: undefined,
    }),
    [actions.fetchRolesFailed]: (state, data) => ({
      ...state,
      status: -1,
      message: data,
    }),
    [actions.deleteRoleStarted]: state => ({
      ...state,
      status: 1,
      message: undefined,
    }),
    [actions.deleteRoleSucceeded]: (state, data) => ({
      ...state,
      roles: state.roles.filter(r => r.id !== data),
      totalCount: state.totalCount - 1,
      status: 0,
      message: undefined,
    }),
    [actions.deleteRoleFailed]: (state, data) => ({
      ...state,
      status: -1,
      message: data,
    }),
  },
  {
    message: undefined,
    roles: [],
    status: 0,
    totalCount: 0,
    totalPages: 0,
  }
)
