export const footerLinkTemplate = (link, key) => {
    return `
        <div class="flex flex-col gap-2" key="${key}">
            <h2 class="text-lg font-semibold">${link.label}</h2>
            <ul class="flex flex-col">
                ${link.links.map((linkItem, key) => {
                    const linkClass = linkItem.class ? ` ${linkItem.class}` : '';
                    const dataCc = linkItem.dataCc ? ` data-cc="${linkItem.dataCc}"` : '';
                    const target = linkItem.url.startsWith('http') ? ' target="_blank" rel="noopener noreferrer"' : '';
                    return `
                    <li class="group hover:bg-[var(--color-background-purple)] p-1" key="${key}">
                        <a 
                            href="${linkItem.url}" 
                            class="text-sm w-full group-hover:text-[var(--color-gradient-end-purple)]${linkClass}"${dataCc}${target}
                        >
                        ${linkItem.label}
                        </a>
                    </li>
                `;
                }).join('')}
            </ul>
        </div>
    `
}


export const contactInfoTemplate = (info, key) => {
    return `
        <div class="flex flex-col gap-2" key="${key}">
            <div class="flex items-center gap-3 mb-1">
                <img src="${info.icon}" alt="${info.label}" class="size-7" height="10" />
                <h2 class="text-2xl font-semibold">${info.label}</h2>
            </div>
            <div>
                ${info.links.map((link, key) => `
                    <div class="flex items-center gap-3 mb-3" key="${key}">
                        <img src="${link.icon}" alt="${link.label}" class="size-7" height="10" />
                        <a href="${link.url}" class="text-2xl underline text-[var(--color-gradient-end-purple)] w-full group-hover:text-[var(--color-gradient-end-purple)]">${link.label}</a>
                    </div>
                `).join('')}
            </div>
        </div>
    `
}