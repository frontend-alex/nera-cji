export const sidebarMenuData = [
    {
        name: 'Dashboard',
        path: '/app/v1/dashboard',
        icon: 'home'
    },
    {
        type: 'group',
        label: 'Main Menu',
        items: [
            {
                name: 'Events',
                icon: 'file',
                items: [
                    {
                        name: 'All Events',
                        path: '/app/v1/events',
                        icon: 'file'
                    },
                    {
                        name: 'Create Event',
                        path: '/app/v1/events/create',
                        icon: 'file'
                    }
                ]
            }
        ]
    }
];

export const sidebarMenuDataWithCustomIcons = [
    {
        name: 'Dashboard',
        path: '/',
        icon: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>'
    },
    {
        name: 'Users',
        path: '/users',
        icon: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>'
    }
];

export const sidebarMenuDataNested = [
    {
        name: 'Level 1',
        icon: 'folder',
        items: [
            {
                name: 'Level 2',
                icon: 'folder',
                items: [
                    {
                        name: 'Level 3',
                        path: '/level3',
                        icon: 'file'
                    },
                    {
                        name: 'Level 3 with Children',
                        icon: 'folder',
                        items: [
                            {
                                name: 'Level 4',
                                path: '/level4',
                                icon: 'file'
                            }
                        ]
                    }
                ]
            }
        ]
    }
];

