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
var PageSeoDialog = /** @class */ (function (_super) {
    __extends(PageSeoDialog, _super);
    function PageSeoDialog(pageId, options) {
        var _this = _super.call(this, options) || this;
        _this.pageId = pageId;
        return _this;
    }
    Object.defineProperty(PageSeoDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageSeoDialog"; },
        enumerable: false,
        configurable: true
    });
    PageSeoDialog.prototype._onRenderContent = function () {
        _super.prototype._onRenderContent.call(this);
        this.setHeader("Параметры SEO");
    };
    PageSeoDialog.prototype._getSaveButtonTitle = function () {
        return "Сохранить";
    };
    PageSeoDialog.prototype._buildUrl = function () {
        return "/brandup.pages/page/seo";
    };
    PageSeoDialog.prototype._buildUrlParams = function (urlParams) {
        urlParams["pageId"] = this.pageId;
    };
    PageSeoDialog.prototype._buildForm = function (model) {
        this.addTextBox("Title", "Заголовок страницы", {});
        this.addTextBox("Description", "Описание страницы", {});
        this.addStringArray("Keywords", "Ключевые слова", { placeholder: "Введите ключевое слово" });
    };
    return PageSeoDialog;
}(FormDialog));
export { PageSeoDialog };
export var seoPage = function (collectionId) {
    var dialog = new PageSeoDialog(collectionId);
    return dialog.open();
};
