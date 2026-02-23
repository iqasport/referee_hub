# Tournament Description Formatting

## Overview

This document outlines the approach for rendering tournament descriptions with proper markdown formatting, including line breaks, clickable links, styled headings, lists, and other rich text elements.

## Problem Statement

Tournament descriptions were displayed as plain text without:
- Line breaks (newlines)
- Clickable URLs
- Visual styling for headings
- Proper formatting for lists
- Any other text formatting

The data was stored correctly in the database, but the display component (`TournamentAboutSection`) rendered it without any markdown processing or styling.

## Solution Approach

### Option 1: Markdown Rendering (Recommended)

Use a markdown processing library to render tournament descriptions. This provides:
- **Line break preservation**: Markdown naturally handles newlines
- **Auto-linking**: URLs are automatically converted to clickable links
- **Future extensibility**: Support for rich formatting (bold, italic, lists, etc.) without code changes
- **User-friendly**: Markdown syntax is intuitive and widely understood

**Recommended Library: `react-markdown` with `remark-gfm`**
- Popular, well-maintained React library (7.5k+ stars)
- Small bundle size with tree-shaking support
- Secure by default (sanitizes HTML to prevent XSS)
- Customizable rendering
- TypeScript support
- **remark-gfm plugin**: Enables GitHub Flavored Markdown including autolink literals (converts plain URLs to clickable links)

**Alternative: `marked` + `DOMPurify`**
- More control over rendering
- Requires additional sanitization setup
- Larger implementation footprint

### Option 2: Simple Text Processing

Implement custom text processing without markdown:
- Replace `\n` with `<br />` tags
- Use regex to detect URLs and wrap in `<a>` tags
- Sanitize output to prevent XSS

**Pros:**
- No external dependencies
- Lighter weight

**Cons:**
- Limited to basic formatting
- More maintenance burden
- Reinventing the wheel
- Harder to extend in the future

### Option 3: Pre-line CSS with Link Detection

Use CSS `white-space: pre-line` to preserve line breaks and add link detection:
- Simplest implementation
- Minimal code changes
- Still requires URL detection logic for clickability

**Cons:**
- Limited formatting options
- Still needs link parsing logic

## Recommended Implementation

**Use `react-markdown` with `remark-gfm` plugin for autolink support:**

1. **Install Dependencies:**
   ```bash
   yarn add react-markdown remark-gfm
   ```

2. **Update TournamentAboutSection Component:**
   {% raw %}
   ```tsx
   import ReactMarkdown from 'react-markdown';
   import remarkGfm from 'remark-gfm';
   
   // In the component render
   <ReactMarkdown 
     remarkPlugins={[remarkGfm]}
     className="tournament-description"
     components={{
       // Customize link rendering to open in new tab
       a: ({ node, ...props }) => (
         <a {...props} target="_blank" rel="noopener noreferrer" />
       )
     }}
   >
     {description || 'No description provided.'}
   </ReactMarkdown>
   ```
   {% endraw %}

3. **Add Styling via Component Renderers:**
   
   Custom component renderers are used to apply inline styles to all markdown elements. This approach:
   - Ensures consistent rendering without CSS conflicts
   - Keeps the component self-contained
   - Avoids specificity issues
   
   Key styles:
   - **Headings**: Progressive sizing from h1 (1rem) to h6 (0.75rem), all smaller than the "Description" header (1.125rem)
   - **Lists**: 1.5rem left padding for indentation, proper list-style-type
   - **Emphasis/Strong**: Explicit italic and bold styles
   - **Code**: Gray background, monospace font, padding
   - **Blockquotes**: Left border, italic, indented
   
   Example:
   ```tsx
   h1: ({ children }) => <h1 style={{ fontSize: '1rem', fontWeight: 'bold', color: '#111827', marginTop: '1rem', marginBottom: '0.5rem' }}>{children}</h1>
   ```

## Security Considerations

- `react-markdown` sanitizes HTML by default (doesn't render HTML tags)
- Only allows safe markdown elements
- Links should use `rel="noopener noreferrer"` for security
- No additional XSS protection needed

## Backward Compatibility

- Existing plain text descriptions will render correctly
- URLs in plain text will be auto-linked
- Line breaks will be preserved
- No database migration required

## Supported Markdown Features

The implementation supports full GitHub Flavored Markdown (GFM) including:

**Text Formatting:**
- *Italic* text using `*asterisks*` or `_underscores_`
- **Bold** text using `**double asterisks**` or `__double underscores__`
- Inline `code` using backticks

**Headings:**
- # H1 through ###### H6
- Properly sized with visual hierarchy
- h1 (1rem) is slightly smaller than the "Description" section header (1.125rem)

**Lists:**
- Ordered lists with numbers (1. 2. 3.)
- Unordered lists with bullets (- or *)
- Nested lists supported
- Proper indentation (1.5rem)

**Links:**
- Plain URLs automatically become clickable: `https://example.com`
- Markdown links: `[text](url)`
- All links open in new tab with security attributes

**Other:**
- Blockquotes using `>`
- Horizontal rules using `---` or `***`
- Code blocks using triple backticks

## Testing Strategy

1. Test with plain text (no markdown)
2. Test with URLs (should become clickable links)
3. Test with line breaks (should be preserved)
4. Test with markdown formatting (bold, italic, lists)
5. Test with malicious input (HTML/script tags should be escaped)
6. Test empty/null descriptions

## Migration Path

1. Add `react-markdown` and `remark-gfm` dependencies
2. Update `TournamentAboutSection` component
3. Add CSS styles for markdown content
4. Update Jest configuration to mock react-markdown and remark-gfm
5. Test with existing tournament data
6. Document markdown support in user guides (future)

## Future Enhancements

- Add markdown formatting guide in the edit modal
- Preview markdown while editing
- Support for images in descriptions
- Support for embedded videos
