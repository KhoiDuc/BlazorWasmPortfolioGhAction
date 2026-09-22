# FontView

A web-based font viewer and character map tool for exploring, previewing, and copying font characters in real time.

![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)
![Platform](https://img.shields.io/badge/platform-Web-brightgreen.svg)

---

## Overview

FontView is a client-side web application that allows users to load font files and inspect every character they contain. It renders a complete character map organized by Unicode categories, making it easy to find, preview, and copy any glyph — including Unicode symbols, hexadecimal codes, emojis, and other special characters.

All processing happens locally in the browser. No files are uploaded to any server.

---

## Features

### Font Loading
- Drag-and-drop or file picker upload
- Supports **TTF**, **OTF**, **WOFF**, and **WOFF2** formats
- Instant font parsing and rendering powered by [opentype.js](https://github.com/opentypejs/opentype.js)
- Quick-access upload popup when a font is already loaded

### Character Map
- Full glyph grid organized by Unicode category
- Categories include Uppercase, Lowercase, Numbers, Punctuation, Accented, Greek, Cyrillic, Arabic, Dingbats, Private Use, and many more
- Collapsible sections for each category
- Click any character to copy it to the clipboard instantly

### Search
- Search by character name, Unicode code point, hex value, or category
- Supports queries like `U+0041`, `0041`, `A`, or `Mayúsculas`
- Results grouped by category with count badges
- Keyboard navigation with arrow keys, Enter to copy, Escape to close
- Positioned in the top-left corner for quick access

### Customization
- **Color picker** — change the display color of all characters in real time
- **Size slider** — adjust character size from 18px to 120px
- **Sort options** — view characters by Category, A→Z, Z→A, Unicode ascending, or Unicode descending

### Sticky Section Navigation
- Category pill navigation bar below the toolbar
- Automatically sticks to the header when scrolling past the original position
- Scroll spy highlights the active category as you scroll through sections
- Click any pill to jump to that section

### Font Information
- Displays font name, subfamily, character count, and file size
- Detail modal shows creator, creation date, and license information
- Preview panel renders sample text in the loaded font

### Settings
- **Theme** — Light and Dark mode with smooth animated transitions and visual preview cards
- **Cache** — Clear all saved preferences or toggle font preloading
- **Preferences** — Set default sort order, character color, and character size (persisted in localStorage)
- **Language** — Switch between English and Spanish with full UI translation
- **About** — Application description, author credit, and license information

### Internationalization
- Full i18n support for **English** and **Spanish**
- All UI labels, placeholders, tooltips, and messages are translated
- Language preference is saved and restored automatically

### Context Menu
- Custom right-click menu with **Copy**, **Share**, and **Print** options
- Clean white design with smooth entrance animation

### Keyboard Shortcuts

| Shortcut       | Action            |
|----------------|-------------------|
| `/`            | Open search       |
| `Ctrl+K`       | Open search       |
| `↑` `↓`        | Navigate results  |
| `Enter`        | Copy character    |
| `Escape`       | Close modal       |

### Responsive Design
- Fully responsive from desktop to mobile
- Touch-friendly character cards and navigation
- Adaptive toolbar and settings layout for small screens
- Drag-to-scroll on the category navigation bar

---

## Supported File Formats

| Format   | Extension |
|----------|-----------|
| TrueType | `.ttf`    |
| OpenType | `.otf`    |
| WOFF     | `.woff`   |
| WOFF2    | `.woff2`  |

---

## Getting Started

### Requirements

- A modern web browser (Chrome, Firefox, Safari, Edge)
- No server or build tools required

### Installation

Unlike other projects, FontView does not manually require an installation since you can run it from the cloud, Enjoy!
