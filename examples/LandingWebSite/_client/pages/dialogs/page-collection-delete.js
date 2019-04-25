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
    var PageCollectionDeleteDialog = /** @class */ (function (_super) {
        __extends(PageCollectionDeleteDialog, _super);
        function PageCollectionDeleteDialog(id, options) {
            var _this = _super.call(this, options) || this;
            _this.id = id;
            return _this;
        }
        Object.defineProperty(PageCollectionDeleteDialog.prototype, "typeName", {
            get: function () { return "BrandUpPages.PageCollectionDeleteDialog"; },
            enumerable: true,
            configurable: true
        });
        PageCollectionDeleteDialog.prototype._onRenderContent = function () {
            var _this = this;
            this.element.classList.add("website-dialog-delete");
            this.setHeader("Удаление коллекции страниц");
            this.addAction("close", "Отмена");
            this.addAction("confirm", "Удалить", true);
            this.registerCommand("confirm", function () {
                _this.__delete();
            });
            this.__textElem = brandup_ui_1.DOM.tag("div", { class: "confirm-text" }, "Подтвердите удаление коллекции страниц.");
            this.content.appendChild(this.__textElem);
            this.__errorsElem = brandup_ui_1.DOM.tag("div", { class: "errors" });
            this.content.appendChild(this.__errorsElem);
            brandup_ui_1.ajaxRequest({
                url: "/brandup.pages/collection/" + this.id,
                success: function (data, status) {
                    switch (status) {
                        case 200:
                            break;
                        case 404:
                            break;
                        default:
                            throw "";
                    }
                }
            });
        };
        PageCollectionDeleteDialog.prototype.__delete = function () {
            var _this = this;
            this.setLoading(true);
            brandup_ui_1.ajaxRequest({
                url: "/brandup.pages/collection/" + this.id,
                method: "DELETE",
                success: function (data, status) {
                    _this.setLoading(false);
                    switch (status) {
                        case 400: {
                            _this.__renderErrors(data.errors);
                            break;
                        }
                        case 200: {
                            _this.resolve(_this.id);
                            return;
                        }
                        default:
                            throw "";
                    }
                }
            });
        };
        PageCollectionDeleteDialog.prototype.__renderErrors = function (errors) {
            brandup_ui_1.DOM.empty(this.__errorsElem);
            if (errors) {
                var elem = brandup_ui_1.DOM.tag("ul");
                for (var i = 0; i < errors.length; i++) {
                    elem.appendChild(brandup_ui_1.DOM.tag("li", null, errors[i]));
                }
                this.__errorsElem.appendChild(elem);
            }
        };
        return PageCollectionDeleteDialog;
    }(dialog_1.Dialog));
    exports.deletePageCollection = function (id) {
        var dialog = new PageCollectionDeleteDialog(id);
        return dialog.open();
    };
});
//# sourceMappingURL=page-collection-delete.js.map