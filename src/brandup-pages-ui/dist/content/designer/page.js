import { DOM, AjaxQueue } from "brandup-ui";
import { TextDesigner } from "./text";
import { HtmlDesigner } from "./html";
import { ModelDesigner } from "./model";
import { ImageDesigner } from "./image";
import { PageBlocksDesigner } from "./page-blocks";
import "./page.less";
var PageDesigner = /** @class */ (function () {
    function PageDesigner(page) {
        this.__fields = {};
        this.__accentedField = null;
        this.page = page;
        this.editId = page.model.editId;
        this.queue = new AjaxQueue();
        this.__rootElem = DOM.queryElement(document.body, "[content-root]");
        this.__rootElem.classList.add("page-designer");
        this.render();
        document.body.classList.add("bp-state-design");
    }
    PageDesigner.prototype.accentField = function (field) {
        if (this.__accentedField)
            throw "";
        this.__rootElem.classList.add("accented");
        for (var key in this.__fields) {
            var f = this.__fields[key];
            if (f === field)
                continue;
            f.element.classList.add("hide-ui");
        }
        this.__accentedField = field;
    };
    PageDesigner.prototype.clearAccent = function () {
        if (this.__accentedField) {
            this.__rootElem.classList.remove("accented");
            for (var key in this.__fields) {
                var f = this.__fields[key];
                f.element.classList.remove("hide-ui");
            }
            this.__accentedField = null;
        }
    };
    PageDesigner.prototype.render = function () {
        var fieldElements = DOM.queryElements(this.__rootElem, "[content-field]");
        for (var i = 0; i < fieldElements.length; i++) {
            var fieldElem = fieldElements.item(i);
            if (!fieldElem.hasAttribute("content-field-model") || !fieldElem.hasAttribute("content-designer") || fieldElem.classList.contains("field-designer"))
                continue;
            var designerName = fieldElem.getAttribute("content-designer");
            var fieldModel = JSON.parse(fieldElem.getAttribute("content-field-model"));
            var fieldDesigner = void 0;
            switch (designerName.toLowerCase()) {
                case "text": {
                    fieldDesigner = new TextDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "html": {
                    fieldDesigner = new HtmlDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "image": {
                    fieldDesigner = new ImageDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "model": {
                    fieldDesigner = new ModelDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "page-blocks": {
                    fieldDesigner = new PageBlocksDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                default:
                    continue;
            }
            this.__fields[fieldDesigner.fullPath] = fieldDesigner;
        }
        this.page.refreshScripts();
    };
    PageDesigner.prototype.destroy = function () {
        for (var key in this.__fields) {
            this.__fields[key].destroy();
        }
        this.__fields = null;
        document.body.classList.remove("bp-state-design");
    };
    return PageDesigner;
}());
export { PageDesigner };
