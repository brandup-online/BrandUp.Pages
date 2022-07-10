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
var PageCreateDialog = /** @class */ (function (_super) {
    __extends(PageCreateDialog, _super);
    function PageCreateDialog(collectionId, options) {
        var _this = _super.call(this, options) || this;
        _this.collectionId = collectionId;
        return _this;
    }
    Object.defineProperty(PageCreateDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageCreateDialog"; },
        enumerable: false,
        configurable: true
    });
    PageCreateDialog.prototype._onRenderContent = function () {
        _super.prototype._onRenderContent.call(this);
        this.setHeader("Параметры новой страницы");
    };
    PageCreateDialog.prototype._getSaveButtonTitle = function () {
        return "Создать";
    };
    PageCreateDialog.prototype._buildUrl = function () {
        return "/brandup.pages/page/create";
    };
    PageCreateDialog.prototype._buildUrlParams = function (urlParams) {
        urlParams["collectionId"] = this.collectionId;
    };
    PageCreateDialog.prototype._buildForm = function (model) {
        this.addTextBox("Header", "Название", { placeholder: "Введите название новой страницы" });
        this.addComboBox("PageType", "Тип страницы", { placeholder: "Выберите тип новой страницы" }, model.pageTypes);
    };
    return PageCreateDialog;
}(FormDialog));
export { PageCreateDialog };
export var createPage = function (collectionId) {
    var dialog = new PageCreateDialog(collectionId);
    return dialog.open();
};
