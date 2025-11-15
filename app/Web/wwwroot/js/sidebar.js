/**
 * Sophisticated Sidebar Component
 * Similar to shadcn sidebar with nested menu support, icons, and toggle functionality
 */

class Sidebar {
    constructor(containerId, options = {}) {
        this.container = document.getElementById(containerId);
        if (!this.container) {
            console.error(`Sidebar container with id "${containerId}" not found`);
            return;
        }

        this.options = {
            data: options.data || [],
            collapsed: options.collapsed || false,
            mobileBreakpoint: options.mobileBreakpoint || 768,
            onToggle: options.onToggle || null,
            onItemClick: options.onItemClick || null,
            activePath: options.activePath || window.location.pathname,
            ...options
        };

        this.state = {
            collapsed: this.options.collapsed,
            mobileOpen: false,
            expandedItems: new Set()
        };

        this.init();
    }

    init() {
        this.render();
        this.attachEventListeners();
        this.updateActiveState();
        this.handleResize();
        // Ensure initial state is set
        this.updateClasses();
    }

    render() {
        const { data } = this.options;
        
        this.container.innerHTML = `
            <div class="sidebar-header">
                <a href="/" class="sidebar-logo">
                    <div class="sidebar-logo-icon">
                        ${this.getIconHTML('logo')}
                    </div>
                    <span class="sidebar-logo-text">${this.options.logoText || 'Sidebar'}</span>
                </a>
                <button class="sidebar-toggle" aria-label="Toggle sidebar" type="button">
                    ${this.getIconHTML('chevron-left')}
                </button>
            </div>
            <div class="sidebar-content">
                ${this.renderMenu(data)}
            </div>
            ${this.options.footer ? `<div class="sidebar-footer">${this.options.footer}</div>` : ''}
        `;

        // Add overlay for mobile
        if (!document.querySelector('.sidebar-overlay')) {
            const overlay = document.createElement('div');
            overlay.className = 'sidebar-overlay';
            overlay.addEventListener('click', () => this.closeMobile());
            document.body.appendChild(overlay);
        }

        // Update classes
        this.updateClasses();
    }

    renderMenu(items, level = 0) {
        if (!items || items.length === 0) return '';

        let html = '<ul class="sidebar-menu">';
        
        items.forEach((item, index) => {
            const hasChildren = item.items && item.items.length > 0;
            const isGroup = item.type === 'group';
            const isSeparator = item.type === 'separator';
            
            if (isGroup) {
                html += `
                    <li class="sidebar-group">
                        <div class="sidebar-group-label">${item.label || ''}</div>
                        ${this.renderMenu(item.items, level)}
                    </li>
                `;
            } else if (isSeparator) {
                html += '<li class="sidebar-separator"></li>';
            } else {
                const itemId = `sidebar-item-${level}-${index}`;
                const isActive = this.isActive(item);
                const isExpanded = this.state.expandedItems.has(itemId);
                const iconHTML = item.icon ? this.getIconHTML(item.icon) : '';
                const badgeHTML = item.badge ? `<span class="sidebar-menu-badge">${item.badge}</span>` : '';
                
                if (hasChildren) {
                    html += `
                        <li class="sidebar-menu-item">
                            <button 
                                class="sidebar-menu-button ${isActive ? 'active' : ''}" 
                                data-item-id="${itemId}"
                                aria-expanded="${isExpanded}"
                                type="button"
                            >
                                ${iconHTML ? `<span class="sidebar-menu-icon">${iconHTML}</span>` : ''}
                                <span class="sidebar-menu-text">${item.name || item.label || ''}</span>
                                ${badgeHTML}
                                <span class="sidebar-menu-chevron">${this.getIconHTML('chevron-right')}</span>
                                ${this.state.collapsed ? `<span class="sidebar-tooltip">${item.name || item.label || ''}</span>` : ''}
                            </button>
                            <ul class="sidebar-submenu ${isExpanded ? 'expanded' : ''}">
                                ${this.renderSubmenu(item.items, level + 1)}
                            </ul>
                        </li>
                    `;
                } else {
                    const linkHTML = item.path || item.url || item.href || '#';
                    const isLink = item.path || item.url || item.href;
                    
                    html += `
                        <li class="sidebar-menu-item">
                            ${isLink ? `
                                <a 
                                    href="${linkHTML}" 
                                    class="sidebar-menu-button ${isActive ? 'active' : ''}"
                                    data-item-id="${itemId}"
                                >
                                    ${iconHTML ? `<span class="sidebar-menu-icon">${iconHTML}</span>` : ''}
                                    <span class="sidebar-menu-text">${item.name || item.label || ''}</span>
                                    ${badgeHTML}
                                    ${this.state.collapsed ? `<span class="sidebar-tooltip">${item.name || item.label || ''}</span>` : ''}
                                </a>
                            ` : `
                                <button 
                                    class="sidebar-menu-button ${isActive ? 'active' : ''}"
                                    data-item-id="${itemId}"
                                    type="button"
                                >
                                    ${iconHTML ? `<span class="sidebar-menu-icon">${iconHTML}</span>` : ''}
                                    <span class="sidebar-menu-text">${item.name || item.label || ''}</span>
                                    ${badgeHTML}
                                    ${this.state.collapsed ? `<span class="sidebar-tooltip">${item.name || item.label || ''}</span>` : ''}
                                </button>
                            `}
                        </li>
                    `;
                }
            }
        });
        
        html += '</ul>';
        return html;
    }

