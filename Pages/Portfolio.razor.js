let currentSlide = 0;

export function changeSlide(direction) {
    const slides = document.querySelectorAll('.carousel-image');
    slides[currentSlide].classList.remove('active');

    currentSlide += direction;
    if (currentSlide >= slides.length) currentSlide = 0;
    if (currentSlide < 0) currentSlide = slides.length - 1;

    slides[currentSlide].classList.add('active');
}

export function initializeScrollProgress() {
    const $ = document.querySelector.bind(document);
    const $$ = document.querySelectorAll.bind(document);
    const scroll = $('.progress');
    const content = $('.content');

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

    scroll.addEventListener('click', () => {
        content.scrollTop = 0; // Scroll .content to top
        const activeNav = $('.nav-item.active');
        if (activeNav) activeNav.classList.remove('active');
        const homeNav = $('.home');
        if (homeNav) homeNav.classList.add('active');
    });

    content.addEventListener('scroll', () => {
        console.log("Scroll event triggered on .content");
        updateScroll();
    });

    // Initial call
    updateScroll();
}