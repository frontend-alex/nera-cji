export const footerLinkTemplate = (link, key) => {
    return `
        <div class="flex flex-col gap-2" key="${key}">
            <h2 class="text-lg font-semibold">${link.label}</h2>
            <ul class="flex flex-col">
                ${link.links.map((link, key) => `
                    <li class="group hover:bg-[var(--color-background-purple)] p-1" key="${key}">
                        <a 
                            href="${link.url}" 
                            class="text-sm w-full group-hover:text-[var(--color-gradient-end-purple)]"
                        >
                        ${link.label}
                        </a>
                    </li>
                `).join('')}
            </ul>
        </div>
    `
}
