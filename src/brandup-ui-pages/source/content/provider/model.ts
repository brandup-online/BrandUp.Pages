import { FieldProvider } from "./base";
import { ModelDesigner } from "../designer/model";
import { AjaxResponse } from "brandup-ui-ajax";
import { PageBlocksDesigner } from "../../content/designer/page-blocks";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { editPage } from "../../dialogs/pages/edit";

export class ModelFieldProvider extends FieldProvider<ModelFieldValue, ModelFieldOptions> {
    createDesigner() {
        let type = this.valueElem.getAttribute("data-content-designer");
        const designerType = type === "page-blocks" ? PageBlocksDesigner : ModelDesigner;
        return new designerType(this);
    }

    createField() {
        throw "";
        //this.field = new ModelField(name, errors, options, this);
        //return this.field;
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
                (this.designer as ModelDesigner)?.deleteItem(index);
                //(this.field as ModelField)?.deleteItem(index);
                this.content.__editor.removeContentItem(path);
            })
        });
    }

    moveItem(itemIndex: number, newIndex: number) {
        this.request({
            url: '/brandup.pages/content/model/move',
            urlParams: { itemIndex: itemIndex.toString(), newIndex: newIndex.toString() },
            method: "POST",
            success: (response: AjaxResponse) => {
                if (response.status === 200) {
                    //this.setValue(response.data.value);
                }
            }
        });
    }

    settingItem(contentPath: string) {
        editPage(this.content.__editor, contentPath).then(() => {
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
                (this.designer as ModelDesigner)?.refreshItem(elem, response.data);
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
                                    (this.designer as ModelDesigner)?.addItem(response.data, index);
                                }
                            }
                        });
                    }
                }
            });
        }
        if (!itemType) {
            selectContentType(this.options.itemTypes).then((type) => {
                itemType = type.name;
                fetchData();  
            });
        }
        else fetchData();
    }
}

export interface ModelFieldValue {
    items: Array<ContentModel>;
}

export interface ContentModel {
    title: string;
    type: ContentTypeModel;
}

export interface ModelFieldOptions {
    addText: string;
    isListValue: boolean;
    itemType: ContentTypeModel;
    itemTypes: Array<ContentTypeModel>;
}

export interface ContentTypeModel {
    name: string;
    title: string;
}