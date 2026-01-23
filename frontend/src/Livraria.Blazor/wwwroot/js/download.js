// Função para download de arquivos no Blazor
window.downloadFile = (fileName, contentType, content) => {
    const blob = new Blob([new Uint8Array(content)], { type: contentType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};