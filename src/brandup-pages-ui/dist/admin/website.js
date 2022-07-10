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
import { UIElement, DOM, ajaxRequest } from "brandup-ui";
import ContentPage from "../pages/content";
import { browserPage } from "../dialogs/pages/browser";
import { listPageEditor } from "../dialogs/editors/list";
import iconBack from "../svg/toolbar-button-back.svg";
import iconList from "../svg/toolbar-button-list.svg";
import iconTree from "../svg/toolbar-button-tree.svg";
import iconWebsite from "../svg/toolbar-button-website.svg";
import { listContentType } from "../dialogs/content-types/list";
var WebSiteToolbar = /** @class */ (function (_super) {
    __extends(WebSiteToolbar, _super);
    function WebSiteToolbar(page) {
        var _this = _super.call(this) || this;
        document.body.classList.add("bp-state-toolbars");
        var isContentPage = page instanceof ContentPage;
        var buttons = [];
        if (isContentPage && page.model.parentPageId) {
            buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-back", title: "Перейти к родительской странице" }, iconBack));
        }
        buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-website", title: "Web-сайт" }, iconWebsite));
        buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-pages", title: "Страницы этого уровня" }, iconList));
        if (isContentPage) {
            buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-pages-child", title: "Дочерние страницы" }, iconTree));
        }
        var toolbarElem = DOM.tag("div", { class: "bp-elem bp-toolbar" }, buttons);
        toolbarElem.appendChild(DOM.tag("div", { class: "bp-toolbar-menu" }, [
            DOM.tag("a", { href: "", "data-command": "bp-editors" }, "Редакторы страниц"),
            DOM.tag("a", { href: "", "data-command": "bp-content-types" }, "Типы контента"),
            //DOM.tag("a", { href: "", "data-command": "bp-page-types" }, "Типы страниц"),
            //DOM.tag("a", { href: "", "data-command": "bp-recyclebin" }, "Корзина")
        ]));
        document.body.appendChild(toolbarElem);
        _this.setElement(toolbarElem);
        _this.registerCommand("bp-back", function () {
            var parentPageId = null;
            if (isContentPage)
                parentPageId = page.model.parentPageId;
            if (parentPageId) {
                ajaxRequest({
                    url: "/brandup.pages/page/".concat(parentPageId),
                    success: function (response) {
                        page.website.nav({ url: response.data.url });
                    }
                });
            }
        });
        _this.registerCommand("bp-pages", function () {
            var parentPageId = null;
            if (isContentPage)
                parentPageId = page.model.parentPageId;
            browserPage(parentPageId);
        });
        _this.registerCommand("bp-pages-child", function () {
            var parentPageId = null;
            if (isContentPage)
                parentPageId = page.model.id;
            browserPage(parentPageId);
        });
        _this.registerCommand("bp-website", function () {
            if (!toolbarElem.classList.toggle("opened-menu")) {
                document.body.removeEventListener("click", _this.__closeMenuFunc);
                return;
            }
            document.body.addEventListener("click", _this.__closeMenuFunc);
        });
        _this.registerCommand("bp-editors", function () {
            toolbarElem.classList.remove("opened-menu");
            listPageEditor();
        });
        _this.registerCommand("bp-content-types", function () {
            toolbarElem.classList.remove("opened-menu");
            listContentType();
        });
        _this.__closeMenuFunc = function (e) {
            var target = e.target;
            if (!target.closest(".bp-toolbar-menu")) {
                toolbarElem.classList.remove("opened-menu");
                document.body.removeEventListener("click", _this.__closeMenuFunc);
                return;
            }
        };
        return _this;
    }
    Object.defineProperty(WebSiteToolbar.prototype, "typeName", {
        get: function () { return "BrandUpPages.WebSiteToolbar"; },
        enumerable: false,
        configurable: true
    });
    WebSiteToolbar.prototype.destroy = function () {
        this.element.remove();
        _super.prototype.destroy.call(this);
    };
    return WebSiteToolbar;
}(UIElement));
export { WebSiteToolbar };
