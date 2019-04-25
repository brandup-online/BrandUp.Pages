var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
define(["require", "exports", "./dialog-delete"], function (require, exports, dialog_delete_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var PageDeleteDialog = /** @class */ (function (_super) {
        __extends(PageDeleteDialog, _super);
        function PageDeleteDialog(pageId, options) {
            var _this = _super.call(this, options) || this;
            _this.pageId = pageId;
            return _this;
        }
        Object.defineProperty(PageDeleteDialog.prototype, "typeName", {
            get: function () { return "BrandUpPages.PageDeleteDialog"; },
            enumerable: true,
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
            return "/brandup.pages/page/" + this.pageId;
        };
        PageDeleteDialog.prototype._buildUrlParams = function (urlParams) {
        };
        return PageDeleteDialog;
    }(dialog_delete_1.DeleteDialog));
    exports.PageDeleteDialog = PageDeleteDialog;
    exports.deletePage = function (pageId) {
        var dialog = new PageDeleteDialog(pageId);
        return dialog.open();
    };
});
//# sourceMappingURL=page-delete.js.map