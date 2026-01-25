# Tournament Description Formatting

## Overview

This document outlines the approach for rendering tournament descriptions with proper formatting, including line breaks and clickable links.

## Problem Statement

Tournament descriptions are currently displayed as plain text without preserving:
- Line breaks (newlines)
- Clickable URLs
- Any other text formatting

The data is stored correctly in the database, but the display component (`TournamentAboutSection`) renders it as a simple `<p>` tag without any processing.

## Solution Approach

### Option 1: Markdown Rendering (Recommended)

Use a markdown processing library to render tournament descriptions. This provides:
- **Line break preservation**: Markdown naturally handles newlines
- **Auto-linking**: URLs are automatically converted to clickable links
- **Future extensibility**: Support for rich formatting (bold, italic, lists, etc.) without code changes
- **User-friendly**: Markdown syntax is intuitive and widely understood

**Recommended Library: `react-markdown`**
- Popular, well-maintained React library (7.5k+ stars)
- Small bundle size with tree-shaking support
- Secure by default (sanitizes HTML to prevent XSS)
- Customizable rendering
- TypeScript support

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

**Use `react-markdown` with the following configuration:**

1. **Install Dependencies:**
   ```bash
   yarn add react-markdown
   ```

2. **Update TournamentAboutSection Component:**
   ```tsx
   import ReactMarkdown from 'react-markdown';
   
   // In the component render
   <ReactMarkdown 
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

3. **Add Styling:**
   ```css
   .tournament-description {
     font-size: 0.875rem;
     color: #374151;
     line-height: 1.625;
     margin-bottom: 0.75rem;
   }
   
   .tournament-description a {
     color: #2563eb;
     text-decoration: underline;
   }
   
   .tournament-description a:hover {
     color: #1d4ed8;
   }
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

## Testing Strategy

1. Test with plain text (no markdown)
2. Test with URLs (should become clickable links)
3. Test with line breaks (should be preserved)
4. Test with markdown formatting (bold, italic, lists)
5. Test with malicious input (HTML/script tags should be escaped)
6. Test empty/null descriptions

## Migration Path

1. Add `react-markdown` dependency
2. Update `TournamentAboutSection` component
3. Add CSS styles for markdown content
4. Test with existing tournament data
5. Document markdown support in user guides (future)

## Future Enhancements

- Add markdown formatting guide in the edit modal
- Preview markdown while editing
- Support for images in descriptions
- Support for embedded videos
