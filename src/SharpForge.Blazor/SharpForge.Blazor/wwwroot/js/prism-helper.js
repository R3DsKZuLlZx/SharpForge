// Prism.js syntax highlighting initialization
export function highlightAll() {
    if (typeof Prism !== 'undefined') {
        Prism.highlightAll();
    }
}

export function highlightElement(element) {
    if (typeof Prism !== 'undefined' && element) {
        Prism.highlightElement(element);
    }
}

