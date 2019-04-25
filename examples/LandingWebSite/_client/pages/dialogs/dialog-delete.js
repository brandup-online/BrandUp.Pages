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
define(["require", "exports", "./dialog", "brandup-ui"], function (require, exports, dialog_1, brandup_ui_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var DeleteDialog = /** @class */ (function (_super) {
        __extends(DeleteDialog, _super);
        function DeleteDialog() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        DeleteDialog.prototype._onRenderContent = function () {
            var _this = this;
            this.element.classList.add("website-dialog-delete");
            this.addAction("close", "Отмена");
            this.addAction("confirm", "Удалить", true);
            this.registerCommand("confirm", function () {
                _this.__delete();
            });
            this.__textElem = brandup_ui_1.DOM.tag("div", { class: "confirm-text" }, this._getText());
            this.content.appendChild(this.__textElem);
            this.__errorsElem = brandup_ui_1.DOM.tag("div", { class: "errors" });
            this.content.appendChild(this.__errorsElem);
            var urlParams = {};
            this._buildUrlParams(urlParams);
            this.setLoading(true);
            brandup_ui_1.ajaxRequest({
                url: this._buildUrl(),
                urlParams: urlParams,
                method: "GET",
                success: function (data, status) {
                    _this.setLoading(false);
                    switch (status) {
                        case 404: {
                            _this.setError("Данные для удаления не найдены.");
                            break;
                        }
                        case 200: {
                            _this.__item = data;
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
            brandup_ui_1.ajaxRequest({
                url: this._buildUrl(),
                urlParams: urlParams,
                method: "DELETE",
                success: function (data, status) {
                    _this.setLoading(false);
                    switch (status) {
                        case 400: {
                            _this.__renderErrors(data.errors);
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
            brandup_ui_1.DOM.empty(this.__errorsElem);
            if (errors) {
                var elem = brandup_ui_1.DOM.tag("ul");
                for (var i = 0; i < errors.length; i++) {
                    elem.appendChild(brandup_ui_1.DOM.tag("li", null, errors[i]));
                }
                this.__errorsElem.appendChild(elem);
            }
        };
        return DeleteDialog;
    }(dialog_1.Dialog));
    exports.DeleteDialog = DeleteDialog;
});
//# sourceMappingURL=dialog-delete.js.map