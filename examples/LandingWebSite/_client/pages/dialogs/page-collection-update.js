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
define(["require", "exports", "./dialog-form", "brandup-ui"], function (require, exports, dialog_form_1, brandup_ui_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var PageCollectionUpdateDialog = /** @class */ (function (_super) {
        __extends(PageCollectionUpdateDialog, _super);
        function PageCollectionUpdateDialog(collectionId, options) {
            var _this = _super.call(this, options) || this;
            _this.collectionId = collectionId;
            return _this;
        }
        Object.defineProperty(PageCollectionUpdateDialog.prototype, "typeName", {
            get: function () { return "BrandUpPages.PageCollectionUpdateDialog"; },
            enumerable: true,
            configurable: true
        });
        PageCollectionUpdateDialog.prototype._getSaveButtonTitle = function () {
            return "Сохранить";
        };
        PageCollectionUpdateDialog.prototype._buildUrl = function () {
            return "/brandup.pages/collection/" + this.collectionId;
        };
        PageCollectionUpdateDialog.prototype._buildUrlParams = function (urlParams) {
        };
        PageCollectionUpdateDialog.prototype._getMethod = function () {
            return "POST";
        };
        PageCollectionUpdateDialog.prototype._buildForm = function () {
            var _this = this;
            this.setHeader("Параметры коллекции страниц");
            brandup_ui_1.ajaxRequest({
                url: "/brandup.pages/collection/" + this.collectionId,
                success: function (data, status) {
                    if (status !== 200) {
                        _this.setError("Ошибка загрузки.");
                        return;
                    }
                    _this.addTextBox("Title", "Название", { placeholder: "Введите название коллекции" }, data.title);
                    _this.addComboBox("Sort", "Сортировка страниц", { placeholder: "Выберите порядок сортировки" }, [{ value: "FirstOld", title: "Сначало старые" }, { value: "FirstNew", title: "Сначало новые" }], data.sort);
                }
            });
        };
        return PageCollectionUpdateDialog;
    }(dialog_form_1.FormDialog));
    exports.PageCollectionUpdateDialog = PageCollectionUpdateDialog;
    exports.updatePageCollection = function (collectionId) {
        var dialog = new PageCollectionUpdateDialog(collectionId);
        return dialog.open();
    };
});
//# sourceMappingURL=page-collection-update.js.map