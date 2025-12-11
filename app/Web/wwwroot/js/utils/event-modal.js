/**
 * Event Details Modal
 * Handles opening and closing the event details modal with animations
 */

window.NERA = window.NERA || {};
NERA.EventModal = {
    overlay: null,
    modal: null,
    currentEventId: null,

    init: function() {
        // Create modal HTML if it doesn't exist
        if (!document.getElementById('event-modal-overlay')) {
            this.createModal();
        }

        this.overlay = document.getElementById('event-modal-overlay');
        this.modal = document.getElementById('event-modal');

        // Close on overlay click
        if (this.overlay) {
            this.overlay.addEventListener('click', (e) => {
                if (e.target === this.overlay) {
                    this.close();
                }
            });
        }

        // Close on close button click
        const closeBtn = document.getElementById('event-modal-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', () => this.close());
        }

        // Close on Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.overlay?.classList.contains('active')) {
                this.close();
            }
        });

        // Attach click handlers to all "View details" links
        this.attachEventListeners();
    },

    createModal: function() {
        const modalHTML = `
            <div id="event-modal-overlay" class="event-modal-overlay">
                <div id="event-modal" class="event-modal">
                    <div class="event-modal-header">
                        <h2 id="event-modal-title" class="event-modal-title">Event Details</h2>
                        <button id="event-modal-close" class="event-modal-close" type="button" aria-label="Close">
                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <line x1="18" y1="6" x2="6" y2="18"></line>
                                <line x1="6" y1="6" x2="18" y2="18"></line>
                            </svg>
                        </button>
                    </div>
                    <div id="event-modal-body" class="event-modal-body">
                        <div class="event-modal-loading">
                            <div class="event-modal-spinner"></div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        document.body.insertAdjacentHTML('beforeend', modalHTML);
    },

    attachEventListeners: function() {
        // Use event delegation for dynamically added elements
        document.addEventListener('click', (e) => {
            const viewDetailsLink = e.target.closest('[data-event-id]');
            if (viewDetailsLink) {
                e.preventDefault();
                const eventId = parseInt(viewDetailsLink.getAttribute('data-event-id'));
                if (eventId) {
                    this.open(eventId);
                }
            }
        });
    },

    open: async function(eventId) {
        this.currentEventId = eventId;
        
        if (!this.overlay) {
            this.init();
        }

        // Show loading state
        this.showLoading();

        // Show modal
        this.overlay.classList.add('active');
        document.body.style.overflow = 'hidden';

        try {
            // Fetch event details
            const response = await fetch(`/app/v1/events/api/details/${eventId}`);
            const data = await response.json();

            if (data.success) {
                this.renderEventDetails(data.eventData);
            } else {
                this.showError(data.message || 'Failed to load event details');
            }
        } catch (error) {
            console.error('Error loading event details:', error);
            this.showError('Failed to load event details. Please try again.');
        }
    },

    close: function() {
        if (this.overlay) {
            this.overlay.classList.remove('active');
            document.body.style.overflow = '';
            this.currentEventId = null;
        }
    },

    showLoading: function() {
        const body = document.getElementById('event-modal-body');
        if (body) {
            body.innerHTML = `
                <div class="event-modal-loading">
                    <div class="event-modal-spinner"></div>
                </div>
            `;
        }
    },

    showError: function(message) {
        const body = document.getElementById('event-modal-body');
        if (body) {
            body.innerHTML = `
                <div class="event-modal-section">
                    <p class="event-modal-value" style="color: #dc2626;">${message}</p>
                </div>
            `;
        }
    },

    renderEventDetails: function(event) {
        const title = document.getElementById('event-modal-title');
        const body = document.getElementById('event-modal-body');

        if (!title || !body) return;

        title.textContent = event.title;

        const costBadge = event.cost === 0 
            ? '<span class="event-modal-badge free">Free</span>'
            : `<span class="event-modal-badge paid">€${event.cost.toFixed(2)}</span>`;

        const statusBadges = [];
        if (event.isFull) {
            statusBadges.push('<span class="event-modal-badge full">Full</span>');
        }
        if (event.isRegistered) {
            statusBadges.push('<span class="event-modal-badge registered">Registered</span>');
        }

        const spotsInfo = event.maxParticipants 
            ? `${event.spotsLeft} of ${event.maxParticipants} spots available`
            : 'Unlimited spots available';

        body.innerHTML = `
            <div class="event-modal-section">
                <span class="event-modal-label">Description</span>
                <p class="event-modal-description">${this.escapeHtml(event.description)}</p>
            </div>

            <div class="event-modal-section">
                <span class="event-modal-label">Event Information</span>
                <div class="event-modal-info-grid">
                    <div class="event-modal-info-item">
                        <svg class="event-modal-info-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                        <div>
                            <div class="event-modal-label" style="margin-bottom: 4px;">Date & Time</div>
                            <div class="event-modal-value">${event.startTime}${event.endTime ? ' - ' + event.endTime : ''}</div>
                        </div>
                    </div>

                    <div class="event-modal-info-item">
                        <svg class="event-modal-info-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.828 0l-4.244-4.243a8 8 0 1111.314 0z" />
                            <path stroke-linecap="round" stroke-linejoin="round" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                        </svg>
                        <div>
                            <div class="event-modal-label" style="margin-bottom: 4px;">Location</div>
                            <div class="event-modal-value">${this.escapeHtml(event.location || 'Not specified')}</div>
                        </div>
                    </div>

                    <div class="event-modal-info-item">
                        <svg class="event-modal-info-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <div>
                            <div class="event-modal-label" style="margin-bottom: 4px;">Cost</div>
                            <div class="event-modal-value">${costBadge}</div>
                        </div>
                    </div>

                    <div class="event-modal-info-item">
                        <svg class="event-modal-info-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                        </svg>
                        <div>
                            <div class="event-modal-label" style="margin-bottom: 4px;">Capacity</div>
                            <div class="event-modal-value">${spotsInfo}</div>
                        </div>
                    </div>
                </div>
            </div>

            ${statusBadges.length > 0 ? `
            <div class="event-modal-section">
                <div style="display: flex; gap: 8px; flex-wrap: wrap;">
                    ${statusBadges.join('')}
                </div>
            </div>
            ` : ''}
        `;
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
};

// Initialize on page load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        NERA.EventModal.init();
    });
} else {
    NERA.EventModal.init();
}

