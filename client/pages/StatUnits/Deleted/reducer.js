import { createReducer } from 'redux-act'

import * as actions from './actions'
import { updateFilter } from '../actions'
import { setSearchCondition, clear } from '../Search/actions'

const defaultState = {
  formData: { sortRule: 1 },
  statUnits: [],
  totalCount: 0,
  isLoading: false,
}

const handlers = {
  [updateFilter]: (state, data) => ({
    ...state,
    formData: { ...state.formData, ...data },
  }),

  [actions.fetchDataSucceeded]: (state, { result, totalCount }) => ({
    ...state,
    statUnits: result,
    totalCount,
    isLoading: false,
  }),

  [actions.fetchDataStarted]: state => ({
    ...state,
    isLoading: true,
  }),
  [actions.setSearchConditionForDeleted]: (state, condition) => ({
    ...state,
    formData: {
      ...state.formData,
      comparison: condition,
    },
  }),
  [actions.clearSearchFormForDeleted]: () => defaultState,
}

export default createReducer(handlers, defaultState)
