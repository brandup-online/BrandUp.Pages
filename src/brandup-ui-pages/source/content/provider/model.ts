import { FieldProvider } from "./base";
import { ModelDesigner } from "../designer/model";
import { AjaxResponse } from "brandup-ui-ajax";
import { PageBlocksDesigner } from "../../content/designer/page-blocks";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { editPage } from "../../dialogs/pages/edit";
import { FieldValueResult } from "../../typings/models";
import { IModelFieldProvider, IParentContent } from "../../typings/content";
import { Content } from "../../content/content";

export class ModelFieldProvider extends FieldProvider<ModelFieldValue, ModelFieldOptions> implements IParentContent, IModelFieldProvider {
    private __contentItems: Content[] = [];
    private __insertIndex: number = 0;
    readonly isModelField = true;

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
        this._refreshIndexses();
        this.request({
            url: '/brandup.pages/content/model/up',
            urlParams: { itemIndex: index.toString() },
            method: "POST",
            success: () => elem.classList.remove("processing")
        });
    }

    itemDown(index: number, elem: Element) {
        this._refreshIndexses();
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
                this.__contentItems = this.__contentItems.filter((content, contentIndex) => {console.log(contentIndex, index); return contentIndex !== index})
                this.content.editor.removeContentItem(path);
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
        editPage(this.content.editor, contentPath).then(() => {
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

    insertContent(item: Content) {
        this.__contentItems.splice(this.__insertIndex, 0, item);
        this.__insertIndex = this.__contentItems.length;
        this._refreshIndexses();
    }

    protected _refreshIndexses(start: number = 0) {
        for (let i = start; i < this.__contentItems.length; i++) {
            this.__contentItems[i].container?.setAttribute("data-content-path-index", i.toString());
        }
    }

    protected getItemIndex(container: HTMLElement) {
        let index = -1;
        for (let i = 0; i < this.__contentItems.length; i++) {
            if (this.__contentItems[i].container === container) {
                index = i;
                break;
            }
        }
        return index;
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
                success: (response: AjaxResponse<FieldValueResult>) => {
                    if (response.status === 200) {
                        this.onSavedValue(response.data);

                        const value = this.getValue();
                        value.items.forEach((model, index) => {

                        });

                        this.request({
                            url: '/brandup.pages/content/model/view',
                            urlParams: { itemIndex: index.toString() },
                            method: "GET",
                            success: (response: AjaxResponse<string>) => {
                                if (response.status === 200) {
                                    const newElem = (this.designer as ModelDesigner)?.addItem(response.data, index);
                                    const modelPath = newElem.dataset.contentPath;
                                    this.content.editor.createContent(modelPath, newElem, () => {
                                        (this.designer as ModelDesigner).renderBlock(newElem);
                                        this._refreshIndexses();
                                    });
                                }
                            }
                        });
                    }
                }
            });
        }

        this.__insertIndex = index;

        if (!itemType) {
            selectContentType(this.options.itemTypes).then((type) => {
                itemType = type.name;
                fetchData();  
            });
        }
        else
            fetchData();
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