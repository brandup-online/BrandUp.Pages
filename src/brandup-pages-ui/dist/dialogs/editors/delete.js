var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
import { DeleteDialog } from "../dialog-delete";
var PageEditorDeleteDialog = /** @class */ (function (_super) {
    __extends(PageEditorDeleteDialog, _super);
    function PageEditorDeleteDialog(editorId, options) {
        var _this = _super.call(this, options) || this;
        _this.editorId = editorId;
        return _this;
    }
    Object.defineProperty(PageEditorDeleteDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageEditorDeleteDialog"; },
        enumerable: false,
        configurable: true
    });
    PageEditorDeleteDialog.prototype._onRenderContent = function () {
        this.setHeader("Удаление редактора");
        _super.prototype._onRenderContent.call(this);
    };
    PageEditorDeleteDialog.prototype._getText = function () {
        return "Подтвердите удаление редактора страниц.";
    };
    PageEditorDeleteDialog.prototype._buildUrl = function () {
        return "/brandup.pages/editor/".concat(this.editorId);
    };
    PageEditorDeleteDialog.prototype._buildUrlParams = function (urlParams) {
    };
    return PageEditorDeleteDialog;
}(DeleteDialog));
export var deletePageEditor = function (editorId) {
    var dialog = new PageEditorDeleteDialog(editorId);
    return dialog.open();
};
