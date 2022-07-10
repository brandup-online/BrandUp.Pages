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
import { UIElement } from "brandup-ui";
import "./base.less";
var FieldDesigner = /** @class */ (function (_super) {
    __extends(FieldDesigner, _super);
    function FieldDesigner(page, elem, options) {
        var _this = _super.call(this) || this;
        _this.page = page;
        _this.options = options;
        _this.path = elem.getAttribute("content-path");
        _this.name = _this.fullPath = elem.getAttribute("content-field");
        if (_this.path)
            _this.fullPath = _this.path + "." + _this.fullPath;
        _this.setElement(elem);
        elem.classList.add("field-designer");
        _this.onRender(elem);
        return _this;
    }
    FieldDesigner.prototype.request = function (options) {
        if (!options.urlParams)
            options.urlParams = {};
        options.urlParams["editId"] = this.page.editId;
        options.urlParams["path"] = this.path;
        options.urlParams["field"] = this.name;
        this.page.queue.push(options);
    };
    return FieldDesigner;
}(UIElement));
export { FieldDesigner };
