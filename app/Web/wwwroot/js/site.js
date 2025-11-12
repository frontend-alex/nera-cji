import { ContactInfo, FooterLinks } from "./constants/data.js";
import { footerLinkTemplate, contactInfoTemplate } from "./templates/templates.js";

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
});