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
import { DOM } from "brandup-ui";
import { UIControl } from "../control";
import "./field.less";
var Field = /** @class */ (function (_super) {
    __extends(Field, _super);
    function Field(name, options) {
        var _this = _super.call(this, options) || this;
        _this.name = name;
        return _this;
    }
    Field.prototype._onRender = function () {
        this.element.classList.add("website-form-field");
        this.defineEvent("changed", { bubbles: true, cancelable: false });
    };
    Field.prototype.raiseChanged = function () {
        this.raiseEvent("changed", {
            field: this,
            value: this.getValue()
        });
    };
    Field.prototype.setErrors = function (errors) {
        this.element.classList.remove("has-errors");
        if (this.__errorsElem) {
            this.__errorsElem.remove();
            this.__errorsElem = null;
        }
        if (!errors || errors.length === 0) {
            return;
        }
        this.element.classList.add("has-errors");
        this.__errorsElem = DOM.tag("ul", { class: "field-errors" });
        for (var i = 0; i < errors.length; i++)
            this.__errorsElem.appendChild(DOM.tag("li", null, errors[i]));
        this.element.insertAdjacentElement("afterend", this.__errorsElem);
    };
    return Field;
}(UIControl));
export { Field };
