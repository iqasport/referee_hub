const { environment } = require('@rails/webpacker')

environment.splitChunks(
  config => Object.assign(
    {},
    config,
    {
      optimization: {
        splitChunks: {
          chunks: 'all',
        },
      }
    }
  )
)

environment.config.merge({
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader'
        }
      }
    ]
  }
})

module.exports = environment