    renderSubmenu(items, level) {
        if (!items || items.length === 0) return '';

        let html = '';
        
        items.forEach((item, index) => {
            const hasChildren = item.items && item.items.length > 0;
            const itemId = `sidebar-submenu-${level}-${index}`;
            const isActive = this.isActive(item);
            const iconHTML = item.icon ? this.getIconHTML(item.icon) : '';
            
            if (hasChildren) {
                // Nested submenu support
                const isExpanded = this.state.expandedItems.has(itemId);
                html += `
                    <li class="sidebar-submenu-item">
                        <button 
                            class="sidebar-submenu-button ${isActive ? 'active' : ''}"
                            data-item-id="${itemId}"
                            aria-expanded="${isExpanded}"
                            type="button"
                        >
                            ${iconHTML ? `<span class="sidebar-submenu-icon">${iconHTML}</span>` : ''}
                            <span class="sidebar-submenu-text">${item.name || item.label || ''}</span>
                            <span class="sidebar-menu-chevron">${this.getIconHTML('chevron-right')}</span>
                        </button>
                        <ul class="sidebar-submenu ${isExpanded ? 'expanded' : ''}">
                            ${this.renderSubmenu(item.items, level + 1)}
                        </ul>
                    </li>
                `;
            } else {
                const linkHTML = item.path || item.url || item.href || '#';
                html += `
                    <li class="sidebar-submenu-item">
                        <a 
                            href="${linkHTML}" 
                            class="sidebar-submenu-button ${isActive ? 'active' : ''}"
                            data-item-id="${itemId}"
                        >
                            ${iconHTML ? `<span class="sidebar-submenu-icon">${iconHTML}</span>` : ''}
                            <span class="sidebar-submenu-text">${item.name || item.label || ''}</span>
                        </a>
                    </li>
                `;
            }
        });
        
        return html;
    }

