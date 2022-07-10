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
import { FormDialog } from "../dialog-form";
var PagePublishDialog = /** @class */ (function (_super) {
    __extends(PagePublishDialog, _super);
    function PagePublishDialog(pageId, options) {
        var _this = _super.call(this, options) || this;
        _this.pageId = pageId;
        return _this;
    }
    Object.defineProperty(PagePublishDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PagePublishDialog"; },
        enumerable: false,
        configurable: true
    });
    PagePublishDialog.prototype._buildUrl = function () {
        return "/brandup.pages/page/".concat(this.pageId, "/publish");
    };
    PagePublishDialog.prototype._buildForm = function () {
        this.setHeader("Публикация страницы");
        this.addTextBox("urlPath", "Название в url", { placeholder: "Введите название в url для страницы" });
        this.addTextBox("header", "Заголовок страницы", {});
    };
    PagePublishDialog.prototype._getSaveButtonTitle = function () {
        return "Опубликовать";
    };
    return PagePublishDialog;
}(FormDialog));
export { PagePublishDialog };
export var publishPage = function (pageId) {
    var dialog = new PagePublishDialog(pageId);
    return dialog.open();
};
