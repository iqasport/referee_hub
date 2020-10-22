import { ContentState, convertToRaw, EditorState } from 'draft-js'
import draftToHtml from 'draftjs-to-html'
import htmlToDraft from 'html-to-draftjs'

export function convertToHtml(editor: EditorState): string {
  const rawContent = convertToRaw(editor.getCurrentContent())

  return draftToHtml(rawContent)
}

export function convertToContent(htmlString: string): EditorState {
  const blocksFromHtml = htmlToDraft(htmlString)
  const { contentBlocks, entityMap } = blocksFromHtml
  const contentState = ContentState.createFromBlockArray(
    contentBlocks,
    entityMap
  )

  return EditorState.createWithContent(contentState)
}
