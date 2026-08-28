# WCAG 2.2 AA Audit Checklist

Use this when creating or reviewing any `.razor` component.

## Per-Component Checklist

### Structure & Landmarks
- [ ] Page has exactly one `<h1>`
- [ ] Heading levels do not skip (no h1 → h3)
- [ ] Landmarks present: `<header>`, `<main id="main-content">`, `<nav>`, `<footer>`
- [ ] Skip-link is first focusable element (in `MainLayout.razor`)

### Buttons & Links
- [ ] Icon-only buttons have `aria-label` (localized)
- [ ] Toggle buttons have `aria-expanded` + `aria-controls`
- [ ] All buttons use `type="button"` (non-submit)
- [ ] Touch targets >= 44x44 CSS px

### Icons & Images
- [ ] Decorative `<i>`/`<svg>` have `aria-hidden="true"`
- [ ] Meaningful icons have `role="img"` + `aria-label`
- [ ] Decorative images: `alt=""`
- [ ] Meaningful images: descriptive `alt`

### Forms
- [ ] Every input has associated `<label for="id">`
- [ ] Required fields: `aria-required="true"`
- [ ] Errors: `aria-invalid="true"` + `aria-describedby` → error text with `role="alert"`
- [ ] Hints: linked via `aria-describedby`

### Dynamic Content
- [ ] Loading: `role="status"` + `aria-live="polite"`
- [ ] Errors: `role="alert"`
- [ ] Chat/streaming: `aria-live="polite"` + `aria-relevant="additions text"`

### Focus & Keyboard
- [ ] `:focus-visible` outline visible (global style in `app.css`)
- [ ] No `outline: none` without replacement
- [ ] No positive `tabindex`
- [ ] Modal: focus trap + return focus on close
- [ ] Tab order follows DOM order

### Color & Contrast
- [ ] Text contrast >= 4.5:1 (normal text)
- [ ] Large text (>= 18.66px bold or 24px) contrast >= 3:1
- [ ] UI components/borders >= 3:1 (SC 1.4.11)
- [ ] Color is not the sole indicator of state

### Motion
- [ ] Non-essential animation wrapped in `@media (prefers-reduced-motion: no-preference)`
- [ ] `@media (prefers-reduced-motion: reduce)` overrides in `app.css`

### Language
- [ ] `<html lang>` set (handled by `index.html` + `culture.js`)
- [ ] Foreign-language inline content wrapped in `<span lang="...">`

## Responsive Checklist

Test at: 375px (mobile) | 768px (tablet) | 1024px (desktop) | 1280px (large)

- [ ] No horizontal scroll at any breakpoint
- [ ] No fixed `width` without `max-width: 100%` fallback
- [ ] `@media` queries use token-aligned values (768px/1024px/1280px)
- [ ] Fluid spacing uses `clamp()`/`min()`
- [ ] Mobile-first: base = mobile, `min-width` to enhance

## Quick Smoke Test

1. Tab from URL bar → skip-link should appear
2. Enter on skip-link → focus jumps to `<main>`
3. Tab through page → logical order, visible focus ring
4. Resize to 375px → layout reflows, no overflow
5. Run Axe DevTools → 0 violations at AA level