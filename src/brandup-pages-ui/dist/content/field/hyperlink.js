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
import iconArrow from "../../svg/combobox-arrow.svg";
import "./hyperlink.less";
var HyperLinkContent = /** @class */ (function (_super) {
    __extends(HyperLinkContent, _super);
    function HyperLinkContent(form, name, options) {
        var _this = _super.call(this, name, options) || this;
        _this.__type = "Page";
        _this.form = form;
        return _this;
    }
    Object.defineProperty(HyperLinkContent.prototype, "typeName", {
        get: function () { return "BrandUpPages.Form.Field.HyperLink"; },
        enumerable: false,
        configurable: true
    });
    HyperLinkContent.prototype._onRender = function () {
        var _this = this;
        _super.prototype._onRender.call(this);
        this.element.classList.add("hyperlink");
        this.element.appendChild(DOM.tag("div", { class: "value-type", "data-command": "open-types-menu" }, [
            this.__typeElem = DOM.tag("span", null, "Page"),
            iconArrow
        ]));
        this.element.appendChild(this.__valueElem = DOM.tag("div", { class: "value", "data-command": "begin-input" }));
        this.element.appendChild(this.__placeholderElem = DOM.tag("div", { class: "placeholder", "data-command": "begin-input" }));
        this.__urlValueInput = DOM.tag("input", { type: "text", class: "url" });
        this.__urlValueInput.addEventListener("change", function () {
            _this.__refreshUI();
            _this.form.request(_this, {
                url: "/brandup.pages/content/hyperlink/url",
                urlParams: {
                    url: _this.__urlValueInput.value
                },
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
        this.__urlValueInput.addEventListener("blur", function () {
            _this.element.classList.remove("inputing");
            _this.__valueElem.innerText = _this.__urlValueInput.value;
        });
        this.element.appendChild(this.__urlValueInput);
        this.__pageValueInput = DOM.tag("input", { type: "text", class: "page" });
        this.__pageValueInput.addEventListener("keyup", function () {
            var title = _this.__pageValueInput.value;
            if (!title || title.length < 3)
                return;
            if (_this.__searchTimeout)
                clearTimeout(_this.__searchTimeout);
            if (_this.__searchRequest)
                _this.__searchRequest.abort();
            _this.__searchTimeout = window.setTimeout(function () {
                _this.__searchRequest = ajaxRequest({
                    url: "/brandup.pages/page/search",
                    urlParams: {
                        title: title
                    },
                    method: "GET",
                    success: function (response) {
                        switch (response.status) {
                            case 200:
                                DOM.empty(_this.__searchElem);
                                if (response.data.length) {
                                    for (var i = 0; i < response.data.length; i++) {
                                        var page = response.data[i];
                                        _this.__searchElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-page", "data-value": page.id }, page.title)));
                                    }
                                }
                                else
                                    _this.__searchElem.appendChild(DOM.tag("li", { class: "text" }, "Страниц не найдено"));
                                break;
                            default:
                                throw "";
                        }
                    }
                });
            }, 500);
        });
        this.element.appendChild(this.__pageValueInput);
        this.__typeMenuElem = DOM.tag("ul", { class: "hyperlink-menu types" }, [
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Page" }, "Page")),
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Url" }, "Url"))
        ]);
        this.element.appendChild(this.__typeMenuElem);
        this.__searchElem = DOM.tag("ul", { class: "hyperlink-menu pages" });
        this.element.appendChild(this.__searchElem);
        this.__closeTypeMenuFunc = function (e) {
            var t = e.target;
            if (!t.closest(".hyperlink-menu") && _this.element) {
                _this.element.classList.remove("opened-types");
                document.body.removeEventListener("click", _this.__closeTypeMenuFunc, false);
            }
        };
        this.__closePageMenuFunc = function (e) {
            var t = e.target;
            if (!t.closest(".hyperlink") && _this.element) {
                _this.element.classList.remove("inputing");
                _this.element.classList.remove("opened-pages");
                document.body.removeEventListener("click", _this.__closePageMenuFunc, false);
            }
        };
        this.registerCommand("open-types-menu", function () {
            if (_this.element.classList.contains("opened-types")) {
                _this.element.classList.remove("opened-types");
                document.body.removeEventListener("click", _this.__closeTypeMenuFunc, false);
                return;
            }
            if (_this.element.classList.contains("opened-pages")) {
                _this.element.classList.remove("inputing");
                _this.element.classList.remove("opened-pages");
                document.body.removeEventListener("click", _this.__closePageMenuFunc, false);
            }
            _this.element.classList.add("opened-types");
            document.body.addEventListener("mousedown", _this.__closeTypeMenuFunc, false);
        });
        this.registerCommand("begin-input", function () {
            _this.element.classList.add("inputing");
            switch (_this.__type) {
                case "Page":
                    if (!_this.element.classList.toggle("opened-pages")) {
                        document.body.removeEventListener("click", _this.__closePageMenuFunc, false);
                        return;
                    }
                    DOM.empty(_this.__searchElem);
                    _this.__searchElem.appendChild(DOM.tag("li", { class: "text" }, "Начните вводить название страницы или её url."));
                    _this.__pageValueInput.focus();
                    _this.__pageValueInput.select();
                    document.body.addEventListener("mousedown", _this.__closePageMenuFunc, false);
                    break;
                case "Url":
                    _this.__urlValueInput.focus();
                    break;
                default:
                    throw "";
            }
        });
        this.registerCommand("select-type", function (elem) {
            var type = elem.getAttribute("data-value");
            _this.element.classList.remove("opened-types");
            document.body.removeEventListener("click", _this.__closeTypeMenuFunc, false);
            _this.__type = type;
            _this.__refreshUI();
        });
        this.registerCommand("select-page", function (elem) {
            _this.element.classList.remove("inputing");
            _this.element.classList.remove("opened-pages");
            document.body.removeEventListener("click", _this.__closePageMenuFunc, false);
            var pageId = elem.getAttribute("data-value");
            _this.__pageValueInput.setAttribute("value-page-id", pageId);
            _this.__valueElem.innerText = elem.innerText;
            _this.__pageValueInput.value = elem.innerText;
            _this.__refreshUI();
            _this.form.request(_this, {
                url: "/brandup.pages/content/hyperlink/page",
                urlParams: {
                    pageId: pageId
                },
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
    HyperLinkContent.prototype.getValue = function () { throw "Not implemented"; };
    HyperLinkContent.prototype.setValue = function (value) {
        if (value) {
            this.__type = value.valueType;
            switch (value.valueType) {
                case "Page": {
                    this.__pageValueInput.setAttribute("value-page-id", value.value);
                    this.__valueElem.innerText = value.pageTitle;
                    this.__pageValueInput.value = value.pageTitle;
                    break;
                }
                case "Url": {
                    this.__urlValueInput.value = value.value;
                    this.__valueElem.innerText = value.value;
                    break;
                }
                default:
                    throw "";
            }
        }
        this.__refreshUI();
    };
    HyperLinkContent.prototype.hasValue = function () {
        switch (this.__type) {
            case "Page": {
                return this.__pageValueInput.hasAttribute("value-page-id");
            }
            case "Url": {
                return this.__urlValueInput.value ? true : false;
            }
            default:
                throw "";
        }
    };
    HyperLinkContent.prototype.__refreshUI = function () {
        if (this.hasValue())
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
        this.__typeElem.innerText = this.__type;
        switch (this.__type) {
            case "Page": {
                this.element.classList.remove("url-value");
                this.element.classList.add("page-value");
                this.__valueElem.innerText = this.__pageValueInput.value;
                this.__placeholderElem.innerText = "Выберите страницу";
                break;
            }
            case "Url": {
                this.element.classList.remove("page-value");
                this.element.classList.add("url-value");
                this.__valueElem.innerText = this.__urlValueInput.value;
                this.__placeholderElem.innerText = "Введите url";
                break;
            }
            default:
                throw "";
        }
    };
    HyperLinkContent.prototype.destroy = function () {
        if (this.__searchRequest)
            this.__searchRequest.abort();
        window.clearTimeout(this.__searchTimeout);
        document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
        document.body.removeEventListener("click", this.__closePageMenuFunc, false);
        _super.prototype.destroy.call(this);
    };
    return HyperLinkContent;
}(Field));
export { HyperLinkContent };
