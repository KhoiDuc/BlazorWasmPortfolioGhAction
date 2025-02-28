// wwwroot/js/scroll-progress.js
export function initializeScrollProgress() {
    const $ = document.querySelector.bind(document);
    const $$ = document.querySelectorAll.bind(document);
    const scroll = $('.progress');
    const content = $('.content');

    // Check for required elements
    if (!scroll) {
        console.error("Element with class 'progress' not found.");
        return;
    }
    if (!content) {
        console.error("Element with class 'content' not found.");
        return;
    }

    console.log("Scroll progress initialized for .content");

    function updateScroll() {
        const scrollTop = content.scrollTop; // Scroll position of .content
        const height = content.scrollHeight - content.clientHeight; // Scrollable height of .content
        const scrollHeight = height > 0 ? Math.round((scrollTop * 100) / height) : 0;

        console.log(`ScrollTop: ${scrollTop}, Height: ${height}, Progress: ${scrollHeight}%`);

        scroll.style.display = scrollTop > 0 ? 'flex' : 'none';
        scroll.style.background = `conic-gradient(#1A2980 ${scrollHeight}%, #26D0CE ${scrollHeight}%)`;
    }

    // Add click event listener to scroll button
    scroll.addEventListener('click', () => {
        content.scrollTop = 0; // Scroll .content to top
        const activeNav = $('.nav-item.active');
        if (activeNav) activeNav.classList.remove('active');
        const homeNav = $('.home');
        if (homeNav) homeNav.classList.add('active');
    });

    // Attach scroll event listener to .content
    content.addEventListener('scroll', () => {
        console.log("Scroll event triggered on .content");
        updateScroll();
    });

    // Initial call
    updateScroll();
}

export function initializeStarfield() {
    const $ = document.querySelector.bind(document);
    const section = $('.start_light');

    if (!section) {
        console.error("Element with class 'start_light' not found.");
        return;
    }

    console.log("Starfield initialized");

    const count = 800;
    let i = 0;

    // Clear existing stars to avoid duplicates (optional)
    section.innerHTML = '';

    while (i < count) {
        const star = document.createElement('i');
        const x = Math.floor(Math.random() * window.innerWidth);
        const y = Math.floor(Math.random() * window.innerHeight);
        const size = Math.random() * 4;
        const duration = Math.random() * 2;

        star.style.left = `${x}px`;
        star.style.top = `${y}px`;
        star.style.width = `${1 + size}px`;
        star.style.height = `${1 + size}px`;
        star.style.animationDuration = `${duration * 2}s`;
        star.style.animationDelay = `${duration}s`;

        section.appendChild(star);
        i++;
    }
}