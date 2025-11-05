import { FooterLinks } from "./constants/data.js";
import { footerLinkTemplate } from "./templates/templates.js";

// Wait for DOM to be ready
document.addEventListener('DOMContentLoaded', () => {
    const footerLinks = document.getElementById('footer-links');
    
    if (footerLinks && FooterLinks) {
        footerLinks.innerHTML = FooterLinks.map((link, key) => footerLinkTemplate(link, key)).join('');
    }
});