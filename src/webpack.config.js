/* eslint-disable global-require */
const path = require('path')
const webpack = require('webpack')
const AssetsPlugin = require('assets-webpack-plugin')
const pkg = require('./package.json')

const isDebug = global.DEBUG === false ? false : !process.argv.includes('--release')
const isVerbose = process.argv.includes('--verbose') || process.argv.includes('-v')
const useHMR = !!global.HMR
const babelConfig = Object.assign({}, pkg.babel, {
  babelrc: false,
  cacheDirectory: useHMR,
})

const config = {
  context: path.resolve(__dirname, './client'),
  entry: [
    /* vendor styles & scripts */
    './index.js',
  ],
  output: {
    path: path.resolve(__dirname, './public/dist'),
    publicPath: '/dist/',
    filename: isDebug ? '[name].js?[hash]' : '[name].[hash].js',
    chunkFilename: isDebug ? '[id].js?[chunkhash]' : '[id].[chunkhash].js',
    sourcePrefix: '  ',
  },
  devtool: isDebug ? 'source-map' : false,
  stats: {
    colors: true,
    reasons: isDebug,
    hash: isVerbose,
    version: isVerbose,
    timings: true,
    chunks: isVerbose,
    chunkModules: isVerbose,
    cached: isVerbose,
    cachedAssets: isVerbose,
  },
  plugins: [
    new webpack.optimize.OccurrenceOrderPlugin(),
    new webpack.DefinePlugin({
      'process.env.NODE_ENV': isDebug ? '"development"' : '"production"',
      __DEV__: isDebug,
    }),
    new AssetsPlugin({
      path: path.resolve(__dirname, './public/dist'),
      filename: 'assets.json',
      prettyPrint: true,
    }),
  ],
  module: {
    rules: [
      {
        test: /\.jsx?$/,
        include: [path.resolve(__dirname, './client')],
        use: `babel?${JSON.stringify(babelConfig)}`,
      },
      {
        test: /\.(css|pcss)/,
        use: [
          'style',
          {
            loader: 'css',
            options: {
              importLoaders: 1,
              sourceMap: isDebug,
              modules: true,
              localIdentName: isDebug ? '[name]_[local]_[hash:base64:3]' : '[hash:base64:4]',
              minimize: !isDebug,
            },
          },
          {
            loader: 'postcss',
            options: {
              parser: 'sugarss',
              map: isDebug ? 'inline' : false,
              plugins: () => [
                require('postcss-smart-import'),
                require('postcss-easy-import')({ extensions: ['.pcss'] }),
                require('precss'),
                require('postcss-cssnext'),
                require('postcss-flexibility'),
                require('postcss-nested-props'),
              ],
            },
          },
        ],
      },
      {
        test: /\.(png|jpg|jpeg|gif|svg|woff|woff2)(\?.*)$/,
        use: 'url-loader?limit=10000',
      },
      {
        test: /\.(eot|ttf|svg)(\?.*)$/,
        use: 'file-loader',
      },
    ],
  },
  resolve: { extensions: ['.js', '.jsx', '.css', '.pcss'] },
}

if (!isDebug) {
  config.plugins = [
    ...config.plugins,
    new webpack.optimize.DedupePlugin(),
    new webpack.optimize.UglifyJsPlugin({ compress: { warnings: isVerbose } }),
    new webpack.optimize.AggressiveMergingPlugin(),
  ]
}

if (isDebug && useHMR) {
  babelConfig.plugins = [
    'react-hot-loader/babel',
    ...babelConfig.plugins,
  ]
  config.entry = [
    'react-hot-loader/patch',
    'webpack-hot-middleware/client',
    'eventsource-polyfill',
    ...config.entry,
  ]
  config.plugins = [
    ...config.plugins,
    new webpack.HotModuleReplacementPlugin(),
    new webpack.NoErrorsPlugin(),
  ]
}

module.exports = config