    getIconHTML(iconName) {
        // Default icon set - you can customize this
        const icons = {
            'logo': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 9h6v6H9z"/></svg>',
            'chevron-left': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m15 18-6-6 6-6"/></svg>',
            'chevron-right': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m9 18 6-6-6-6"/></svg>',
            'home': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>',
            'user': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>',
            'settings': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="3"/><path d="M12 1v6m0 6v6M5.64 5.64l4.24 4.24m4.24 4.24l4.24 4.24M1 12h6m6 0h6M5.64 18.36l4.24-4.24m4.24-4.24l4.24-4.24"/></svg>',
            'file': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>',
            'folder': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>',
            'mail': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="4" width="20" height="16" rx="2"/><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/></svg>',
            'bell': '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>',
        };

        // If icon is a string and exists in our icon set, use it
        if (typeof iconName === 'string' && icons[iconName]) {
            return icons[iconName];
        }
        
        // If icon is HTML/SVG string, return it directly
        if (typeof iconName === 'string' && iconName.trim().startsWith('<')) {
            return iconName;
        }
        
        // If icon is an object with svg property
        if (typeof iconName === 'object' && iconName.svg) {
            return iconName.svg;
        }

        // Default fallback
        return icons['file'] || '';
    }

    attachEventListeners() {
        // Toggle button
        const toggleBtn = this.container.querySelector('.sidebar-toggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', () => this.toggle());
        }

        // Menu item clicks
        this.container.addEventListener('click', (e) => {
            const button = e.target.closest('.sidebar-menu-button, .sidebar-submenu-button');
            if (!button) return;

            const itemId = button.getAttribute('data-item-id');
            if (!itemId) return;

            // Check if it's a collapsible item
            if (button.hasAttribute('aria-expanded')) {
                e.preventDefault();
                this.toggleItem(itemId);
            } else {
                // Handle item click callback
                if (this.options.onItemClick) {
                    const item = this.findItemById(itemId);
                    if (item) {
                        this.options.onItemClick(item, e);
                    }
                }
            }
        });

