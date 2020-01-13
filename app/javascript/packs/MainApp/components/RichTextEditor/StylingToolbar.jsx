import React from 'react'
import PropTypes from 'prop-types'
import { Modifier, EditorState, RichUtils } from 'draft-js'
import { colorStyleMap } from '../../constants'

const StyleButton = (props) => {
  const {
    onToggle, style, active, label, index, type
  } = props

  function handleToggle(e) {
    e.preventDefault();
    onToggle(style);
  }

  let className = 'RichEditor-styleButton';
  let colorStyle = {}
  if (active) {
    className += ' RichEditor-activeButton';
    if (type === 'color') {
      colorStyle = { ...colorStyleMap[style] }
    }
  }

  return (
    <span style={colorStyle} role="button" tabIndex={index} className={className} onMouseDown={handleToggle}>
      {label}
    </span>
  )
}

StyleButton.propTypes = {
  onToggle: PropTypes.func.isRequired,
  style: PropTypes.string.isRequired,
  active: PropTypes.bool.isRequired,
  label: PropTypes.string.isRequired,
  index: PropTypes.number.isRequired,
  type: PropTypes.oneOf(['inline', 'color']).isRequired
}

const INLINE_STYLES = [
  { label: 'Bold', style: 'BOLD' },
  { label: 'Italic', style: 'ITALIC' },
  { label: 'Underline', style: 'UNDERLINE' },
  { label: 'Monospace', style: 'CODE' },
]

const InlineStyleControls = (props) => {
  const { editorState } = props
  const currentStyle = editorState.getCurrentInlineStyle()

  return (
    <div className="RichEditor-controls">
      {INLINE_STYLES.map((type, index) => (
        <StyleButton
          key={type.label}
          active={currentStyle.has(type.style)}
          label={type.label}
          onToggle={props.onToggle}
          style={type.style}
          index={index}
          type="inline"
        />
      ))}
    </div>
  )
}

InlineStyleControls.propTypes = {
  editorState: PropTypes.shape({ getCurrentInlineStyle: PropTypes.func }).isRequired,
  onToggle: PropTypes.func.isRequired,
}

const COLORS = [
  { label: 'Red', style: 'color-red' },
  { label: 'Orange', style: 'color-orange' },
  { label: 'Yellow', style: 'color-yellow' },
  { label: 'Green', style: 'color-green' },
  { label: 'Blue', style: 'color-blue' },
  { label: 'Indigo', style: 'color-indigo' },
  { label: 'Violet', style: 'color-violet' },
];

const ColorControls = (props) => {
  const { editorState, onToggle } = props
  const currentStyle = editorState.getCurrentInlineStyle();

  return (
    <div className="RichEditor-controls">
      {COLORS.map((type, index) => (
        <StyleButton
          key={type.label}
          active={currentStyle.has(type.style)}
          label={type.label}
          onToggle={onToggle}
          style={type.style}
          index={index}
          type="color"
        />
      ))}
    </div>
  );
};

ColorControls.propTypes = {
  editorState: PropTypes.shape({ getCurrentInlineStyle: PropTypes.func }).isRequired,
  onToggle: PropTypes.func.isRequired,
}

const StylingToolbar = (props) => {
  const { editorState, handleEditorChange } = props

  function handleToggleStyle(inlineStyle) {
    handleEditorChange(
      RichUtils.toggleInlineStyle(
        editorState,
        inlineStyle
      )
    );
  }

  function handleToggleColor(toggledColor) {
    const selection = editorState.getSelection();

    // Let's just allow one color at a time. Turn off all active colors.
    const nextContentState = Object.keys(colorStyleMap)
      .reduce(
        (contentState, color) => Modifier.removeInlineStyle(contentState, selection, color),
        editorState.getCurrentContent()
      );

    let nextEditorState = EditorState.push(
      editorState,
      nextContentState,
      'change-inline-style'
    );

    const currentStyle = editorState.getCurrentInlineStyle();

    // Unset style override for current color.
    if (selection.isCollapsed()) {
      nextEditorState = currentStyle.reduce(
        (state, color) => RichUtils.toggleInlineStyle(state, color),
        nextEditorState
      )
    }

    // If the color is being toggled on, apply it.
    if (!currentStyle.has(toggledColor)) {
      nextEditorState = RichUtils.toggleInlineStyle(
        nextEditorState,
        toggledColor
      );
    }

    handleEditorChange(nextEditorState);
  }

  return (
    <div>
      <InlineStyleControls onToggle={handleToggleStyle} editorState={editorState} />
      <ColorControls onToggle={handleToggleColor} editorState={editorState} />
    </div>
  )
}

StylingToolbar.propTypes = {
  editorState: PropTypes.shape({
    getCurrentInlineStyle: PropTypes.func,
    getCurrentContent: PropTypes.func,
    getSelection: PropTypes.func,
  }).isRequired,
  handleEditorChange: PropTypes.func.isRequired
}

export default StylingToolbar
