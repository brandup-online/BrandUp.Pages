import BalloonEditor from '@ckeditor/ckeditor5-editor-balloon/src/ballooneditor';
import EssentialsPlugin from '@ckeditor/ckeditor5-essentials/src/essentials';
import AutoformatPlugin from '@ckeditor/ckeditor5-autoformat/src/autoformat';
import BoldPlugin from '@ckeditor/ckeditor5-basic-styles/src/bold';
import ItalicPlugin from '@ckeditor/ckeditor5-basic-styles/src/italic';
import HeadingPlugin from '@ckeditor/ckeditor5-heading/src/heading';
import LinkPlugin from '@ckeditor/ckeditor5-link/src/link';
import ListPlugin from '@ckeditor/ckeditor5-list/src/list';
import ParagraphPlugin from '@ckeditor/ckeditor5-paragraph/src/paragraph';
import './editor.css';

var createEditor = function (elem, config) {
    var editorConfig = {
        plugins: [
            EssentialsPlugin,
            AutoformatPlugin,
            BoldPlugin,
            ItalicPlugin,
            HeadingPlugin,
            LinkPlugin,
            ListPlugin,
            ParagraphPlugin
        ],

        toolbar: [
            'heading',
            'bold',
            'italic',
            'link',
            'bulletedList',
            'numberedList',
            'blockQuote',
            'undo',
            'redo'
        ]
    };

    if (config.toolbar)
        editorConfig.toolbar = config.toolbar;

    if (config.language)
        editorConfig.language = config.language;

    if (config.placeholder)
        editorConfig.placeholder = config.placeholder;

    return BalloonEditor.create(elem, editorConfig);
};

export default createEditor;