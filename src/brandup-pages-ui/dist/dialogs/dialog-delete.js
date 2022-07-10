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
import { DOM, ajaxRequest } from "brandup-ui";
import "./dialog-delete.less";
var DeleteDialog = /** @class */ (function (_super) {
    __extends(DeleteDialog, _super);
    function DeleteDialog() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DeleteDialog.prototype._onRenderContent = function () {
        var _this = this;
        this.element.classList.add("bp-dialog-delete");
        this.addAction("close", "Отмена");
        this.addAction("confirm", "Удалить", true);
        this.registerCommand("confirm", function () {
            _this.__delete();
        });
        this.__textElem = DOM.tag("div", { class: "confirm-text" }, this._getText());
        this.content.appendChild(this.__textElem);
        this.__errorsElem = DOM.tag("div", { class: "errors" });
        this.content.appendChild(this.__errorsElem);
        var urlParams = {};
        this._buildUrlParams(urlParams);
        this.setLoading(true);
        ajaxRequest({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "GET",
            success: function (response) {
                _this.setLoading(false);
                switch (response.status) {
                    case 404: {
                        _this.setError("Данные для удаления не найдены.");
                        break;
                    }
                    case 200: {
                        _this.__item = response.data;
                        break;
                    }
                    default:
                        throw "";
                }
            }
        });
    };
    DeleteDialog.prototype.__delete = function () {
        var _this = this;
        this.setLoading(true);
        var urlParams = {};
        this._buildUrlParams(urlParams);
        ajaxRequest({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "DELETE",
            success: function (response) {
                _this.setLoading(false);
                switch (response.status) {
                    case 400: {
                        _this.__renderErrors(response.data.errors);
                        break;
                    }
                    case 200: {
                        _this.resolve(_this.__item);
                        return;
                    }
                    default:
                        throw "";
                }
            }
        });
    };
    DeleteDialog.prototype.__renderErrors = function (errors) {
        DOM.empty(this.__errorsElem);
        if (errors) {
            var elem = DOM.tag("ul");
            for (var i = 0; i < errors.length; i++) {
                elem.appendChild(DOM.tag("li", null, errors[i]));
            }
            this.__errorsElem.appendChild(elem);
        }
    };
    return DeleteDialog;
}(Dialog));
export { DeleteDialog };
