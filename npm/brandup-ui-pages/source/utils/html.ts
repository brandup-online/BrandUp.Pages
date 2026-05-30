// Экранирование текста перед вставкой в innerHTML — защита от XSS.
export const escapeHtml = (value: string): string => {
    if (!value)
        return "";

    return value
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
};

// Готовит текстовое значение для вставки в innerHTML: экранирует HTML,
// при многострочном режиме заменяет переводы строк на <br />.
export const textToHtml = (value: string, allowMultiline?: boolean): string => {
    const escaped = escapeHtml(value);
    return allowMultiline ? escaped.replace(/(?:\r\n|\r|\n)/g, "<br />") : escaped;
};
