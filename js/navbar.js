/**
 * =============================================================================
 * NAVBAR & LAYOUT JS HELPERS — navbar.js
 * Cleaned & simplified navigation helpers
 * =============================================================================
 */

// Legacy helper stubs to prevent JS runtime exceptions on cached/legacy calls
function initNavbar() {
    // Navigation is now handled by Blazor state in MainLayout.razor
}

function initNavbarMobile() {
    // Mobile navigation closing is handled via EventCallback in Blazor
}

function fadeOut() {
    // Handled by CSS transitions
}

function initScrollingMenu(containerSelector, navSelector) {
    // No-op: replaced with native CSS overflow-y auto for better performance and touch support
}

window.initNavbar = initNavbar;
window.initNavbarMobile = initNavbarMobile;
window.fadeOut = fadeOut;
window.initScrollingMenu = initScrollingMenu;