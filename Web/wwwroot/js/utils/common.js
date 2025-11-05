// Common utility functions
window.NERA = window.NERA || {};

NERA.Utils = {
    // Format date for display
    formatDate: function(date) {
        return new Date(date).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    },

    // Format time for display
    formatTime: function(time) {
        return new Date(time).toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Show loading spinner
    showLoading: function(element) {
        if (element) {
            element.innerHTML = '<div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div>';
        }
    },

    // Hide loading spinner
    hideLoading: function(element, originalContent) {
        if (element && originalContent) {
            element.innerHTML = originalContent;
        }
    },

    // Show alert message
    showAlert: function(message, type = 'info') {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        
        const container = document.querySelector('.container');
        if (container) {
            container.insertBefore(alertDiv, container.firstChild);
        }
    }
};
