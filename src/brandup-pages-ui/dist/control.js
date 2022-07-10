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
import { UIElement, Utility } from "brandup-ui";
var UIControl = /** @class */ (function (_super) {
    __extends(UIControl, _super);
    function UIControl(options, element) {
        var _this = _super.call(this) || this;
        _this.options = {};
        _this.isInject = false;
        var tagName = _this._getTagName();
        if (!tagName)
            throw new Error();
        if (!element) {
            _this.__fragment = document.createDocumentFragment();
            element = document.createElement(tagName);
            _this.__fragment.appendChild(element);
        }
        else
            _this.isInject = true;
        _this.setElement(element);
        _this._onApplyDefaultOptions();
        _this._applyOptions(options);
        _this._onInitialize();
        return _this;
    }
    // Options
    UIControl.prototype._onApplyDefaultOptions = function () { return; };
    UIControl.prototype._applyOptions = function (options) {
        if (options)
            Utility.extend(this.options, options);
    };
    // Render
    UIControl.prototype._getTagName = function () {
        return "div";
    };
    UIControl.prototype._getHtmlTemplate = function () {
        return null;
    };
    UIControl.prototype.render = function (container, position) {
        if (position === void 0) { position = "afterbegin"; }
        if (container) {
            if (!this.__fragment)
                throw new Error();
            if (Utility.isString(container)) {
                container = document.getElementById(container.substr(1));
                if (!container)
                    throw new Error();
            }
        }
        var htmlTemplate = this._getHtmlTemplate();
        if (htmlTemplate)
            this.element.insertAdjacentHTML(position, htmlTemplate);
        if (this.__fragment) {
            container.appendChild(this.__fragment);
            delete this.__fragment;
        }
        this._onRender();
        return this;
    };
    UIControl.prototype.destroy = function () {
        if (!this.isInject && this.element)
            this.element.remove();
        _super.prototype.destroy.call(this);
    };
    UIControl.prototype._onInitialize = function () { return; };
    return UIControl;
}(UIElement));
export { UIControl };
