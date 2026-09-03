window.app = {
    page: null,

    createObjectUrl: function (fileInput) {
        if (fileInput.files.length > 0) {
            var file = fileInput.files[0];
            var url = window.URL.createObjectURL(file);
            if (window.app.page) {
                window.app.page.invokeMethodAsync('SetBlobUrl', url);
            }
        }
    },

    showFileSelector: function (elementId) {
        var fileSelector = document.getElementById(elementId);
        if (fileSelector) {
            fileSelector.click();
        }
    },

    registerPage: function (pageRef) {
        window.app.page = pageRef;
    }
};

window.skipToContent = function () {
    var main = document.getElementById('main-content');
    if (main) {
        main.focus();
        main.scrollIntoView();
    }
};

function getUserAgent() {
    return navigator.userAgent;
}

window.getUserAgent = getUserAgent;
window.clipboardCopy = {
    copyText: function (text) {
        navigator.clipboard.writeText(text).then(function () {
            console.log(text);
        })
            .catch(function (error) {
                alert(error);
            });
    }
};
window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
function initializeScrollProgress() {
    const $ = document.querySelector.bind(document);
    const scroll = $('.progress');

    if (!scroll) {
        return;
    }

    function updateScroll() {
        const scrollTop = window.scrollY || document.documentElement.scrollTop;
        const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
        const scrollHeight = height > 0 ? Math.round((scrollTop * 100) / height) : 0;

        scroll.style.display = scrollTop > 0 ? 'flex' : 'none';
        scroll.style.background = `conic-gradient(var(--color-brand-primary, #3498db) ${scrollHeight}%, var(--color-bg-surface, #d9534f) ${scrollHeight}%)`;
    }

    scroll.addEventListener('click', () => {
        window.scrollTo({ top: 0, left: 0, behavior: 'smooth' });
    });

    window.addEventListener('scroll', updateScroll, { passive: true });
    updateScroll();
}

function initializeStarfield() {
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

window.carouselFunctions = {
    initializeCarousel: function () {
        let currentSlide = 0;
        const slides = document.querySelectorAll('.carousel-image');

        return {
            changeSlide: function (direction) {
                if (slides.length === 0) return;

                slides[currentSlide].classList.remove('active');

                currentSlide += direction;
                if (currentSlide >= slides.length) currentSlide = 0;
                if (currentSlide < 0) currentSlide = slides.length - 1;

                slides[currentSlide].classList.add('active');
            }
        };
    }
};

window.scrollToSection = function (sectionId) {
    const el = document.getElementById(sectionId);
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

window.scrollToTop = function () {
    const behavior = window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth';
    const home = document.getElementById('home');

    if (home) {
        home.scrollIntoView({ behavior, block: 'start' });
        return;
    }

    window.scrollTo({ top: 0, left: 0, behavior });
    document.documentElement.scrollTop = 0;
    document.body.scrollTop = 0;
};