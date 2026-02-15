import {
    ClassicEditor, Essentials, Paragraph, Bold, Italic, Font, SimpleUploadAdapter, Image, ImageUpload, ImageToolbar, ImageCaption, ImageStyle, ImageResize, Link, List, BlockQuote, Heading, MediaEmbed, HtmlEmbed, GeneralHtmlSupport, Alignment,
    Underline, Strikethrough, Subscript, Superscript, Code, CodeBlock, HorizontalLine, SourceEditing, RemoveFormat, SpecialCharacters, FindAndReplace, Highlight,
    Table, TableToolbar, TableProperties, TableCellProperties, Indent, IndentBlock
} from 'ckeditor5';

export const editorConfig = {
    licenseKey: 'eyJhbGciOiJFUzI1NiJ9.eyJleHAiOjE3OTc5ODM5OTksImp0aSI6ImJlMWVlMjM0LTU5OWEtNDI4ZC1hNDZhLTM5NGZkODMzZTZlMiIsInVzYWdlRW5kcG9pbnQiOiJodHRwczovL3Byb3h5LWV2ZW50LmNrZWRpdG9yLmNvbSIsImRpc3RyaWJ1dGlvbkNoYW5uZWwiOlsiY2xvdWQiLCJkcnVwYWwiXSwiZmVhdHVyZXMiOlsiRFJVUCIsIkUyUCIsIkUyVyJdLCJyZW1vdmVGZWF0dXJlcyI6WyJQQiIsIlJGIiwiU0NIIiwiVENQIiwiVEwiLCJUQ1IiLCJJUiIsIlNVQSIsIkI2NEEiLCJMUCIsIkhFIiwiUkVEIiwiUEZPIiwiV0MiLCJGQVIiLCJCS00iLCJGUEgiLCJNUkUiXSwidmMiOiI4YjFkMGIyYSJ9.JXtI2yif_i7LNULBdcp0cLB32Ncv1XJP3JX_KO93drqGfxwDeEPk64YNUU576nVzsbt5I320Z2uUaWhtIrFrbA',
    plugins: [
        Essentials, Paragraph, Bold, Italic, Font, Link, Image, ImageUpload, ImageToolbar, ImageCaption, ImageStyle, ImageResize, List, BlockQuote, Heading, SimpleUploadAdapter, MediaEmbed, HtmlEmbed, GeneralHtmlSupport, Alignment,
        Underline, Strikethrough, Subscript, Superscript, Code, CodeBlock, HorizontalLine, SourceEditing, RemoveFormat, SpecialCharacters, FindAndReplace, Highlight,
        Table, TableToolbar, TableProperties, TableCellProperties, Indent, IndentBlock
    ],
    toolbar: {
        items: [
            'sourceEditing', '|',
            'heading', '|',
            'bold', 'italic', 'underline', 'strikethrough', 'code', 'removeFormat', '|',
            'subscript', 'superscript', '|',
            'link', 'bulletedList', 'numberedList', 'blockQuote', '|',
            'outdent', 'indent', 'alignment', '|',
            'fontSize', 'fontColor', 'fontBackgroundColor', 'highlight', '|',
            'insertTable', 'uploadImage', 'mediaEmbed', 'codeBlock', 'htmlEmbed', 'horizontalLine', 'specialCharacters', '|',
            'findAndReplace', 'undo', 'redo'
        ],
        shouldNotGroupWhenFull: true
    },
    table: {
        contentToolbar: [
            'tableColumn', 'tableRow', 'mergeTableCells', 'tableProperties', 'tableCellProperties'
        ]
    },
    simpleUpload: { uploadUrl: '/api/post/uploadimage' },
    htmlSupport: {
        allow: [
            {
                name: /.*/,
                attributes: true,
                classes: true,
                styles: true
            }
        ]
    },
    mediaEmbed: {
        previewsInData: true,
        extraProviders: [
            {
                name: 'vnexpress',
                url: /^https:\/\/video\.vnexpress\.net\/embed\/v_(\d+)/,
                html: match => {
                    const id = match[1];
                    return '<div style="position:relative; padding-bottom:56.25%; height:0; overflow:hidden;">' +
                        '<iframe src="https://video.vnexpress.net/embed/v_' + id + '" ' +
                        'style="position:absolute; top:0; left:0; width:100%; height:100%;" ' +
                        'frameborder="0" allow="autoplay; encrypted-media" allowfullscreen>' +
                        '</iframe>' +
                        '</div>';
                }
            }
        ]
    }
};