        // Window resize
        window.addEventListener('resize', () => this.handleResize());

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
                e.preventDefault();
                this.toggle();
            }
        });
    }

    toggle() {
        if (window.innerWidth <= this.options.mobileBreakpoint) {
            this.toggleMobile();
        } else {
            this.toggleCollapse();
        }
    }

    toggleCollapse() {
        this.state.collapsed = !this.state.collapsed;
        this.updateClasses();
        
        if (this.options.onToggle) {
            this.options.onToggle(this.state.collapsed);
        }

        // Save to localStorage
        localStorage.setItem('sidebar-collapsed', this.state.collapsed);
    }

    toggleMobile() {
        this.state.mobileOpen = !this.state.mobileOpen;
        this.updateClasses();
        
        const overlay = document.querySelector('.sidebar-overlay');
        if (overlay) {
            overlay.classList.toggle('active', this.state.mobileOpen);
        }

        if (this.options.onToggle) {
            this.options.onToggle(this.state.mobileOpen);
        }
    }

    closeMobile() {
        this.state.mobileOpen = false;
        this.updateClasses();
        
        const overlay = document.querySelector('.sidebar-overlay');
        if (overlay) {
            overlay.classList.remove('active');
        }
    }

    toggleItem(itemId) {
        if (this.state.expandedItems.has(itemId)) {
            this.state.expandedItems.delete(itemId);
        } else {
            this.state.expandedItems.add(itemId);
        }

        const button = this.container.querySelector(`[data-item-id="${itemId}"]`);
        const submenu = button?.nextElementSibling || button?.parentElement?.querySelector('.sidebar-submenu');
        
        if (button) {
            button.setAttribute('aria-expanded', this.state.expandedItems.has(itemId));
        }
        
        if (submenu) {
            submenu.classList.toggle('expanded', this.state.expandedItems.has(itemId));
        }
    }

    updateClasses() {
        this.container.classList.toggle('collapsed', this.state.collapsed);
        this.container.classList.toggle('mobile-open', this.state.mobileOpen);
        this.container.classList.toggle('hidden', !this.state.mobileOpen && window.innerWidth <= this.options.mobileBreakpoint && !this.state.mobileOpen);
        
        document.body.classList.toggle('sidebar-open', !this.state.collapsed && window.innerWidth > this.options.mobileBreakpoint);
        document.body.classList.toggle('sidebar-closed', this.state.collapsed || window.innerWidth <= this.options.mobileBreakpoint);
    }

    isActive(item) {
        if (!item) return false;
        
        const activePath = this.options.activePath || window.location.pathname;
        const itemPath = item.path || item.url || item.href;
        
        if (!itemPath || itemPath === '#') return false;
        
        // Exact match
        if (activePath === itemPath) return true;
        
        // Check if active path starts with item path (for nested routes)
        // Only if item path is not just '/'
        if (itemPath !== '/' && activePath.startsWith(itemPath)) {
            // Make sure it's a proper path segment (not just a prefix)
            const nextChar = activePath[itemPath.length];
            return !nextChar || nextChar === '/' || nextChar === '?';
        }
        
        return false;
    }

    updateActiveState() {
        const activePath = this.options.activePath || window.location.pathname;
        const allButtons = this.container.querySelectorAll('.sidebar-menu-button, .sidebar-submenu-button');
        
        allButtons.forEach(button => {
            const href = button.getAttribute('href');
            if (href && href !== '#' && activePath === href) {
                button.classList.add('active');
                
                // Expand parent menus
                let parent = button.closest('.sidebar-submenu');
                while (parent) {
                    const parentButton = parent.previousElementSibling;
                    if (parentButton && parentButton.hasAttribute('data-item-id')) {
                        const itemId = parentButton.getAttribute('data-item-id');
                        this.state.expandedItems.add(itemId);
                        parentButton.setAttribute('aria-expanded', 'true');
                        parent.classList.add('expanded');
                    }
                    parent = parent.closest('.sidebar-submenu')?.parentElement?.closest('.sidebar-submenu');
                }
            }
        });
    }

    findItemById(itemId) {
        // This is a simplified version - you might want to traverse the data structure
        const findInItems = (items) => {
            for (const item of items) {
                if (item.id === itemId) return item;
                if (item.items) {
                    const found = findInItems(item.items);
                    if (found) return found;
                }
            }
            return null;
        };
        return findInItems(this.options.data);
    }

    handleResize() {
        if (window.innerWidth <= this.options.mobileBreakpoint) {
            this.closeMobile();
        }
        this.updateClasses();
    }

    // Public API methods
    collapse() {
        if (!this.state.collapsed) {
            this.toggleCollapse();
        }
    }

    expand() {
        if (this.state.collapsed) {
            this.toggleCollapse();
        }
    }

    open() {
        if (window.innerWidth <= this.options.mobileBreakpoint) {
            this.state.mobileOpen = true;
        } else {
            this.state.collapsed = false;
        }
        this.updateClasses();
    }

    close() {
        if (window.innerWidth <= this.options.mobileBreakpoint) {
            this.closeMobile();
    } else {
            this.state.collapsed = true;
            this.updateClasses();
        }
    }

    setData(data) {
        this.options.data = data;
        this.render();
        this.attachEventListeners();
        this.updateActiveState();
    }

    setActivePath(path) {
        this.options.activePath = path;
        this.updateActiveState();
    }
}

// Initialize sidebar from localStorage
const initSidebarFromStorage = () => {
    const savedState = localStorage.getItem('sidebar-collapsed');
    return savedState === 'true';
};

// Global functions for easy access
window.Sidebar = Sidebar;

// Convenience function to create a sidebar
window.createSidebar = (containerId, options = {}) => {
    const defaultOptions = {
        collapsed: initSidebarFromStorage(),
        ...options
    };
    return new Sidebar(containerId, defaultOptions);
};

// Legacy toggle function for backward compatibility
window.toggleSidebar = () => {
    const sidebar = window.sidebarInstance;
    if (sidebar) {
        sidebar.toggle();
    } else {
        // Fallback to old behavior
        const sidebarEl = document.getElementById('sidebar');
        if (sidebarEl) {
            sidebarEl.classList.toggle('active');
            const isActive = sidebarEl.classList.contains('active');
            document.body.classList.toggle('sidebar-open', isActive);
            document.body.classList.toggle('sidebar-closed', !isActive);
        }
    }
};
