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
import { DOM, Utility } from "brandup-ui";
import "./dialog.less";
import iconBack from "../svg/dialog-back.svg";
import iconClose from "../svg/dialog-close.svg";
import { UIControl } from "../control";
var dialogsPanelElem = DOM.tag("div", { class: "bp-elem bp-dialog-panel" });
var currentDialog = null;
document.body.appendChild(dialogsPanelElem);
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
        enumerable: false,
        configurable: true
    });
    Dialog.prototype._getHtmlTemplate = function () {
        return '<div class="bp-dialog-header">' +
            '    <span class="title"></span>' +
            '</div>' +
            '<div class="bp-dialog-content"></div>' +
            '<div class="bp-dialog-footer">' +
            '   <span class="notes"></span>' +
            '</div>';
    };
    Dialog.prototype._onRender = function () {
        var _this = this;
        this.element.classList.add("bp-dialog");
        this.headerElem = DOM.getElementByClass(this.element, "bp-dialog-header");
        this.headerTitleElem = DOM.getElementByClass(this.headerElem, "title");
        this.contentElem = DOM.getElementByClass(this.element, "bp-dialog-content");
        this.footerElem = DOM.getElementByClass(this.element, "bp-dialog-footer");
        this.footerNotesElem = DOM.getElementByClass(this.footerElem, "notes");
        if (this.options.header)
            this.setHeader(this.options.header);
        if (this.options.notes)
            this.setHeader(this.options.notes);
        if (this.__parentDialog) {
            this.headerElem.insertAdjacentElement("afterbegin", DOM.tag("a", { href: "", class: "button back", "data-command": "close" }, iconBack));
        }
        else {
            this.headerElem.insertAdjacentElement("beforeend", DOM.tag("a", { href: "", class: "button x", "data-command": "close" }, iconClose));
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
        this.headerTitleElem.innerHTML = html ? html : "";
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
        var b = DOM.tag("button", { class: "button", "data-command": name }, title);
        if (isAccent)
            b.classList.add("accent");
        this.footerElem.appendChild(b);
        this.element.classList.add("has-actions");
    };
    Dialog.prototype.setError = function (message) {
        this.element.classList.add("has-error");
        var list = DOM.tag("ul");
        if (Utility.isArray(message)) {
            for (var i = 0; i < message.length; i++) {
                list.appendChild(DOM.tag("li", null, message[i]));
            }
        }
        else
            list.appendChild(DOM.tag("li", null, message));
        this.__errorElem = DOM.tag("div", { class: "bp-dialog-error" }, list);
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
        this.destroy();
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
}(UIControl));
export { Dialog };
