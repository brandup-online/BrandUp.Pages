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
var PageCollectionUpdateDialog = /** @class */ (function (_super) {
    __extends(PageCollectionUpdateDialog, _super);
    function PageCollectionUpdateDialog(collectionId, options) {
        var _this = _super.call(this, options) || this;
        _this.collectionId = collectionId;
        return _this;
    }
    Object.defineProperty(PageCollectionUpdateDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageCollectionUpdateDialog"; },
        enumerable: false,
        configurable: true
    });
    PageCollectionUpdateDialog.prototype._getSaveButtonTitle = function () {
        return "Сохранить";
    };
    PageCollectionUpdateDialog.prototype._buildUrl = function () {
        return "/brandup.pages/collection/".concat(this.collectionId, "/update");
    };
    PageCollectionUpdateDialog.prototype._buildForm = function (model) {
        this.setHeader("Параметры коллекции страниц");
        this.addTextBox("Title", "Название", { placeholder: "Введите название коллекции" });
        this.addComboBox("Sort", "Сортировка страниц", { placeholder: "Выберите порядок сортировки" }, model.sorts);
    };
    return PageCollectionUpdateDialog;
}(FormDialog));
export { PageCollectionUpdateDialog };
export var updatePageCollection = function (collectionId) {
    var dialog = new PageCollectionUpdateDialog(collectionId);
    return dialog.open();
};
