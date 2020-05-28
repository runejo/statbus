import React from 'react'
import { arrayOf, string, number, oneOfType, func, bool, shape } from 'prop-types'
import { Message, Select as SemanticUiSelect, Label } from 'semantic-ui-react'
import ReactSelect from 'react-select'
import debounce from 'lodash/debounce'
import R from 'ramda'

import { hasValue, createPropType } from 'helpers/validation'
import { internalRequest } from 'helpers/request'
import { getNewName } from '../../helpers/locale'

import styles from './styles.pcss'

const notSelected = { value: undefined, text: 'NotSelected' }

const NameCodeOption = {
  transform: x => ({
    ...x,
    value: x.id,
    label: getNewName(x),
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

class InstitutionalSectorCodeField extends React.Component {
  static propTypes = {
    name: string.isRequired,
    value: createPropType(props => (props.multiselect ? arrayOf(numOrStr) : numOrStr)),
    onChange: func.isRequired,
    onBlur: func,
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
    locale: string.isRequired,
    popuplocalizedKey: string,
    pageSize: number,
    waitTime: number,
    lookup: number,
    responseToOption: func,
    options: arrayOf(shape({
      value: numOrStr.isRequired,
      text: numOrStr.isRequired,
    })),
  }

  static defaultProps = {
    value: undefined,
    onBlur: R.identity,
    label: undefined,
    title: undefined,
    placeholder: undefined,
    multiselect: false,
    required: false,
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
    touched: false,
    popuplocalizedKey: undefined,
  }

  state = {
    initialValue: this.props.multiselect ? [] : null,
    value: hasValue(this.props.value)
      ? this.props.value
      : this.props.multiselect
        ? []
        : notSelected.value,
    optionsFetched: false,
    options: [],
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
            initialValue: multiselect ? value.map(responseToOption) : responseToOption(value[0]),
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
    const { locale, multiselect, responseToOption, isEdit } = this.props
    const { value, initialValue } = this.state
    const ids =
      isEdit && R.is(Array, nextProps.value)
        ? R.is(Array, initialValue) && initialValue.map(x => x.id)
        : initialValue && initialValue.id
    if (isEdit && R.equals(ids, nextProps.value)) {
      this.setState({ value: initialValue })
    }
    if (!R.equals(nextProps.value && value)) {
      this.setState({ value: nextProps.value })
    }
    if (nextProps.value === 0 || nextProps.value.length === 0 || nextProps.value[0] === 0) {
      this.setState({ value: '' })
    }
    if (nextProps.locale !== locale) {
      this.setState({
        value: multiselect ? value.map(responseToOption) : responseToOption(value),
        options: this.state.options.map(responseToOption),
      })
    }
  }

  componentWillUnmount() {
    this.handleLoadOptions.cancel()
  }

  loadOptions = (wildcard, page, callback) => {
    const { lookup, pageSize, multiselect, required, responseToOption } = this.props
    const { optionsFetched } = this.state
    internalRequest({
      url: `/api/lookup/paginated/${lookup}`,
      queryParams: { page: page - 1, pageSize, wildcard },
      method: 'get',
      onSuccess: (data) => {
        let options =
          multiselect || !required || optionsFetched
            ? data
            : [{ id: notSelected.value, name: notSelected.text }, ...data]
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
    const value = multiselect ? raw.map(x => x.value) : raw.value
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
      popuplocalizedKey,
    } = this.props
    const hasErrors = touched && hasValue(errorKeys)
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
          options:
              multiselect || !required
                ? options
                : [{ value: notSelected.value, text: localize(notSelected.text) }, ...options],
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
        },
      ]
    const className = `field${!hasOptions && required ? ' required' : ''}`
    return (
      <div
        className={className}
        style={{ opacity: `${disabled ? 0.25 : 1}` }}
        data-tooltip={popuplocalizedKey ? localize(popuplocalizedKey) : null}
        data-position="top left"
      >
        {label !== undefined && <label htmlFor={name}>{label}</label>}
        <Select
          {...ownProps}
          value={this.state.value}
          options={this.state.options}
          onBlur={onBlur}
          name={name}
          placeholder={placeholder}
          disabled={disabled}
          openOnFocus
        />
        {hasErrors && (
          <Message title={label} list={errorKeys.map(localize)} compact={hasOptions} error />
        )}
      </div>
    )
  }
}

export default InstitutionalSectorCodeField
