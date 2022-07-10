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
var PageCollectionDeleteDialog = /** @class */ (function (_super) {
    __extends(PageCollectionDeleteDialog, _super);
    function PageCollectionDeleteDialog(collectionId, options) {
        var _this = _super.call(this, options) || this;
        _this.collectionId = collectionId;
        return _this;
    }
    Object.defineProperty(PageCollectionDeleteDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageCollectionDeleteDialog"; },
        enumerable: false,
        configurable: true
    });
    PageCollectionDeleteDialog.prototype._onRenderContent = function () {
        this.setHeader("Удаление коллекции страниц");
        _super.prototype._onRenderContent.call(this);
    };
    PageCollectionDeleteDialog.prototype._getText = function () {
        return "Подтвердите удаление коллекции страниц.";
    };
    PageCollectionDeleteDialog.prototype._buildUrl = function () {
        return "/brandup.pages/collection/".concat(this.collectionId);
    };
    PageCollectionDeleteDialog.prototype._buildUrlParams = function (urlParams) {
    };
    return PageCollectionDeleteDialog;
}(DeleteDialog));
export var deletePageCollection = function (collectionId) {
    var dialog = new PageCollectionDeleteDialog(collectionId);
    return dialog.open();
};
