// SmartCampus - Premium UI Interactions

// ================================================
// NAVBAR SCROLL EFFECT
// ================================================
document.addEventListener('DOMContentLoaded', function () {
    const navbar = document.querySelector('.navbar-soft');
    let lastScrollPos = 0;

    window.addEventListener('scroll', function () {
        const scrollPos = window.scrollY;
        
        if (scrollPos > 50) {
            navbar?.classList.add('scrolled');
        } else {
            navbar?.classList.remove('scrolled');
        }
        
        lastScrollPos = scrollPos;
    });

    // ================================================
    // SIDEBAR TOGGLE
    // ================================================
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.sidebar-soft');
    const mainContent = document.querySelector('.with-sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            mainContent?.classList.toggle('collapsed');
        });
    }

    // ================================================
    // ACTIVE SIDEBAR LINK
    // ================================================
    const sidebarLinks = document.querySelectorAll('.sidebar-link');
    const currentPath = window.location.pathname;

    sidebarLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (currentPath.includes(href)) {
            link.classList.add('active');
        }
    });

    // ================================================
    // MODAL FUNCTIONALITY
    // ================================================
    window.openModal = function (modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.add('show');
            document.body.style.overflow = 'hidden';
        }
    };

    window.closeModal = function (modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('show');
            document.body.style.overflow = 'auto';
        }
    };

    // Close modal when clicking outside
    document.querySelectorAll('.modal-soft').forEach(modal => {
        modal.addEventListener('click', function (e) {
            if (e.target === this) {
                this.classList.remove('show');
                document.body.style.overflow = 'auto';
            }
        });
    });

    // ================================================
    // FORM VALIDATION
    // ================================================
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // ================================================
    // FILE UPLOAD DRAG & DROP
    // ================================================
    const fileInputs = document.querySelectorAll('input[type="file"]');
    fileInputs.forEach(input => {
        const parent = input.parentElement;
        if (parent) {
            parent.addEventListener('dragover', (e) => {
                e.preventDefault();
                parent.style.background = 'rgba(0, 102, 255, 0.05)';
                parent.style.borderColor = 'var(--primary-color)';
            });

            parent.addEventListener('dragleave', () => {
                parent.style.background = '';
                parent.style.borderColor = '';
            });

            parent.addEventListener('drop', (e) => {
                e.preventDefault();
                input.files = e.dataTransfer.files;
                parent.style.background = '';
                parent.style.borderColor = '';
            });
        }
    });

    // ================================================
    // FORM INPUT ANIMATIONS
    // ================================================
    const formControls = document.querySelectorAll('.form-control, .form-select');
    formControls.forEach(control => {
        control.addEventListener('focus', function () {
            this.style.boxShadow = '0 0 0 3px rgba(0, 102, 255, 0.1)';
        });

        control.addEventListener('blur', function () {
            this.style.boxShadow = '';
        });
    });

    // ================================================
    // SMOOTH TRANSITIONS FOR PAGE CHANGES
    // ================================================
    document.querySelectorAll('a[href*="/Pages/"], a[asp-page]').forEach(link => {
        link.addEventListener('click', function (e) {
            const mainContent = document.querySelector('main');
            if (mainContent && !this.getAttribute('target')) {
                mainContent.style.opacity = '0.5';
                mainContent.style.transition = 'opacity 150ms ease-in-out';
            }
        });
    });

    // ================================================
    // COUNTER ANIMATION
    // ================================================
    window.animateCounter = function (element, target, duration = 1000) {
        const start = parseInt(element.textContent) || 0;
        const increment = (target - start) / (duration / 16);
        let current = start;

        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                element.textContent = target;
                clearInterval(timer);
            } else {
                element.textContent = Math.floor(current);
            }
        }, 16);
    };

    // ================================================
    // TABLE ROW SELECTION
    // ================================================
    const selectAllCheckbox = document.getElementById('selectAll');
    const rowCheckboxes = document.querySelectorAll('input[type="checkbox"][name="selected"]');

    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function () {
            rowCheckboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
        });
    }

    rowCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function () {
            const allChecked = Array.from(rowCheckboxes).every(cb => cb.checked);
            const someChecked = Array.from(rowCheckboxes).some(cb => cb.checked);
            
            if (selectAllCheckbox) {
                selectAllCheckbox.checked = allChecked;
                selectAllCheckbox.indeterminate = someChecked && !allChecked;
            }
        });
    });

    // ================================================
    // LAZY LOAD IMAGES
    // ================================================
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    observer.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img.lazy').forEach(img => imageObserver.observe(img));
    }

    // ================================================
    // KEYBOARD SHORTCUTS
    // ================================================
    document.addEventListener('keydown', (e) => {
        // Ctrl/Cmd + K to focus search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.querySelector('[data-search]');
            if (searchInput) {
                searchInput.focus();
            }
        }

        // Escape to close modals
        if (e.key === 'Escape') {
            document.querySelectorAll('.modal-soft.show').forEach(modal => {
                modal.classList.remove('show');
            });
        }
    });

    // ================================================
    // SMOOTH SCROLL TO ANCHOR
    // ================================================
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            const target = document.querySelector(href);
            
            if (target && href !== '#') {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth' });
            }
        });
    });
});

// ================================================
// UTILITY FUNCTIONS
// ================================================

function formatDate(date) {
    return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

function formatTime(date) {
    return new Date(date).toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit'
    });
}

function truncateText(text, maxLength = 50) {
    return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function throttle(func, limit) {
    let inThrottle;
    return function (...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}
