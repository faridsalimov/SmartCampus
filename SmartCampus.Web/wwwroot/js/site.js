document.addEventListener('DOMContentLoaded', function () {
    window.SmartCampusModals = {
        showAlert: function(modalId, options = {}) {
            const modal = document.getElementById(modalId);
            if (!modal) {
                console.warn(`Modal "${modalId}" not found`);
                return;
            }

            if (options.title) {
                const titleEl = modal.querySelector('.modal-title');
                if (titleEl) titleEl.textContent = options.title;
            }

            if (options.message) {
                const messageEl = modal.querySelector('#alertMessage');
                if (messageEl) messageEl.textContent = options.message;
            }

            if (options.actionUrl) {
                const form = modal.querySelector('#alertForm');
                if (form) form.action = options.actionUrl;
            }

            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        },

        showForm: function(modalId, options = {}) {
            const modal = document.getElementById(modalId);
            if (!modal) {
                console.warn(`Modal "${modalId}" not found`);
                return;
            }

            if (options.title) {
                const titleEl = modal.querySelector('.modal-title');
                if (titleEl) {
                    titleEl.innerHTML = `<i class="fas ${options.icon || 'fa-edit'}"></i> ${options.title}`;
                }
            }

            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        },

        showContent: function(modalId, options = {}) {
            const modal = document.getElementById(modalId);
            if (!modal) {
                console.warn(`Modal "${modalId}" not found`);
                return;
            }

            if (options.title) {
                const titleEl = modal.querySelector('.modal-title');
                if (titleEl) {
                    titleEl.innerHTML = `<i class="fas ${options.icon || 'fa-file-alt'}"></i> ${options.title}`;
                }
            }

            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        },

        close: function(modalId) {
            const modal = document.getElementById(modalId);
            if (modal) {
                const bsModal = bootstrap.Modal.getInstance(modal);
                if (bsModal) bsModal.hide();
            }
        },

        confirmDelete: function(modalId, options = {}) {
            const modal = document.getElementById(modalId);
            if (!modal) {
                console.warn(`Modal "${modalId}" not found`);
                return;
            }

            const entity = options.entityName || 'this item';
            const message = `Are you sure you want to delete ${entity}? This action cannot be undone.`;

            this.showAlert(modalId, {
                title: '⚠️ Confirm Deletion',
                message: message,
                actionUrl: options.actionUrl || '#'
            });
        }
    };

    window.openModal = function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) new bootstrap.Modal(modal).show();
    };

    window.closeModal = function(modalId) {
        window.SmartCampusModals.close(modalId);
    };

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

    const formControls = document.querySelectorAll('.form-control, .form-select');
    formControls.forEach(control => {
        control.addEventListener('focus', function () {
            this.style.boxShadow = '0 0 0 3px rgba(0, 102, 255, 0.1)';
        });

        control.addEventListener('blur', function () {
            this.style.boxShadow = '';
        });
    });

    document.querySelectorAll('a[href*="/Pages/"], a[asp-page]').forEach(link => {
        link.addEventListener('click', function (e) {
            const mainContent = document.querySelector('main');
            if (mainContent && !this.getAttribute('target')) {
                mainContent.style.opacity = '0.5';
                mainContent.style.transition = 'opacity 150ms ease-in-out';
            }
        });
    });

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

    document.addEventListener('keydown', (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.querySelector('[data-search]');
            if (searchInput) {
                searchInput.focus();
            }
        }
    });

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

    document.addEventListener('click', function (e) {
        const btn = e.target.closest && e.target.closest('[data-delete-url]');
        if (!btn) return;

        const url = btn.getAttribute('data-delete-url');
        const entityId = btn.getAttribute('data-delete-entity');
        const message = btn.getAttribute('data-delete-message') || 'Are you sure you want to delete this item?';

        const modal = document.getElementById('confirmModal');
        if (!modal) return;

        const msgEl = modal.querySelector('#confirmModalMessage');
        if (msgEl) msgEl.textContent = message;

        const form = modal.querySelector('#confirmModalForm');
        if (form) {
            form.action = url || '#';

            
            let existing = form.querySelector('input[name="id"]');
            if (existing) existing.value = entityId || '';
            else if (entityId) {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = 'id';
                input.value = entityId;
                form.appendChild(input);
            }
        }
    });

    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (form && form.id === 'confirmModalForm') {
            e.preventDefault();

            const action = form.action || '#';
            const formData = new FormData(form);

            fetch(action, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin'
            })
            .then(function (response) {
                
                const modalEl = document.getElementById('confirmModal');
                const modalInstance = bootstrap.Modal.getInstance(modalEl);
                if (modalInstance) modalInstance.hide();

                if (response.ok) {
                    
                    if (window.Toast) {
                        window.Toast.success('Deleted successfully.');
                    }
                    setTimeout(function () { 
                        window.location.reload(); 
                    }, 900);
                } else {
                    
                    if (response.status === 401) {
                        if (window.Toast) {
                            window.Toast.error('You do not have permission to delete this item.');
                        }
                    } else if (response.status === 404) {
                        if (window.Toast) {
                            window.Toast.error('Item not found.');
                        }
                    } else {
                        return response.text().then(function (text) {
                            console.error('Delete error response:', text);
                            if (window.Toast) {
                                window.Toast.error('Error deleting item. Please try again.');
                            }
                        });
                    }
                }
            })
            .catch(function (err) {
                console.error('Request failed:', err);
                if (window.Toast) {
                    window.Toast.error('Request failed. Please try again.');
                }
            });
        }
    });
});

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