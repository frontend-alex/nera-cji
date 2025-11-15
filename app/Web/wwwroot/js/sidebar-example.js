/**
 * Sidebar Usage Examples
 * 
 * This file shows different ways to use the sidebar component.
 */

// Example 1: Basic usage with inline data
const basicSidebarData = [
    {
        name: 'Home',
        path: '/',
        icon: 'home'
    },
    {
        name: 'About',
        path: '/about',
        icon: 'user'
    }
];

// Example 2: With nested items
const nestedSidebarData = [
    {
        name: 'Dashboard',
        path: '/dashboard',
        icon: 'home',
        items: [
            {
                name: 'Overview',
                path: '/dashboard/overview',
                icon: 'file'
            },
            {
                name: 'Analytics',
                path: '/dashboard/analytics',
                icon: 'file'
            }
        ]
    }
];

// Example 3: With groups and separators
const groupedSidebarData = [
    {
        type: 'group',
        label: 'Navigation',
        items: [
            {
                name: 'Home',
                path: '/',
                icon: 'home'
            }
        ]
    },
    {
        type: 'separator'
    },
    {
        type: 'group',
        label: 'Settings',
        items: [
            {
                name: 'Profile',
                path: '/profile',
                icon: 'user'
            }
        ]
    }
];

// Example 4: With badges
const badgeSidebarData = [
    {
        name: 'Messages',
        path: '/messages',
        icon: 'mail',
        badge: '12'
    },
    {
        name: 'Notifications',
        path: '/notifications',
        icon: 'bell',
        badge: '3'
    }
];

// Example 5: With custom SVG icons
const customIconSidebarData = [
    {
        name: 'Custom Item',
        path: '/custom',
        icon: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/></svg>'
    }
];

function initializeCustomSidebar() {
    const sidebar = window.createSidebar('sidebar', {
        data: basicSidebarData,
        logoText: 'My App',
        collapsed: false,
        mobileBreakpoint: 768,
        activePath: window.location.pathname,
        onToggle: (collapsed) => {
            console.log('Sidebar state:', collapsed);
        },
        onItemClick: (item, event) => {
            console.log('Clicked:', item.name);
        }
    });
    
    return sidebar;
}

function updateSidebarExample() {
    if (window.sidebarInstance) {
        window.sidebarInstance.setData(nestedSidebarData);
    }
}

function controlSidebarExample() {
    const sidebar = window.sidebarInstance;
    
    if (sidebar) {
        sidebar.collapse();
        
        setTimeout(() => sidebar.expand(), 2000);
        
        setTimeout(() => sidebar.open(), 4000);
        
        setTimeout(() => sidebar.close(), 6000);
    }
}

function updateActivePathExample(newPath) {
    if (window.sidebarInstance) {
        window.sidebarInstance.setActivePath(newPath);
    }
}

export {
    basicSidebarData,
    nestedSidebarData,
    groupedSidebarData,
    badgeSidebarData,
    customIconSidebarData,
    initializeCustomSidebar,
    updateSidebarExample,
    controlSidebarExample,
    updateActivePathExample
};

