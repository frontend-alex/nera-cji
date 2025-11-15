import { ContactInfo, FooterLinks } from "./constants/data.js";
import { footerLinkTemplate, contactInfoTemplate } from "./templates/templates.js";
import { sidebarMenuData } from "./constants/sidebar-data.js";

// Wait for DOM to be ready
document.addEventListener('DOMContentLoaded', () => {
    const footerLinks = document.getElementById('footer-links');
    const contactInfo = document.getElementById('contact-info');
    
    if (footerLinks && FooterLinks) {
        footerLinks.innerHTML = FooterLinks.map((link, key) => footerLinkTemplate(link, key)).join('');
    }

    if (contactInfo && ContactInfo) {
        contactInfo.innerHTML = ContactInfo.map((info, key) => contactInfoTemplate(info, key)).join('');
    }

    // Initialize Sidebar
    if (document.getElementById('sidebar')) {
        window.sidebarInstance = window.createSidebar('sidebar', {
            data: sidebarMenuData,
            logoText: 'NERA',
            collapsed: false,
            mobileBreakpoint: 768,
            activePath: window.location.pathname,
            
            // Callbacks
            onToggle: (collapsed) => {
                console.log('Sidebar toggled:', collapsed ? 'collapsed' : 'expanded');
            },
            
            onItemClick: (item, event) => {
                console.log('Menu item clicked:', item);
                // You can add custom logic here, like analytics tracking
            }
        });
    }
});