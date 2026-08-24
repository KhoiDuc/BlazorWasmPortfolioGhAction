(() => {
    const sectionSelector =
        "section";

    const app = document.getElementById("app");

    let initialized = false;

    function initRevealAnimations(sections) {
        const reducedMotion = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        ).matches;

        sections.forEach(section => {
            if (section.id === "home") return;
            section.classList.add("motion-reveal");
        });

        if (reducedMotion || !("IntersectionObserver" in window)) {
            sections.forEach(section => {
                section.classList.add("is-visible");
            });

            return;
        }

        const visibleThreshold = window.innerHeight * 0.9;

        sections.forEach(section => {
            const rect = section.getBoundingClientRect();

            if (rect.top < visibleThreshold && rect.bottom > 0) {
                section.classList.add("is-visible");
            }
        });

        document.documentElement.classList.add("motion-ready");

        const observer = new IntersectionObserver(
            entries => {
                entries.forEach(entry => {
                    if (!entry.isIntersecting) {
                        return;
                    }

                    entry.target.classList.add("is-visible");

                    observer.unobserve(entry.target);
                });
            },
            {
                threshold: 0.12,
                rootMargin: "0px 0px -8% 0px"
            }
        );

        sections.forEach(section => {
            if (!section.classList.contains("is-visible")) {
                observer.observe(section);
            }
        });
    }

    function initStaggerAnimations() {
        const reducedMotion = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        ).matches;

        if (reducedMotion || !("IntersectionObserver" in window)) {
            return;
        }

        const groups = [
            {
                itemSelector: ".experience-item",
                delayStep: 90
            },
            {
                itemSelector: ".project-card",
                delayStep: 90
            },
            {
                itemSelector: ".technology-group",
                delayStep: 60
            },
            {
                itemSelector: ".education-item",
                delayStep: 80
            },
            {
                itemSelector: ".nav-group",
                delayStep: 50
            },
            {
                itemSelector: ".crypto-card-list > *",
                delayStep: 60
            }
        ];

        groups.forEach(group => {
            const items = [
                ...document.querySelectorAll(group.itemSelector)
            ];

            if (items.length === 0) {
                return;
            }

            items.forEach((item, index) => {
                item.classList.add("motion-stagger");

                item.style.setProperty(
                    "--stagger-delay",
                    `${index * group.delayStep}ms`
                );
            });

            const triggerSection = items[0].closest("section") || items[0].parentElement;

            if (!triggerSection) return;

            const observer = new IntersectionObserver(
                entries => {
                    const entry = entries[0];

                    if (!entry.isIntersecting) {
                        return;
                    }

                    items.forEach(item => {
                        item.classList.add("is-stagger-visible");
                    });

                    observer.disconnect();
                },
                {
                    threshold: 0.12,
                    rootMargin: "0px 0px -8% 0px"
                }
            );

            observer.observe(triggerSection);
        });
    }

    function initActiveNavigation() {
        const navLinks = [
            ...document.querySelectorAll('#main-navigation a')
        ].filter(a => a.getAttribute('href'));

        if (navLinks.length === 0) return;

        const sections = [];
        navLinks.forEach(link => {
            const href = link.getAttribute('href');
            let section = null;
            if (href.startsWith('#')) {
                section = document.querySelector(href);
            } else if (href === '' || href === '/') {
                section = document.querySelector('#home') || document.querySelector('section');
            } else {
                const id = href.replace(/[^a-zA-Z0-9-]/g, '');
                section = document.getElementById(id) || document.querySelector(`[data-nav="${href}"]`);
            }
            if (section) sections.push({ link, section });
        });

        if (sections.length === 0) return;

        let ticking = false;

        function updateActiveSection() {
            ticking = false;
            const marker = window.innerHeight * 0.35;
            let activeItem = null;

            sections.forEach(item => {
                const rect = item.section.getBoundingClientRect();
                if (rect.top <= marker && rect.bottom > 72) activeItem = item;
            });

            const isAtBottom = window.scrollY + window.innerHeight >= document.documentElement.scrollHeight - 4;
            if (isAtBottom) activeItem = sections[sections.length - 1];

            sections.forEach(item => {
                const isActive = item === activeItem;
                item.link.classList.toggle('active', isActive);
                if (isActive) item.link.setAttribute('aria-current', 'location');
                else item.link.removeAttribute('aria-current');
            });
        }

        function requestUpdate() {
            if (ticking) return;
            ticking = true;
            requestAnimationFrame(updateActiveSection);
        }

        window.addEventListener('scroll', requestUpdate, { passive: true });
        window.addEventListener('resize', requestUpdate);
        updateActiveSection();
    }

    function initScrollBackdrop() {
        let ticking = false;

        function updateScroll() {
            ticking = false;
            document.body.classList.toggle('is-scrolled', window.scrollY > 12);
        }

        function requestUpdate() {
            if (ticking) return;
            ticking = true;
            requestAnimationFrame(updateScroll);
        }

        window.addEventListener('scroll', requestUpdate, { passive: true });
        updateScroll();
    }

    function initHeroParallax() {
        const hero = document.querySelector(".hero");

        if (!hero) {
            return;
        }

        const reducedMotion = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        );

        const mobileViewport = window.matchMedia(
            "(max-width: 768px)"
        );

        let ticking = false;

        function updateParallax() {
            ticking = false;

            if (
                reducedMotion.matches ||
                mobileViewport.matches
            ) {
                hero.style.setProperty(
                    "--pattern-parallax",
                    "0px"
                );

                return;
            }

            const rect = hero.getBoundingClientRect();

            if (rect.bottom <= 0) {
                return;
            }

            const progress = Math.min(
                Math.max(
                    -rect.top / Math.max(rect.height, 1),
                    0
                ),
                1
            );

            const offset = progress * 28;

            hero.style.setProperty(
                "--pattern-parallax",
                `${offset.toFixed(2)}px`
            );
        }

        function requestUpdate() {
            if (ticking) {
                return;
            }

            ticking = true;

            requestAnimationFrame(updateParallax);
        }

        window.addEventListener(
            "scroll",
            requestUpdate,
            { passive: true }
        );

        window.addEventListener(
            "resize",
            requestUpdate
        );

        reducedMotion.addEventListener(
            "change",
            requestUpdate
        );

        mobileViewport.addEventListener(
            "change",
            requestUpdate
        );

        updateParallax();
    }

    function initializePortfolioMotion() {
        if (initialized) {
            return true;
        }

        const sections = [
            ...document.querySelectorAll(sectionSelector)
        ];

        if (sections.length === 0) {
            return false;
        }

        initRevealAnimations(sections);
        initStaggerAnimations();
        initActiveNavigation();
        initHeroParallax();
        initScrollBackdrop();

        initialized = true;

        return true;
    }

    function reinitializePortfolioMotion() {
        initialized = false;

        document.querySelectorAll(".motion-reveal").forEach(el => {
            el.classList.remove("motion-reveal", "is-visible");
        });

        document.querySelectorAll(".motion-stagger").forEach(el => {
            el.classList.remove("motion-stagger", "is-stagger-visible");
            el.style.removeProperty("--stagger-delay");
        });

        initializePortfolioMotion();
    }

    if (!initializePortfolioMotion()) {
        const renderObserver = new MutationObserver(() => {
            if (initializePortfolioMotion()) {
                renderObserver.disconnect();
            }
        });

        renderObserver.observe(app, {
            childList: true,
            subtree: true
        });
    }

    let lastUrl = location.href;
    new MutationObserver(() => {
        const url = location.href;
        if (url !== lastUrl) {
            lastUrl = url;
            reinitializePortfolioMotion();
        }
    }).observe(document, { subtree: true, childList: true });
})();