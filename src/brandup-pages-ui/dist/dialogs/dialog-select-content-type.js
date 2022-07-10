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
import { Dialog } from "./dialog";
import { DOM } from "brandup-ui";
import "./dialog-select-content-type.less";
var SelectContentTypeDialog = /** @class */ (function (_super) {
    __extends(SelectContentTypeDialog, _super);
    function SelectContentTypeDialog(types, options) {
        var _this = _super.call(this, options) || this;
        _this.__types = types;
        return _this;
    }
    Object.defineProperty(SelectContentTypeDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.SelectItemTypeDialog"; },
        enumerable: false,
        configurable: true
    });
    SelectContentTypeDialog.prototype._onRenderContent = function () {
        var _this = this;
        this.element.classList.add("bp-dialog-select-content-type");
        this.setHeader("Выберите тип контента");
        this.__types.map(function (type, index) {
            var itemElem = DOM.tag("a", { class: "item", href: "", "data-command": "select", "data-index": index }, type.title);
            _this.content.appendChild(itemElem);
        });
        this.registerCommand("select", function (elem) {
            var index = parseInt(elem.getAttribute("data-index"));
            var type = _this.__types[index];
            _this.resolve(type);
        });
    };
    return SelectContentTypeDialog;
}(Dialog));
export { SelectContentTypeDialog };
export var selectContentType = function (types) {
    var dialog = new SelectContentTypeDialog(types);
    return dialog.open();
};
