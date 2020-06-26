/* eslint-disable react/prop-types */
import React from 'react'

const RichTextEditor = ({ onChange, value }) => (
  <textarea onChange={onChange} data-testid="text-editor">{value}</textarea>
)

export default RichTextEditor
