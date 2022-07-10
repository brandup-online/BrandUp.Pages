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
import { Field } from "../../form/field";
import { DOM, ajaxRequest } from "brandup-ui";
import "./pages.less";
var PagesContent = /** @class */ (function (_super) {
    __extends(PagesContent, _super);
    function PagesContent(form, name, options) {
        var _this = _super.call(this, name, options) || this;
        _this.form = form;
        return _this;
    }
    Object.defineProperty(PagesContent.prototype, "typeName", {
        get: function () { return "BrandUpPages.Form.Field.Pages"; },
        enumerable: false,
        configurable: true
    });
    PagesContent.prototype._onRender = function () {
        var _this = this;
        _super.prototype._onRender.call(this);
        this.element.classList.add("pages");
        this.element.appendChild(this.inputElem = DOM.tag("input", { type: "text" }));
        this.element.appendChild(this.valueElem = DOM.tag("div", { class: "value", "data-command": "begin-input" }));
        this.element.appendChild(DOM.tag("div", { class: "placeholder", "data-command": "begin-input" }, this.options.placeholder));
        this.element.appendChild(this.searchElem = DOM.tag("ul", { class: "pages-menu" }));
        this.inputElem.addEventListener("keyup", function () {
            var title = _this.inputElem.value;
            if (!title || title.length < 3)
                return;
            if (_this.__searchTimeout)
                clearTimeout(_this.__searchTimeout);
            if (_this.__searchRequest)
                _this.__searchRequest.abort();
            _this.__searchTimeout = window.setTimeout(function () {
                _this.__searchRequest = ajaxRequest({
                    url: "/brandup.pages/collection/search",
                    urlParams: {
                        pageType: _this.options.pageType,
                        title: title
                    },
                    method: "GET",
                    success: function (response) {
                        switch (response.status) {
                            case 200:
                                DOM.empty(_this.searchElem);
                                if (response.data.length) {
                                    for (var i = 0; i < response.data.length; i++) {
                                        var collection = response.data[i];
                                        _this.searchElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select", "data-value": collection.id, "data-url": collection.pageUrl }, collection.title + ": " + collection.pageUrl)));
                                    }
                                }
                                else {
                                    _this.searchElem.appendChild(DOM.tag("li", { class: "text" }, "Коллекций страниц не найдено"));
                                }
                                break;
                            default:
                                throw "";
                        }
                    }
                });
            }, 500);
        });
        this.__closeMenuFunc = function (e) {
            var t = e.target;
            if (!t.closest(".pages") && _this.element) {
                _this.element.classList.remove("inputing");
                document.body.removeEventListener("click", _this.__closeMenuFunc, false);
            }
        };
        this.registerCommand("begin-input", function () {
            if (!_this.element.classList.toggle("inputing")) {
                document.body.removeEventListener("click", _this.__closeMenuFunc, false);
                return;
            }
            _this.element.classList.add("inputing");
            DOM.empty(_this.searchElem);
            _this.searchElem.appendChild(DOM.tag("li", { class: "text" }, "Начните вводить название коллекции страниц."));
            _this.inputElem.focus();
            _this.inputElem.select();
            document.body.addEventListener("mousedown", _this.__closeMenuFunc, false);
        });
        this.registerCommand("select", function (elem) {
            _this.element.classList.remove("inputing");
            document.body.removeEventListener("click", _this.__closeMenuFunc, false);
            var pageCollectionId = elem.getAttribute("data-value");
            var pageUrl = elem.getAttribute("data-url");
            _this.setValue({
                id: pageCollectionId,
                title: elem.innerText,
                pageUrl: pageUrl
            });
            _this.form.request(_this, {
                url: "/brandup.pages/content/pages",
                urlParams: { pageCollectionId: pageCollectionId },
                method: "POST",
                success: function (response) {
                    switch (response.status) {
                        case 200:
                            _this.setValue(response.data);
                            break;
                        default:
                            throw "";
                    }
                }
            });
        });
        this.__refreshUI();
    };
    PagesContent.prototype.getValue = function () { throw new Error("Method not implemented."); };
    PagesContent.prototype.setValue = function (value) {
        if (!value) {
            this.inputElem.removeAttribute("value-collection-id");
            this.inputElem.value = "";
            this.valueElem.innerText = "";
        }
        else {
            this.inputElem.setAttribute("value-collection-id", value.id);
            this.inputElem.value = value.title;
            this.valueElem.innerText = value.title + ": " + value.pageUrl;
        }
        this.__refreshUI();
    };
    PagesContent.prototype.hasValue = function () {
        return this.inputElem.hasAttribute("value-collection-id");
    };
    PagesContent.prototype.__refreshUI = function () {
        if (this.hasValue())
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    };
    PagesContent.prototype.destroy = function () {
        if (this.__searchRequest)
            this.__searchRequest.abort();
        window.clearTimeout(this.__searchTimeout);
        document.body.removeEventListener("click", this.__closeMenuFunc, false);
        _super.prototype.destroy.call(this);
    };
    return PagesContent;
}(Field));
export { PagesContent };
