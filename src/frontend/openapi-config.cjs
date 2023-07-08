module.exports = {
  schemaFile: 'http://localhost:5000/swagger/v1/swagger.json',
  apiFile: './app/store/baseApi.ts',
  apiImport: 'baseApi',
  outputFile: './app/store/serviceApi.ts',
  exportName: 'serviceApi',
  hooks: true, // generate react hooks
  tag: true, // generate cache invalidation tags (Swashbuckle creates a tag per controller)
}