/**
 * Page Transition Utilities
 * Provides smooth animations when navigating between pages
 */

window.NERA = window.NERA || {};
NERA.PageTransitions = {
    // Initialize page transitions
    init: function() {
        this.handlePageLoad();
        this.handleLinkClicks();
        this.handleFormSubmissions();
    },

    // Handle page load animation
    handlePageLoad: function() {
        document.addEventListener('DOMContentLoaded', () => {
            const mainContent = document.querySelector('.main-content');
            if (mainContent) {
                mainContent.classList.add('page-loaded');
            }

            // Add transition class to body
            document.body.classList.add('page-transition-container');

            // Animate sections
            const sections = document.querySelectorAll('section, .section');
            sections.forEach((section, index) => {
                section.style.animationDelay = `${(index + 1) * 0.1}s`;
            });

            // Animate cards and grid items
            const cards = document.querySelectorAll('.card, .bg-white.border');
            cards.forEach((card, index) => {
                card.style.animationDelay = `${(index + 1) * 0.05}s`;
            });
        });
    },

    // Handle link clicks with smooth transitions
    handleLinkClicks: function() {
        document.addEventListener('click', (e) => {
            const link = e.target.closest('a');
            if (!link) return;

            // Skip if it's an external link, anchor link, or has special attributes
            if (
                link.hostname !== window.location.hostname ||
                link.hash ||
                link.hasAttribute('download') ||
                link.hasAttribute('target') ||
                link.getAttribute('href')?.startsWith('javascript:') ||
                link.getAttribute('href')?.startsWith('mailto:') ||
                link.getAttribute('href')?.startsWith('tel:')
            ) {
                return;
            }

            // Add click animation
            link.style.transform = 'scale(0.98)';
            setTimeout(() => {
                link.style.transform = '';
            }, 150);

            // Add fade out effect before navigation
            const mainContent = document.querySelector('.main-content');
            if (mainContent) {
                mainContent.style.opacity = '0.7';
                mainContent.style.transition = 'opacity 0.2s ease-out';
            }
        });
    },

    // Handle form submissions with smooth transitions
    handleFormSubmissions: function() {
        document.addEventListener('submit', (e) => {
            const form = e.target;
            if (!form || form.tagName !== 'FORM') return;

            // Add loading state
            const submitButton = form.querySelector('button[type="submit"], input[type="submit"]');
            if (submitButton) {
                submitButton.style.opacity = '0.7';
                submitButton.style.transform = 'scale(0.98)';
                submitButton.disabled = true;
            }

            // Add subtle fade to form
            form.style.opacity = '0.9';
            form.style.transition = 'opacity 0.2s ease-out';
        });
    },

    // Animate element entrance
    animateElement: function(element, animation = 'fadeIn', delay = 0) {
        if (!element) return;

        element.style.animation = `${animation} 0.5s ease-out ${delay}s backwards`;
        element.style.opacity = '0';

        setTimeout(() => {
            element.style.opacity = '1';
        }, delay * 1000);
    },

    // Smooth scroll to element
    scrollToElement: function(selector, offset = 0) {
        const element = document.querySelector(selector);
        if (!element) return;

        const elementPosition = element.getBoundingClientRect().top;
        const offsetPosition = elementPosition + window.pageYOffset - offset;

        window.scrollTo({
            top: offsetPosition,
            behavior: 'smooth'
        });
    }
};

// Initialize on page load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        NERA.PageTransitions.init();
    });
} else {
    NERA.PageTransitions.init();
}

