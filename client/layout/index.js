import { connect } from 'react-redux'
import Layout from './Layout'

const mapStateToProps = (_, { routes, location, params }) => ({
  routerProps: {
    routes,
    location,
    params,
  },
})

export default connect(mapStateToProps)(Layout)
