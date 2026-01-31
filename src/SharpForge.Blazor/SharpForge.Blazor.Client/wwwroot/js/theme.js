export function getTheme() {
    return localStorage.getItem('sharpforge-theme');
}

export function setTheme(theme) {
    localStorage.setItem('sharpforge-theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
}

export function getSystemPreference() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

