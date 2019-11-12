const { environment } = require('@rails/webpacker')
const dotenv = require('dotenv');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

const dotenvFiles = [
  `.env.${process.env.NODE_ENV}.local`,
  '.env.local',
  `.env.${process.env.NODE_ENV}`,
  '.env'
]

dotenvFiles.forEach((dotenvFile) => {
  dotenv.config({ path: dotenvFile, silent: true })
})

environment.optimization = {
  minimizer: [new UglifyJsPlugin({ sourceMap: true })]
}

module.exports = environment
