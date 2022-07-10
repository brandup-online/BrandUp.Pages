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
var PageDeleteDialog = /** @class */ (function (_super) {
    __extends(PageDeleteDialog, _super);
    function PageDeleteDialog(pageId, options) {
        var _this = _super.call(this, options) || this;
        _this.pageId = pageId;
        return _this;
    }
    Object.defineProperty(PageDeleteDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageDeleteDialog"; },
        enumerable: false,
        configurable: true
    });
    PageDeleteDialog.prototype._onRenderContent = function () {
        this.setHeader("Удаление страницы");
        _super.prototype._onRenderContent.call(this);
    };
    PageDeleteDialog.prototype._getText = function () {
        return "Подтвердите удаление страницы.";
    };
    PageDeleteDialog.prototype._buildUrl = function () {
        return "/brandup.pages/page/".concat(this.pageId);
    };
    PageDeleteDialog.prototype._buildUrlParams = function (urlParams) {
    };
    return PageDeleteDialog;
}(DeleteDialog));
export { PageDeleteDialog };
export var deletePage = function (pageId) {
    var dialog = new PageDeleteDialog(pageId);
    return dialog.open();
};
