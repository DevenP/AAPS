window.downloadFile = (fileName, contentType, content) => {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
};

window.previewFile = (contentType, content) => {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
};

window.setThemeCookie = (isDarkMode) => {
    const mode = isDarkMode ? "dark" : "light";
    document.cookie = `theme=${mode}; path=/; max-age=31536000; SameSite=Lax`;
};

window.getThemeCookie = () => {
    const entry = document.cookie.split('; ').find(r => r.startsWith('theme='));
    return entry ? entry.split('=')[1] === 'dark' : false;
};