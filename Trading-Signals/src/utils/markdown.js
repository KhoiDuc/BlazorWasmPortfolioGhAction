/**
 * A lightweight, high-fidelity custom Markdown-to-HTML parser.
 * Supports headings, bold/italics, horizontal lines, code blocks, lists, and Bootstrap-styled responsive tables.
 */

export function parseMarkdown(text) {
  if (!text) return '';

  // Escape HTML characters to prevent XSS but keep safe replacements
  let html = text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');

  // Replace LaTeX arrows and common math/markdown arrow representations
  html = html
    .replace(/\$?\\?leftrightarrow\$?/gi, ' ↔ ')
    .replace(/\$?\\?Leftarrow\$?/g, ' ⇐ ')
    .replace(/\$?\\?Rightarrow\$?/g, ' ⇒ ')
    .replace(/\$?\\?i?r[ieg]{1,2}h?t_?arrow\$?/gi, ' → ')
    .replace(/(?:\\to\b|\$to\$)/gi, ' → ')
    .replace(/\$?\\?l[ieg]{1,2}f?t_?arrow\$?/gi, ' ← ')
    .replace(/\$?\\?uparrow\$?/gi, ' ↑ ')
    .replace(/\$?\\?downarrow\$?/gi, ' ↓ ');

  // Split text by lines to parse structural elements (tables, lists, headers)
  const lines = html.split('\n');
  let inTable = false;
  let currentTable = [];
  const parsedLines = [];

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim();

    // 1. Table parsing
    if (line.startsWith('|') && line.endsWith('|')) {
      if (!inTable) {
        inTable = true;
        currentTable = [];
      }
      
      // Extract cells
      const cells = line
        .split('|')
        .map(c => c.trim())
        .filter((_, idx, arr) => idx > 0 && idx < arr.length - 1);

      // Check if this is a divider line (e.g. |:---|:---|)
      const isDivider = cells.every(c => /^:?-+:?$/.test(c));
      if (isDivider) {
        continue; // Skip separator row in final HTML output
      }

      currentTable.push(cells);
    } else {
      // If we were parsing a table and the line is no longer part of the table
      if (inTable) {
        parsedLines.push(renderTableHtml(currentTable));
        inTable = false;
      }
      parsedLines.push(lines[i]); // Keep original formatting (with leading spaces if needed)
    }
  }

  // If table remains at the end
  if (inTable) {
    parsedLines.push(renderTableHtml(currentTable));
  }

  // Join parsed lines back together
  html = parsedLines.join('\n');

  // 2. Block Code Blocks: ```code```
  html = html.replace(/```([\s\S]*?)```/g, '<pre class="custom-code-block"><code>$1</code></pre>');

  // 3. Headers
  html = html.replace(/^###### (.*?)$/gm, '<h6 class="ai-header-h6">$1</h6>');
  html = html.replace(/^##### (.*?)$/gm, '<h6 class="ai-header-h6">$1</h6>');
  html = html.replace(/^#### (.*?)$/gm, '<h6 class="ai-header-h6">$1</h6>');
  html = html.replace(/^### (.*?)$/gm, '<h5 class="ai-header-h5">$1</h5>');
  html = html.replace(/^## (.*?)$/gm, '<h4 class="ai-header-h4">$1</h4>');
  html = html.replace(/^# (.*?)$/gm, '<h3 class="ai-header-h3">$1</h3>');

  // 4. Horizontal rules
  html = html.replace(/^---$/gm, '<hr class="ai-hr" />');

  // 5. Lists (unordered lists)
  // Let's replace list items with custom styled `li` tags
  html = html.replace(/^\*\s+(.*?)$/gm, '<li class="ai-list-item">$1</li>');
  html = html.replace(/^-\s+(.*?)$/gm, '<li class="ai-list-item">$1</li>');

  // 6. Inline elements (Bold, Italic, Inline Code)
  html = parseInlineElements(html);

  // 7. Line breaks
  // Avoid doubling up on line breaks for table and pre tags
  html = html.replace(/\n/g, '<br/>');
  
  // Clean up line breaks before/after custom containers to avoid excessive spaces
  html = html.replace(/<\/table><\/div><br\/>/g, '</table></div>');
  html = html.replace(/<\/pre><br\/>/g, '</pre>');
  html = html.replace(/<\/h[3-5]><br\/>/g, (match) => match.replace('<br/>', ''));
  html = html.replace(/<hr class="ai-hr" \/><br\/>/g, '<hr class="ai-hr" />');
  
  // Fix nested lists line breaks if any
  html = html.replace(/<\/li><br\/>/g, '</li>');

  return html;
}

function renderTableHtml(rows) {
  if (rows.length === 0) return '';
  let html = '<div class="table-responsive my-3"><table class="table table-bordered table-striped custom-ai-table">';
  
  for (let r = 0; r < rows.length; r++) {
    const cells = rows[r];
    html += '<tr>';
    for (let c = 0; c < cells.length; c++) {
      const cellContent = parseInlineElements(cells[c]);
      if (r === 0) {
        html += `<th class="table-dark">${cellContent}</th>`;
      } else {
        html += `<td>${cellContent}</td>`;
      }
    }
    html += '</tr>';
  }
  
  html += '</table></div>';
  return html;
}

function parseInlineElements(text) {
  return text
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.*?)\*/g, '<em>$1</em>')
    .replace(/`(.*?)`/g, '<code class="custom-code-inline">$1</code>');
}
