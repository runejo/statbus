import React from 'react'
import { arrayOf, string, number, oneOfType, func, bool, shape } from 'prop-types'
import { Message, Select as SemanticUiSelect, Label } from 'semantic-ui-react'
import ReactSelect from 'react-select'
import debounce from 'lodash/debounce'
import R from 'ramda'

import { hasValue, createPropType } from 'helpers/validation'
import { internalRequest } from 'helpers/request'
import { getNewName } from 'helpers/locale'

import styles from './styles.pcss'

const notSelected = { value: undefined, text: 'NotSelected' }

const NameCodeOption = {
  transform: x => ({
    ...x,
    key: x.id,
    value: x.id,
    label: getNewName(x),
    text: getNewName(x),
  }),
  // eslint-disable-next-line react/prop-types
  render: params => (
    <div className="content">
      <div className="title">
        {params.code && <div className={styles['select-field-code']}>{params.code}</div>}
        {params.code && <br />}
        {getNewName(params, false)}
        <hr />
      </div>
    </div>
  ),
}

// eslint-disable-next-line react/prop-types
const createRemovableValueComponent = localize => ({ value, onRemove }) => (
  <Label
    content={value.value === notSelected.value ? localize(value.label) : value.label}
    onRemove={() => onRemove(value)}
    removeIcon="delete"
    color="blue"
    basic
  />
)

// eslint-disable-next-line react/prop-types
const createValueComponent = localize => ({ value: { value, label } }) => (
  <div className="Select-value">
    <span className="Select-value-label" role="option" aria-selected="true">
      {value === notSelected.value ? localize(notSelected.text) : label}
    </span>
  </div>
)

const numOrStr = oneOfType([number, string])

class SelectField extends React.Component {
  static propTypes = {
    name: string.isRequired,
    value: createPropType(props => (props.multiselect ? arrayOf(numOrStr) : numOrStr)),
    onChange: func.isRequired,
    onBlur: func,
    error: bool,
    errors: arrayOf(string),
    label: string,
    title: string,
    placeholder: string,
    multiselect: bool,
    required: bool,
    touched: bool,
    disabled: bool,
    inline: bool,
    width: numOrStr,
    createOptionComponent: func,
    localize: func.isRequired,
    popuplocalizedKey: string,
    pageSize: number,
    waitTime: number,
    lookup: number,
    responseToOption: func,
    isEdit: bool,
    locale: string,
    options: arrayOf(shape({
      value: numOrStr.isRequired,
      text: numOrStr.isRequired,
    })),
    url: string,
  }

  static defaultProps = {
    value: undefined,
    onBlur: R.identity,
    label: undefined,
    title: undefined,
    placeholder: undefined,
    multiselect: false,
    required: false,
    error: false,
    errors: [],
    disabled: false,
    inline: false,
    width: undefined,
    createOptionComponent: NameCodeOption.render,
    pageSize: 10,
    waitTime: 250,
    lookup: undefined,
    responseToOption: NameCodeOption.transform,
    options: undefined,
    isEdit: false,
    locale: '',
    url: '',
    touched: false,
    popuplocalizedKey: undefined,
  }

