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
define(["require", "exports", "brandup-ui", "./dialogs/page-collection-list", "./styles.less"], function (require, exports, brandup_ui_1, page_collection_list_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var BrandUpPages = /** @class */ (function (_super) {
        __extends(BrandUpPages, _super);
        function BrandUpPages() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        Object.defineProperty(BrandUpPages.prototype, "typeName", {
            get: function () { return "BrandUpPages.Toolbar"; },
            enumerable: true,
            configurable: true
        });
        BrandUpPages.prototype.load = function () {
            this.__renderToolbars();
            this.__registerCommands();
        };
        BrandUpPages.prototype.__renderToolbars = function () {
            var toolbarElem = brandup_ui_1.DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar" }, [
                brandup_ui_1.DOM.tag("button", { class: "brandup-pages-toolbar-button list", "data-command": "brandup-pages-collections" })
            ]);
            document.body.appendChild(toolbarElem);
            this.setElement(toolbarElem);
        };
        BrandUpPages.prototype.__registerCommands = function () {
            this.registerCommand("brandup-pages-collections", function () {
                page_collection_list_1.listPageCollection(null);
            });
        };
        return BrandUpPages;
    }(brandup_ui_1.UIElement));
    var current = null;
    var initialize = function () {
        if (!current && document.readyState === "complete") {
            current = new BrandUpPages();
        }
    };
    document.addEventListener("readystatechange", initialize);
    initialize();
    window.addEventListener("load", function () {
        current.load();
    });
});
//# sourceMappingURL=root.js.map