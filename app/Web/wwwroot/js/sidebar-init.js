/**
 * Sidebar Initialization Script
 * 
 * This file shows how to initialize the sidebar component.
 * Import your menu data and create a sidebar instance.
 */

import { sidebarMenuData } from './constants/sidebar-data.js';

document.addEventListener('DOMContentLoaded', () => {
    window.sidebarInstance = window.createSidebar('sidebar', {
        data: sidebarMenuData,
        logoText: 'NERA',
        collapsed: false,
        mobileBreakpoint: 768,
        activePath: window.location.pathname,
        
        onToggle: (collapsed) => {
            console.log('Sidebar toggled:', collapsed ? 'collapsed' : 'expanded');
        },
        
        onItemClick: (item, event) => {
            console.log('Menu item clicked:', item);
        }
    });
    
    console.log('Sidebar initialized');
});

window.updateSidebarData = (newData) => {
    if (window.sidebarInstance) {
        window.sidebarInstance.setData(newData);
    }
};

window.updateSidebarActivePath = (path) => {
    if (window.sidebarInstance) {
        window.sidebarInstance.setActivePath(path);
    }
};