  state = {
    initialValue: this.props.value,
    value: hasValue(this.props.value)
      ? this.props.value
      : this.props.multiselect
        ? []
        : notSelected.value,
    optionsFetched: false,
    options: [],
    test: [
      {
        id: 15,
        code: '10000',
        name: 'Nhkqoer',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 15,
        value: 15,
        label: '10000 Nhkqoer',
        text: '10000 Nhkqoer',
      },
      {
        id: 18,
        code: '10001',
        name: 'Gurulpm',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 18,
        value: 18,
        label: '10001 Gurulpm',
        text: '10001 Gurulpm',
      },
      {
        id: 21,
        code: '10002',
        name: 'Mlfoauh mqjjg',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 21,
        value: 21,
        label: '10002 Mlfoauh mqjjg',
        text: '10002 Mlfoauh mqjjg',
      },
      {
        id: 24,
        code: '10003',
        name: 'QbdsvtaVnshiex',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 24,
        value: 24,
        label: '10003 QbdsvtaVnshiex',
        text: '10003 QbdsvtaVnshiex',
      },
      {
        id: 27,
        code: '10004',
        name: 'Qsznivl brdwn',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 27,
        value: 27,
        label: '10004 Qsznivl brdwn',
        text: '10004 Qsznivl brdwn',
      },
      {
        id: 30,
        code: '10005',
        name: 'BJD',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 30,
        value: 30,
        label: '10005 BJD',
        text: '10005 BJD',
      },
      {
        id: 33,
        code: '10006',
        name: 'Wauddqn xHcgutka y',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 33,
        value: 33,
        label: '10006 Wauddqn xHcgutka y',
        text: '10006 Wauddqn xHcgutka y',
      },
      {
        id: 36,
        code: '10007',
        name: 'LCBVW',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 36,
        value: 36,
        label: '10007 LCBVW',
        text: '10007 LCBVW',
      },
      {
        id: 39,
        code: '10008',
        name: 'Qromwjf ptuzh',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 39,
        value: 39,
        label: '10008 Qromwjf ptuzh',
        text: '10008 Qromwjf ptuzh',
      },
      {
        id: 42,
        code: '10009',
        name: 'Hcgutka y',
        nameLanguage1: null,
        nameLanguage2: null,
        key: 42,
        value: 42,
        label: '10009 Hcgutka y',
        text: '10009 Hcgutka y',
      },
    ],
  }

