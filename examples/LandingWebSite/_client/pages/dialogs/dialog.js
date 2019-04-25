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
define(["require", "exports", "brandup-ui", "./dialog.less"], function (require, exports, brandup_ui_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var Dialog = /** @class */ (function (_super) {
        __extends(Dialog, _super);
        function Dialog(options) {
            var _this = _super.call(this, options) || this;
            _this.__parentDialog = null;
            _this.__childDialog = null;
            return _this;
        }
        Object.defineProperty(Dialog.prototype, "content", {
            get: function () {
                return this.contentElem;
            },
            enumerable: true,
            configurable: true
        });
        Dialog.prototype._getHtmlTemplate = function () {
            return '<div class="website-dialog-header">' +
                '    <span class="title"></span>' +
                '</div>' +
                '<div class="website-dialog-content"></div>' +
                '<div class="website-dialog-footer">' +
                '   <span class="notes"></span>' +
                '</div>';
        };
        Dialog.prototype._onRender = function () {
            var _this = this;
            this.element.classList.add("website-dialog");
            this.headerElem = brandup_ui_1.DOM.getElementByClass(this.element, "website-dialog-header");
            this.headerTitleElem = brandup_ui_1.DOM.getElementByClass(this.headerElem, "title");
            this.contentElem = brandup_ui_1.DOM.getElementByClass(this.element, "website-dialog-content");
            this.footerElem = brandup_ui_1.DOM.getElementByClass(this.element, "website-dialog-footer");
            this.footerNotesElem = brandup_ui_1.DOM.getElementByClass(this.footerElem, "notes");
            if (this.options.header)
                this.setHeader(this.options.header);
            if (this.options.notes)
                this.setHeader(this.options.notes);
            if (this.options.disablePageScroll)
                document.body.classList.add("");
            if (this.__parentDialog) {
                this.headerElem.insertAdjacentElement("afterbegin", brandup_ui_1.DOM.tag("a", { href: "", class: "button back", "data-command": "close" }));
            }
            else {
                this.headerElem.insertAdjacentElement("beforeend", brandup_ui_1.DOM.tag("a", { href: "", class: "button x", "data-command": "close" }));
            }
            this.registerCommand("close", function () {
                _this._onClose();
            });
            this._onRenderContent();
        };
        Dialog.prototype._onClose = function () {
            this.destroy();
        };
        Dialog.prototype.setHeader = function (html) {
            if (html) {
                this.headerTitleElem.innerHTML = html;
                this.element.classList.add("has-header");
            }
            else {
                this.headerTitleElem.innerText = "";
                this.element.classList.remove("has-header");
            }
        };
        Dialog.prototype.setNotes = function (html) {
            if (html) {
                this.footerNotesElem.innerHTML = html;
                this.element.classList.add("has-notes");
            }
            else {
                this.footerNotesElem.innerHTML = "";
                this.element.classList.remove("has-notes");
            }
        };
        Dialog.prototype.setLoading = function (value) {
            if (value)
                this.element.classList.add("loading");
            else
                this.element.classList.remove("loading");
        };
        Dialog.prototype.addAction = function (name, title, isAccent) {
            if (isAccent === void 0) { isAccent = false; }
            var b = brandup_ui_1.DOM.tag("button", { class: "button", "data-command": name }, title);
            if (isAccent)
                b.classList.add("accent");
            this.footerElem.appendChild(b);
            this.element.classList.add("has-actions");
        };
        Dialog.prototype.setError = function (title, notes) {
            this.element.classList.add("has-error");
            this.__errorElem = brandup_ui_1.DOM.tag("div", { class: "website-dialog-error" }, title);
            this.content.insertAdjacentElement("beforebegin", this.__errorElem);
        };
        Dialog.prototype.removeError = function () {
            this.element.classList.remove("has-error");
            if (this.__errorElem) {
                this.__errorElem.remove();
                this.__errorElem = null;
            }
        };
        Dialog.prototype.open = function () {
            var _this = this;
            if (currentDialog) {
                currentDialog.element.classList.add("hide");
                this.__parentDialog = currentDialog;
                currentDialog.__childDialog = this;
            }
            else
                document.body.classList.add("website-state-showdialog");
            currentDialog = this;
            this.render(dialogsPanelElem);
            this.element.classList.add("opened");
            return new Promise(function (resolve, reject) {
                _this.__resolve = resolve;
                _this.__reject = reject;
            });
        };
        Dialog.prototype.resolve = function (value) {
            if (this.__resolve)
                this.__resolve(value);
            this.destroy();
        };
        Dialog.prototype.reject = function (reason) {
            if (this.__reject)
                this.__reject(reason);
        };
        Dialog.prototype.destroy = function () {
            if (this.__childDialog) {
                this.__childDialog.destroy();
                this.__childDialog = null;
            }
            if (!this.__parentDialog) {
                currentDialog = null;
                document.body.classList.remove("website-state-showdialog");
            }
            else {
                currentDialog = this.__parentDialog;
                currentDialog.element.classList.remove("hide");
                this.__parentDialog.__childDialog = null;
                this.__parentDialog = null;
            }
            _super.prototype.destroy.call(this);
        };
        return Dialog;
    }(brandup_ui_1.UIControl));
    exports.Dialog = Dialog;
    var dialogsPanelElem = brandup_ui_1.DOM.tag("div", { class: "brandup-pages-elem website-dialogs-panel" });
    var currentDialog = null;
    document.body.appendChild(dialogsPanelElem);
});
//# sourceMappingURL=dialog.js.map