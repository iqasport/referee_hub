# Bio Field Formatting

## Overview

This document outlines the approach for rendering referee profile bio fields with proper markdown formatting, including line break preservation, clickable links, and rich text styling.

## Problem Statement

The bio field on referee profile pages was displayed as plain text without:
- Line breaks (newlines collapsed in HTML)
- Clickable URLs (links shown as plain text)
- Any other text formatting (bold, italic, lists, etc.)

The data is stored correctly in the database, but the display component (`RefereeHeader`) rendered it as a raw string without any markdown processing.

## Solution Approach

### Option 1: Markdown Rendering (Recommended)

Use a markdown processing library to render bio content. This provides:
- **Line break preservation**: Markdown naturally handles newlines
- **Auto-linking**: URLs are automatically converted to clickable links
- **Future extensibility**: Support for rich formatting (bold, italic, lists, etc.) without code changes
- **User-friendly**: Markdown syntax is intuitive and widely understood
- **Consistency**: Matches the existing approach used for tournament descriptions

**Selected Library: `react-markdown` with `remark-gfm`**
- Already present in the project dependencies
- Popular, well-maintained React library
- Secure by default (sanitizes HTML to prevent XSS)
- Customizable rendering with component overrides
- TypeScript support
- **remark-gfm plugin**: Enables GitHub Flavored Markdown including autolink literals (converts plain URLs to clickable links)

### Option 2: Simple Text Processing

Implement custom text processing without markdown:
- Replace `\n` with `<br />` tags
- Use regex to detect URLs and wrap in `<a>` tags
- Sanitize output to prevent XSS

**Pros:**
- No additional dependencies

**Cons:**
- Reinvents the wheel when `react-markdown` is already available
- Limited formatting options
- Higher maintenance burden

### Option 3: Pre-line CSS with Link Detection

Use CSS `white-space: pre-line` to preserve line breaks and add link detection:
- Simplest implementation

**Cons:**
- Still requires URL detection logic for clickability
- Limited formatting options

## Recommended Implementation

**Use `react-markdown` with `remark-gfm` plugin (already in project dependencies):**

In `RefereeHeader.tsx`, update the `renderBio` function to use `ReactMarkdown` when not in edit mode:

```tsx
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

const renderBio = () => {
  if (!isEditing) {
    return (
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          a: (props) => (
            <a
              href={props.href}
              target="_blank"
              rel="noopener noreferrer"
              style={{ color: '#2563eb', textDecoration: 'underline' }}
            >
              {props.children}
            </a>
          ),
        }}
      >
        {user.bio ?? ''}
      </ReactMarkdown>
    );
  } else {
    return (
      <textarea
        aria-multiline="true"
        className="bg-gray-200 rounded p-4 text-lg block w-full mb-4"
        style={{ resize: "none" }}
        onChange={handleStringChange("bio")}
        value={editableUser.bio ?? ""}
        placeholder="Bio"
      />
    );
  }
};
```

## Security Considerations

- `react-markdown` sanitizes HTML by default (does not render raw HTML tags)
- Only safe markdown elements are allowed
- Links use `rel="noopener noreferrer"` to prevent tab-napping attacks
- No additional XSS protection required

## Backward Compatibility

- Existing plain text bios render correctly (no markdown syntax)
- URLs in plain text are auto-linked by `remark-gfm`
- Line breaks are preserved via markdown paragraph and line-break handling
- No database migration required

## Supported Markdown Features

The implementation supports GitHub Flavored Markdown (GFM) including:

**Text Formatting:**
- *Italic* text using `*asterisks*` or `_underscores_`
- **Bold** text using `**double asterisks**`
- Inline `code` using backticks

**Links:**
- Plain URLs automatically become clickable: `https://example.com`
- Markdown links: `[text](url)`
- All links open in a new tab with security attributes

**Lists:**
- Ordered lists (1. 2. 3.)
- Unordered lists (- or *)

**Other:**
- Headings, blockquotes, code blocks, horizontal rules

## Testing Strategy

1. Test with plain text bio (no markdown) — should render as-is
2. Test with URLs (should become clickable links)
3. Test with line breaks (should be preserved)
4. Test with markdown formatting (bold, italic, lists)
5. Test with null/empty bio
6. Test that raw HTML tags are escaped (XSS prevention)

## Migration Path

1. Dependencies already present (`react-markdown`, `remark-gfm`)
2. Update `RefereeHeader` component to use `ReactMarkdown` in view mode
3. Jest mocks for `react-markdown` and `remark-gfm` already configured in `jest.setup.ts`
4. Update `RefereeHeader` tests to cover markdown rendering

## Future Enhancements

- Add a markdown formatting guide/tooltip in the bio edit textarea
- Live preview of markdown while editing
