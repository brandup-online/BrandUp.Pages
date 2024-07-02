import { AjaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { ModelDesigner, ModelDesignerOptions } from "../designer/model";
import { FieldProvider } from "./base";
import { ContentFieldModel, IContentFieldDesigner } from "../../typings/content";
import { PageBlocksDesigner } from "../../content/designer/page-blocks";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { editPage } from "../../dialogs/pages/edit";
import { ModelFieldFormValue } from "../../content/field/model";
import { Content } from "../../content/content";

export class ModelFieldProvider extends FieldProvider<ModelFieldFormValue, ModelDesignerOptions> {
    protected __designerType;

    constructor(content: Content, model: ContentFieldModel<ModelFieldFormValue, ModelDesignerOptions>, valueElem: HTMLElement) {
        super(content, model, valueElem);
        let type = this.__valueElem.getAttribute("data-content-designer");
        this.__designerType = type === "page-blocks" ? PageBlocksDesigner : ModelDesigner;
    }

    createDesigner() {
        const designer = new this.__designerType(this.__valueElem, this.__model.options, this);
        return designer;
    }

    itemUp(index: number, elem: Element) {
        this.request({
            url: '/brandup.pages/content/model/up',
            urlParams: { itemIndex: index.toString() },
            method: "POST",
            success: () => elem.classList.remove("processing")
        });
    }

    itemDown(index: number, elem: Element) {
        this.request({
            url: '/brandup.pages/content/model/down',
            urlParams: { itemIndex: index.toString() },
            method: "POST",
            success: () => elem.classList.remove("processing")
        });
    }

    deleteItem(index: number, path: string) {
        this.request({
            url: '/brandup.pages/content/model',
            urlParams: { itemIndex: index.toString() },
            method: "DELETE",
            success: (() => {
                this.content.__editor.removeContentItem(path);
            })
        });
    }

    settingItem(contentPath: string) {
        editPage(this.content.__editor.editId, contentPath).then(() => {
            // this.__refreshItem(e.target, e.value.index);
        }).catch(() => {
            // this.__refreshItem(e.target, e.value.index);
        });
    }

    refreshItem(elem: Element, index?: number) {
        const urlParams = { itemIndex: index?.toString() };

        this.request({
            url: '/brandup.pages/content/model/view',
            urlParams: urlParams,
            method: "GET",
            success: (response: AjaxResponse<string>) => {
                (this.designer as ModelDesigner).refreshItem(elem, response.data);
                this.content.__editor.redraw();
            }
        });
    }

    addItem(itemType: string, index: number) {
        const fetchData = () => {
            this.request({
                url: '/brandup.pages/content/model',
                urlParams: {
                    itemType: itemType,
                    itemIndex: index.toString()
                },
                method: "PUT",
                success: (response: AjaxResponse) => {
                    if (response.status === 200) {
                        this.request({
                            url: '/brandup.pages/content/model/view',
                            urlParams: { itemIndex: index.toString() },
                            method: "GET",
                            success: (response: AjaxResponse<string>) => {
                                if (response.status === 200) {
                                    (this.designer as ModelDesigner).addItem(response.data, index);
                                    this.content.__editor.redraw();
                                }
                            }
                        });
                    }
                }
            });
        }
        if (!itemType) {
            selectContentType(this.__model.options.itemTypes).then((type) => {
                itemType = type.name;
                fetchData();  
            });
        }
        else fetchData();
    }
}