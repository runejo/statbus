import { withFormik } from 'formik'
import { pipe, prop } from 'ramda'

import createSubForm from './createSubForm'

const handleSubmit = (values, { props: { onSubmit, ...props }, setSubmitting, setStatus }) => {
  onSubmit(values, {
    props,
    started: () => {
      setSubmitting(true)
    },
    succeeded: () => {
      setSubmitting(false)
    },
    failed: (errors) => {
      setSubmitting(false)
      setStatus({ errors })
    },
  })
}
// const onReset = () => {
//   console.log('ПРОИЗОШЕЛ РЕСЕТ')
// }

export default (validationSchema, mapPropsToValues = prop('values'), showReset = true) =>
  pipe(
    createSubForm(showReset),
    withFormik({ validationSchema, mapPropsToValues, handleSubmit }),
  )