  componentDidMount() {
    if (hasValue(this.props.options)) return
    const { value: ids, lookup, multiselect, responseToOption } = this.props
    internalRequest({
      url: `/api/lookup/${lookup}/GetById/`,
      queryParams: { ids },
      method: 'get',
      onSuccess: (value) => {
        if (hasValue(value)) {
          this.setState({
            value: multiselect ? value.map(responseToOption) : responseToOption(value[0]),
          })
        }
      },
    })
    fetch(`/api/lookup/paginated/${lookup}?page=0&pageSize=10`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
    })
      .then(resp => resp.json())
      .then((result) => {
        const options =
          Array.isArray(result) && result.length > 0 ? result.map(responseToOption) : []
        this.setState({ options })
      })
  }

  componentWillReceiveProps(nextProps) {
    const { locale, multiselect, responseToOption, onChange, isEdit, url } = this.props
    const { value, initialValue } = this.state
    const isEditDataSource = url.includes('datasources' && 'edit')
    if (isEdit || isEditDataSource) {
      if (R.equals(initialValue, nextProps.value)) {
        this.setState({ value: initialValue })
      } else {
        this.setState({ value: nextProps.value })
      }
    }
    if (nextProps.locale !== locale) {
      const currValue = hasValue(value) ? value : []
      const isArrayOfStrings =
        Array.isArray(currValue) && currValue.every(x => typeof x === 'string')
      this.setState({
        value: multiselect
          ? isArrayOfStrings
            ? currValue
            : currValue.map(responseToOption)
          : typeof currValue === 'string'
            ? currValue
            : responseToOption(currValue),
        options: this.state.options.map(responseToOption),
      })
      return
    }
    if (
      R.isEmpty(nextProps.value) ||
      (Array.isArray(nextProps.value) && nextProps.value.every(x => x === '')) ||
      (R.is(Array, nextProps.value) && R.isEmpty(nextProps.value))
    ) {
      this.setState({ value: '' })
    }
    if (!R.equals(nextProps.value && value)) {
      this.setState({ value: nextProps.value })
    }
    if (R.isNil(nextProps.value) || (R.is(Array, nextProps.value) && R.isEmpty(nextProps.value))) {
      this.setState({ value: '' }, () => onChange(undefined, { ...this.props, value: '' }))
    }
  }

  componentWillUnmount() {
    this.handleLoadOptions.cancel()
  }

  componentDidUpdate() {
    if (this.props.multiselect) {
      console.log('this props', this.props)

      console.log('this state', this.state.options)
    }
  }

  loadOptions = (wildcard, page, callback) => {
    const { lookup, pageSize, multiselect, required, responseToOption } = this.props

    // console.log('page', page)
    console.log('loadOptions')

    const { optionsFetched } = this.state
    internalRequest({
      url: `/api/lookup/paginated/${lookup}`,
      queryParams: { page: page - 1, pageSize, wildcard },
      method: 'get',
      onSuccess: (data) => {
        let options = data
        if (responseToOption) options = options.map(responseToOption)
        if (optionsFetched) {
          this.setState({ options: this.state.options.concat(options) }, () => {
            callback(null, { options })
          })
        } else {
          this.setState({ optionsFetched: true }, () => {
            callback(null, { options })
          })
        }
      },
    })
  }

  handleLoadOptions = debounce(this.loadOptions, this.props.waitTime)

  handleAsyncSelect = (data) => {
    const { multiselect, onChange, responseToOption } = this.props
    const raw = data !== null ? data : { value: notSelected.value }
    const value = multiselect ? R.uniq(raw.map(x => x.value)) : raw.value
    if (!R.equals(this.state.value, value)) {
      this.setState(
        {
          value: multiselect ? raw.map(responseToOption) : responseToOption(raw),
        },
        () => onChange(undefined, { ...this.props, value }, data),
      )
    }
  }

  handlePlainSelect = (event, { value, ...data }) => {
    const nextData = { ...data, ...this.props, value }
    if (!R.equals(this.state.value, value)) {
      this.setState({ value }, () => this.props.onChange(event, nextData))
    }
  }

  render() {
    const {
      name,
      label: labelKey,
      touched,
      error,
      errors: errorKeys,
      options,
      multiselect,
      title: titleKey,
      placeholder: placeholderKey,
      createOptionComponent,
      required,
      disabled,
      inline,
      width,
      onBlur,
      localize,
    } = this.props
    const hasErrors = (touched && hasValue(errorKeys)) || (error && hasValue(errorKeys))
    const label = labelKey !== undefined ? localize(labelKey) : undefined
    const title = titleKey ? localize(titleKey) : label
    const placeholder = placeholderKey ? localize(placeholderKey) : label
    const hasOptions = hasValue(options)
    const [Select, ownProps] = hasOptions
      ? [
        SemanticUiSelect,
        {
          onChange: this.handlePlainSelect,
          error: hasErrors,
          multiple: multiselect,
          options,
          // multiselect || !required
          //   ? options
          //   : [{ value: notSelected.value, text: localize(notSelected.text) }, ...options],
          required,
          title,
          inline,
          width,
        },
      ]
      : [
        ReactSelect.Async,
        {
          onChange: this.handleAsyncSelect,
          loadOptions: this.handleLoadOptions,
          getOptionLabel: option => option.name,
          getOptionValue: option => option.key,
          valueComponent: multiselect
            ? createRemovableValueComponent(localize)
            : createValueComponent(localize),
          optionRenderer: createOptionComponent,
          inputProps: { type: 'react-select', name },
          className: hasErrors ? 'react-select--error' : '',
          multi: multiselect,
          backspaceRemoves: true,
          searchable: true,
          pagination: true,
          clearable: false,
          filterOptions: R.identity,
          required,
          removeSelected: true,
        },
      ]
    const className = `field${!hasOptions && required ? ' required' : ''}`

    return (
      <div className={className} style={{ opacity: `${disabled ? 0.25 : 1}` }}>
        {label !== undefined && <label htmlFor={name}>{label}</label>}
        <Select
          {...ownProps}
          value={this.state.value}
          // options={this.props.options ? this.props.options : this.state.options}
          options={this.state.options}
          onBlur={onBlur}
          name={name}
          placeholder={placeholder}
          disabled={disabled}
          autoComplete="off"
          // removeSelected: true,
          // removeSelected={this.state.removeSelected}
          // openOnFocus
        />
        {hasErrors && (
          <Message title={label} list={errorKeys.map(localize)} compact={hasOptions} error />
        )}
      </div>
    )
  }
}

export default SelectField
