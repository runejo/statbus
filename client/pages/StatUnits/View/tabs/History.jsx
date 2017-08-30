import React from 'react'
import { func, shape, number } from 'prop-types'
import { Table, Icon, Loader, Button, Label, Popup } from 'semantic-ui-react'

import { formatDateTime } from 'helpers/dateHelper'

const reasons = {
  0: { icon: 'plus', name: 'ReasonCreate' },
  1: { icon: 'edit', name: 'ReasonEdit' },
  2: { icon: 'write', name: 'ReasonCorrect' },
  3: { icon: 'trash', name: 'ReasonDelete' },
  4: { icon: 'undo', name: 'ReasonUndelete' },
  null: { icon: 'help', name: 'ReasonUnknown' },
}

const maxLength = 40

const substringComment = (str) => {
  if (str === undefined || str === null || str.length < maxLength) return str || ''
  return (
    <Popup
      trigger={<p>{str.substring(0, maxLength)} <Icon name="plus square outline" /></p>}
      content={str}
      on="click"
      size="large"
    />
  )
}

class HistoryList extends React.Component {

  static propTypes = {
    localize: func.isRequired,
    fetchHistory: func.isRequired,
    fetchHistoryDetails: func.isRequired,
    data: shape({
      type: number.isRequired,
      id: number.isRequired,
    }).isRequired,
    history: shape({}).isRequired,
    historyDetails: shape({}).isRequired,
  }

  state = {
    selectedRow: undefined,
  }

  componentDidMount() {
    const { fetchHistory, data: { type, id } } = this.props
    fetchHistory(type, id)
  }

  setActiveRow(r) {
    const { fetchHistoryDetails, data: { type } } = this.props
    this.setState(
      { selectedRow: r.regId },
      () => fetchHistoryDetails(type, r.regId),
    )
  }

  render() {
    const { history, historyDetails, localize } = this.props
    return (
      <Table celled>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>{localize('Account')}</Table.HeaderCell>
            <Table.HeaderCell>{localize('ChangeReason')}</Table.HeaderCell>
            <Table.HeaderCell>{localize('Comment')}</Table.HeaderCell>
            <Table.HeaderCell>{localize('StartPeriod')}</Table.HeaderCell>
            <Table.HeaderCell>{localize('EndPeriod')}</Table.HeaderCell>
            <Table.HeaderCell><Icon name="content" /></Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {history.result !== undefined && history.result.map(r =>
            this.state.selectedRow === r.regId
              ? <Table.Row key={r.regId}>
                <Table.Cell colSpan="6">
                  <Table>
                    <Table.Header>
                      <Table.HeaderCell>{localize('Name')}</Table.HeaderCell>
                      <Table.HeaderCell>{localize('ValueBefore')}</Table.HeaderCell>
                      <Table.HeaderCell>{localize('ValueAfter')}</Table.HeaderCell>
                      <Table.HeaderCell width="1">
                        <Button
                          icon="window close"
                          size="mini"
                          onClick={() => this.setState({ selectedRow: undefined })}
                          negative
                        />
                      </Table.HeaderCell>
                    </Table.Header>
                    <Table.Body><Table.Row><Table.Cell colSpan="4">
                      <Label ribbon color="blue" size="small">
                        <Label color="blue" tag>{`${localize('RecordCreatedBy')}: `}
                          <Label.Detail>{r.name}</Label.Detail>
                        </Label>
                        <Label color="blue" tag>{`${localize('At')}: `}
                          <Label.Detail>{formatDateTime(r.startPeriod)}</Label.Detail>
                        </Label>
                        <Label color="blue" tag>{`${localize('DueReason')}: `}
                          <Label.Detail>
                            <Icon name={reasons[r.changeReason].icon} />
                            {localize(reasons[r.changeReason].name)}
                          </Label.Detail>
                        </Label>
                        <Label color="blue" tag>{`${localize('WithСomment')}: `}
                          <Label.Detail>{substringComment(r.editComment)}</Label.Detail>
                        </Label>
                      </Label>
                    </Table.Cell></Table.Row>
                      {(historyDetails === undefined || historyDetails.result === undefined)
                        ? <Table.Row>
                          <Table.Cell colSpan="4">
                            <Loader active inline size="mini" content="Loading..." />
                          </Table.Cell>
                        </Table.Row>
                        : historyDetails.result.map(d => (
                          <Table.Row key={`${r.regId}_${d.name}`}>
                            <Table.Cell>{d.name}</Table.Cell>
                            <Table.Cell>{d.before}</Table.Cell>
                            <Table.Cell colSpan="2">{d.after}</Table.Cell>
                          </Table.Row>
                        ))
                      }
                    </Table.Body>
                    <Table.Footer>
                      <Table.HeaderCell colSpan="4">
                        {`${localize('TotalChanges')}: ${historyDetails.totalCount}`}
                      </Table.HeaderCell>
                    </Table.Footer>
                  </Table>
                </Table.Cell>
              </Table.Row>
            : <Table.Row key={r.regId}>
              <Table.Cell>{r.name}</Table.Cell>
              <Table.Cell>
                <Icon name={reasons[r.changeReason].icon} />
                {localize(reasons[r.changeReason].name)}
              </Table.Cell>
              <Table.Cell>{substringComment(r.editComment)}</Table.Cell>
              <Table.Cell>{formatDateTime(r.startPeriod)}</Table.Cell>
              <Table.Cell>{formatDateTime(r.endPeriod)}</Table.Cell>
              <Table.Cell width="1">
                <Button
                  icon="content"
                  disabled={r.changeReason === 0}
                  onClick={() => this.setActiveRow(r)}
                  color="blue"
                  size="mini"
                />
              </Table.Cell>
            </Table.Row>)}
        </Table.Body>
        <Table.Footer>
          <Table.Row>
            <Table.HeaderCell colSpan="6">
              {`${localize('Total')}: `}
              {history.totalCount !== undefined && history.totalCount}
            </Table.HeaderCell>
          </Table.Row>
        </Table.Footer>
      </Table>
    )
  }
}

export default HistoryList
