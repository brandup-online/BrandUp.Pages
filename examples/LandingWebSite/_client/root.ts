import "./styles.less";

var toTranslit = (text: string) => {
    return text.replace(/([а-яё])|([\s_-])|([^a-z\d])/gi,
        (all: string, ch: string, space: string, words: string, i: number) => {
            if (space || words) {
                return space ? '-' : '';
            }
            var code = ch.charCodeAt(0);
            var next = text.charAt(i + 1);
            var index = code == 1025 || code == 1105 ? 0 : code > 1071 ? code - 1071 : code - 1039;
            var t = ['yo', 'a', 'b', 'v', 'g', 'd', 'e', 'zh',
                'z', 'i', 'y', 'k', 'l', 'm', 'n', 'o', 'p',
                'r', 's', 't', 'u', 'f', 'h', 'c', 'ch', 'sh',
                'shch', '', 'y', '', 'e', 'yu', 'ya'
            ];

            var isNext = next && next.toUpperCase() === next ? 1 : 0;

            return ch.toUpperCase() === ch ? isNext ? t[index].toUpperCase() : t[index].substr(0, 1).toUpperCase() + t[index].substring(1) : t[index];
        }
    );
}