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
var PageEditorAssignDialog = /** @class */ (function (_super) {
    __extends(PageEditorAssignDialog, _super);
    function PageEditorAssignDialog(options) {
        return _super.call(this, options) || this;
    }
    Object.defineProperty(PageEditorAssignDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageEditorAssignDialog"; },
        enumerable: false,
        configurable: true
    });
    PageEditorAssignDialog.prototype._getSaveButtonTitle = function () {
        return "Назначить";
    };
    PageEditorAssignDialog.prototype._buildUrl = function () {
        return "/brandup.pages/editor/assign";
    };
    PageEditorAssignDialog.prototype._buildUrlParams = function (urlParams) {
    };
    PageEditorAssignDialog.prototype._buildForm = function (model) {
        this.setHeader("Назначение редактора");
        this.addTextBox("Email", "E-mail", { placeholder: "Введите e-mail пользователя" });
    };
    return PageEditorAssignDialog;
}(FormDialog));
export { PageEditorAssignDialog };
export var assignPageEditor = function () {
    var dialog = new PageEditorAssignDialog();
    return dialog.open();
};
