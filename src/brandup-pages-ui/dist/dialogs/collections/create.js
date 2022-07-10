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
var PageCollectionCreateDialog = /** @class */ (function (_super) {
    __extends(PageCollectionCreateDialog, _super);
    function PageCollectionCreateDialog(pageId, options) {
        var _this = _super.call(this, options) || this;
        _this.pageId = pageId;
        return _this;
    }
    Object.defineProperty(PageCollectionCreateDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageCollectionCreateDialog"; },
        enumerable: false,
        configurable: true
    });
    PageCollectionCreateDialog.prototype._getSaveButtonTitle = function () {
        return "Создать";
    };
    PageCollectionCreateDialog.prototype._buildUrl = function () {
        return "/brandup.pages/collection/create";
    };
    PageCollectionCreateDialog.prototype._buildUrlParams = function (urlParams) {
        if (this.pageId)
            urlParams["pageId"] = this.pageId;
    };
    PageCollectionCreateDialog.prototype._buildForm = function (model) {
        this.setHeader("Создание коллекции страниц");
        this.addTextBox("Title", "Название", { placeholder: "Введите название коллекции" });
        this.addComboBox("PageType", "Тип страниц", { placeholder: "Выберите тип страниц" }, model.pageTypes);
        this.addComboBox("Sort", "Сортировка страниц", { placeholder: "Выберите порядок сортировки" }, model.sorts);
    };
    return PageCollectionCreateDialog;
}(FormDialog));
export { PageCollectionCreateDialog };
export var createPageCollection = function (pageId) {
    var dialog = new PageCollectionCreateDialog(pageId);
    return dialog.open();
};
