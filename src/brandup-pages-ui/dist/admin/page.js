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
import { UIElement, DOM } from "brandup-ui";
import { PageDesigner } from "../content/designer/page";
import { editPage } from "../dialogs/pages/edit";
import { publishPage } from "../dialogs/pages/publish";
import { seoPage } from "../dialogs/pages/seo";
import iconDiscard from "../svg/toolbar-button-discard.svg";
import iconEdit from "../svg/toolbar-button-edit.svg";
import iconPublish from "../svg/toolbar-button-publish.svg";
import iconSave from "../svg/toolbar-button-save.svg";
import iconSettings from "../svg/toolbar-button-settings.svg";
import iconSeo from "../svg/toolbar-button-seo.svg";
var PageToolbar = /** @class */ (function (_super) {
    __extends(PageToolbar, _super);
    function PageToolbar(page) {
        var _this = _super.call(this) || this;
        //page.attachDestroyFunc(() => { this.destroy(); });
        var toolbarElem = DOM.tag("div", { class: "bp-elem bp-toolbar bp-toolbar-right" });
        var isLoading = false;
        if (page.model.editId) {
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-content", title: "Редактор контента" }, iconSettings));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-commit", title: "Применить изменения к странице" }, iconSave));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-discard", title: "Отменить изменения" }, iconDiscard));
            //let cancelNav = true;
            _this.registerCommand("bp-content", function () {
                editPage(page.model.editId).then(function () {
                    page.website.app.reload();
                });
            });
            _this.registerCommand("bp-commit", function () {
                if (isLoading)
                    return;
                isLoading = true;
                page.website.request({
                    url: "/brandup.pages/page/content/commit",
                    urlParams: { editId: page.model.editId },
                    method: "POST",
                    success: function (response) {
                        //cancelNav = false;
                        if (response.status !== 200)
                            throw "";
                        page.website.nav({ url: response.data, replace: true });
                        isLoading = false;
                    }
                }, true);
            });
            _this.registerCommand("bp-discard", function () {
                if (isLoading)
                    return;
                isLoading = true;
                page.website.request({
                    url: "/brandup.pages/page/content/discard",
                    urlParams: { editId: page.model.editId },
                    method: "POST",
                    success: function (response) {
                        //cancelNav = false;
                        if (response.status !== 200)
                            throw "";
                        page.website.nav({ url: response.data, replace: true });
                        isLoading = false;
                    }
                });
            });
            _this.__designer = new PageDesigner(page);
            //this.__pageNavFunc = (e: CustomEvent<NavigationOptions>) => {
            //    if (cancelNav && e.detail.pushState) {
            //        e.cancelBubble = true;
            //        e.stopPropagation();
            //        e.preventDefault();
            //    }
            //    else {
            //        e.cancelBubble = false;
            //    }
            //    cancelNav = true;
            //};
            //window.addEventListener("pageNavigating", this.__pageNavFunc, false);
        }
        else {
            if (page.model.status !== "Published") {
                toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-publish" }, iconPublish));
                _this.registerCommand("bp-publish", function () {
                    publishPage(page.model.id).then(function (result) {
                        page.website.nav({ url: result.url, replace: true });
                    });
                });
            }
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-seo", title: "Параметры индексирования страницы" }, iconSeo));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-edit", title: "Редактировать контент страницы" }, iconEdit));
            _this.registerCommand("bp-edit", function () {
                if (isLoading)
                    return;
                isLoading = true;
                page.website.request({
                    url: "/brandup.pages/page/content/begin",
                    urlParams: { pageId: page.model.id },
                    method: "POST",
                    success: function (response) {
                        isLoading = false;
                        if (response.status !== 200)
                            throw "";
                        if (response.data.currentDate) {
                            var popup = DOM.tag("div", { class: "bp-toolbar-popup" }, [
                                DOM.tag("div", { class: "text" }, "Ранее вы не завершили редактирование этой страницы."),
                                DOM.tag("div", { class: "buttons" }, [
                                    DOM.tag("button", { "data-command": "continue-edit", "data-value": response.data.url }, "Продолжить"),
                                    DOM.tag("button", { "data-command": "restart-edit" }, "Начать заново")
                                ])
                            ]);
                            _this.element.appendChild(popup);
                            _this.setPopup(popup);
                        }
                        else
                            page.website.nav({ url: response.data.url, replace: true });
                    }
                });
            });
            _this.registerCommand("bp-seo", function () {
                seoPage(page.model.id).then(function () {
                    page.website.app.reload();
                });
            });
            _this.registerCommand("continue-edit", function (elem) {
                _this.setPopup(null);
                var url = elem.getAttribute("data-value");
                page.website.nav({ url: url, replace: true });
            });
            _this.registerCommand("restart-edit", function () {
                _this.setPopup(null);
                page.website.request({
                    url: "/brandup.pages/page/content/begin",
                    urlParams: { pageId: page.model.id, force: "true" },
                    method: "POST",
                    success: function (response) {
                        isLoading = false;
                        if (response.status !== 200)
                            throw "";
                        page.website.nav({ url: response.data.url, replace: true });
                    }
                });
            });
            _this.__closePopupFunc = function (e) {
                var t = e.target;
                if (!t.closest(".bp-toolbar-popup")) {
                    _this.__popupElem.remove();
                    _this.__popupElem = null;
                    document.body.removeEventListener("click", _this.__closePopupFunc);
                }
            };
        }
        document.body.appendChild(toolbarElem);
        _this.setElement(toolbarElem);
        return _this;
    }
    Object.defineProperty(PageToolbar.prototype, "typeName", {
        //private __pageNavFunc: (e: CustomEvent) => void;
        get: function () { return "BrandUpPages.PageToolbar"; },
        enumerable: false,
        configurable: true
    });
    PageToolbar.prototype.setPopup = function (popup) {
        if (this.__popupElem) {
            this.__popupElem.remove();
            this.__popupElem = null;
            document.body.removeEventListener("click", this.__closePopupFunc);
        }
        if (popup) {
            this.__popupElem = popup;
            document.body.addEventListener("click", this.__closePopupFunc);
        }
    };
    PageToolbar.prototype.destroy = function () {
        if (this.__designer) {
            this.__designer.destroy();
            this.__designer = null;
        }
        this.element.remove();
        //window.removeEventListener("pageNavigating", this.__pageNavFunc, false);
        _super.prototype.destroy.call(this);
    };
    return PageToolbar;
}(UIElement));
export { PageToolbar };
