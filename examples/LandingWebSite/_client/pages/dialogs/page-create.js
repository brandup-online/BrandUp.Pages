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
define(["require", "exports", "./dialog-form"], function (require, exports, dialog_form_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var PageCreateDialog = /** @class */ (function (_super) {
        __extends(PageCreateDialog, _super);
        function PageCreateDialog(collectionId, options) {
            var _this = _super.call(this, options) || this;
            _this.collectionId = collectionId;
            return _this;
        }
        Object.defineProperty(PageCreateDialog.prototype, "typeName", {
            get: function () { return "BrandUpPages.PageCreateDialog"; },
            enumerable: true,
            configurable: true
        });
        PageCreateDialog.prototype._getSaveButtonTitle = function () {
            return "Создать";
        };
        PageCreateDialog.prototype._buildUrl = function () {
            return "/brandup.pages/page";
        };
        PageCreateDialog.prototype._buildUrlParams = function (urlParams) {
            urlParams["collectionId"] = this.collectionId;
        };
        PageCreateDialog.prototype._getMethod = function () {
            return "PUT";
        };
        PageCreateDialog.prototype._buildForm = function () {
            this.setHeader("Параметры новой страницы");
            this.addTextBox("Title", "Название", { placeholder: "Введите название новой страницы" }, null);
            this.addComboBox2("PageType", "Тип страницы", { placeholder: "Выберите тип новой страницы" }, null, "/brandup.pages/collection/" + this.collectionId + "/pageTypes", function (item) { return { value: item.name, title: item.title }; });
        };
        return PageCreateDialog;
    }(dialog_form_1.FormDialog));
    exports.PageCreateDialog = PageCreateDialog;
    exports.createPage = function (collectionId) {
        var dialog = new PageCreateDialog(collectionId);
        return dialog.open();
    };
});
//# sourceMappingURL=page-create.js.map