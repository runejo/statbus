import React from 'react'
import PropTypes from 'prop-types'
import { Link } from 'react-router'
import { Button, Table, Segment, Form, Confirm } from 'semantic-ui-react'

import { checkSystemFunction as sF } from 'helpers/config'
import { internalRequest } from 'helpers/request'
import Paginate from 'components/Paginate'
import { dateTimeFormat } from 'helpers/dateHelper'

class List extends React.Component {
  state = { id: undefined }

  askDelete = id => () => this.setState({ id })

  cancelDelete = () => this.setState({ id: undefined })

  confirmDelete = () => {
    const { id } = this.state
    this.setState({ id: undefined }, () => this.props.deleteSampleFrame(id))
  }

  handleEdit = (_, { name, value }) => this.props.updateFilter({ [name]: value })

  handleSubmit = (e) => {
    e.preventDefault()
    this.props.setQuery(this.props.formData)
  }

  handleDownload = (_, { item }) => {
    internalRequest({
      url: `/api/sampleframes/${item.id}/preview/download`,
      method: 'get',
      onSuccess: () => {
        this.props.getSampleFrames(this.props.query)
      },
    })
  }

  handleCheckFileGeneration = () => {
    this.props.getSampleFrames(this.props.query)
  }

  renderConfirm() {
    const { result, localize } = this.props
    const { name } = result.find(x => x.id === this.state.id)
    return (
      <Confirm
        onConfirm={this.confirmDelete}
        onCancel={this.cancelDelete}
        header={`${localize('AreYouSure')}?`}
        content={`${localize('DeleteSampleFrameMessage')} "${name}"?`}
        open
      />
    )
  }

  render() {
    const { formData, result, totalCount, localize } = this.props
    const canPreview = sF('SampleFramesView')
    const canEdit = sF('SampleFramesEdit')
    const canDelete = sF('SampleFramesDelete')
    return (
      <div>
        <h2>{localize('SampleFrames')}</h2>
        {canEdit && (
          <Button
            as={Link}
            to="/sampleframes/create"
            content={localize('CreateSampleFrame')}
            icon="add square"
            size="medium"
            color="green"
          />
        )}
        {this.state.id != null && this.renderConfirm()}
        <Segment>
          <Form onSubmit={this.handleSubmit}>
            <Form.Input
              name="wildcard"
              value={formData.wildcard}
              onChange={this.handleEdit}
              label={localize('SampleFramesWildcard')}
            />
            <Button
              type="submit"
              content={localize('Search')}
              icon="search"
              primary
              floated="right"
            />
            <br />
            <br />
            <br />
          </Form>
          <Paginate totalCount={Number(totalCount)}>
            <Table selectable size="small" fixed>
              <Table.Header>
                <Table.Row>
                  <Table.HeaderCell content={localize('Name')} width="5" />
                  <Table.HeaderCell content={localize('Description')} width="5" />
                  {(canPreview || canDelete) && <Table.HeaderCell />}
                </Table.Row>
              </Table.Header>
              <Table.Body>
                {result.map(x => (
                  <Table.Row key={x.id}>
                    <Table.Cell>
                      {canEdit ? <Link to={`/sampleframes/${x.id}`}>{x.name}</Link> : x.name}
                    </Table.Cell>
                    <Table.Cell>{x.description}</Table.Cell>
                    {(canDelete || canPreview) && (
                      <Table.Cell>
                        {canDelete && (
                          <Button
                            onClick={this.askDelete(x.id)}
                            icon="trash"
                            size="mini"
                            color="red"
                            floated="right"
                          />
                        )}
                        {canPreview && (
                          <div>
                            {Number(x.status) === 1 && (
                              <Button
                                onClick={this.handleDownload}
                                item={x}
                                content={localize('SampleFrameGenerationEnqueue')}
                                color="green"
                                size="mini"
                                floated="right"
                              />
                            )}
                            {[4, 6].includes(Number(x.status)) && (
                              <Button
                                as="a"
                                href={`/api/sampleframes/${x.id}/preview/download`}
                                target="__blank"
                                content={localize('DownloadSampleFrame')}
                                onClick={this.handleCheckFileGeneration}
                                icon="download"
                                color="blue"
                                size="mini"
                                floated="right"
                              />
                            )}
                            {[2, 3, 5].includes(Number(x.status)) && (
                              <Button
                                onClick={this.handleCheckFileGeneration}
                                content={localize('SampleFrameGenerationEnqueue')}
                                item={x}
                                loading={x.loading}
                                color={Number(x.status) === 5 ? 'red' : null}
                                disabled={Number(x.status) === 5}
                                size="mini"
                                floated="right"
                              >
                                {Number(x.status) === 2 && localize('InQueue')}
                                {Number(x.status) === 3 && localize('InProgress')}
                                {Number(x.status) === 5 && localize('SampleFrameGenerationError')}
                              </Button>
                            )}
                          </div>
                        )}
                        {canPreview && (
                          <Button
                            as={Link}
                            to={`/sampleframes/preview/${x.id}`}
                            content={localize('PreviewSampleFrame')}
                            icon="search"
                            color="blue"
                            size="mini"
                            floated="right"
                          />
                        )}
                      </Table.Cell>
                    )}
                  </Table.Row>
                ))}
              </Table.Body>
            </Table>
          </Paginate>
        </Segment>
      </div>
    )
  }
}

const { arrayOf, func, number, shape, string } = PropTypes
List.propTypes = {
  formData: shape({
    wildcard: string.isRequired,
  }).isRequired,
  result: arrayOf(shape({
    id: number.isRequired,
    name: string.isRequired,
    status: number.isRequired,
    generatedDateTime: dateTimeFormat,
  })).isRequired,
  totalCount: number.isRequired,
  query: shape.isRequired,
  setQuery: func.isRequired,
  updateFilter: func.isRequired,
  getSampleFrames: func.isRequired,
  deleteSampleFrame: func.isRequired,
  localize: func.isRequired,
}

export default List
