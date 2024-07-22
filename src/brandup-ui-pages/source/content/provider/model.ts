import { FieldProvider } from "./base";
import { ModelDesigner } from "../designer/model";
import { AjaxResponse } from "@brandup/ui-ajax";
import { PageBlocksDesigner } from "../../content/designer/page-blocks";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { editPage } from "../../dialogs/content/edit";
import { DOM } from "@brandup/ui-dom";
import { Content, IContentHost } from "../content";
import { ContentEditor } from "../editor";
import { ContentModel, FieldValueResult } from "../../typings/content";

export class ModelFieldProvider extends FieldProvider<ModelFieldValue, ModelFieldOptions> implements IContentHost {
    private __contents: Content[] = [];

    // IContentHost members

    get editor(): ContentEditor { return this.content.host.editor; }

    get isList(): boolean { return this.options.isListValue; }

    attach(content: Content) {
        if (this.options.isListValue)
            this.__contents.splice(content.index, 0, content);
        else {
            if (this.__contents.length !== 0)
                throw "Model field already exist content.";
            this.__contents[0] = content;
        }
    }

    // ModelFieldProvider members
    
    addItem(index: number) {
        if (!this.options.isListValue && this.hasValue())
            throw "Content already exists.";

        return selectContentType(this.options.itemTypes)
            .then(type => new Promise<Content>((resolve) => {
                this.request({
                    url: '/brandup.pages/content/model',
                    query: { itemType: type.name, itemIndex: index.toString() },
                    method: "PUT",
                    success: (response: AjaxResponse<AddContentResult>) => {
                        if (response.status === 200 && response.data) {
                            this.onSavedValue(response.data.fieldValue);

                            const newContent = this.editor.initContent(response.data.content);
                            resolve(newContent);
                        }
                        else
                            throw "Error add content.";
                    }
                });
            }));

            //.then(value => new Promise<Content>(resolve => {
            //    if (this.valueElem) {
            //        this.request({
            //            url: '/brandup.pages/content/model/view',
            //            urlParams: { itemIndex: index.toString() },
            //            method: "GET",
            //            success: (response: AjaxResponse<string>) => {
            //                if (response.status === 200) {
            //                    const newElem = this.__addItemDOM(response.data, index);
            //                }
            //                else
            //                    throw "Error load content view.";
            //            }
            //        });
            //    }
            //}));
    }

    private __addItemDOM(html: string, index: number) {
        const fragment = document.createDocumentFragment();
        const container = DOM.tag("div", null, html);
        fragment.appendChild(container);
        const newElem = DOM.queryElement(container, "[data-content-path]");
        if (!newElem) return;

        let elem;

        while (index >= 0 && !elem) {
            elem = this.__contents[index].container;
            index -= 1;
        }
        if (index < 0 && !elem) {
            this.designer?.element?.insertAdjacentElement("afterbegin", newElem);
        } else {
            elem?.insertAdjacentElement("beforebegin", newElem);
        }

        return newElem;
    }

    createDesigner() {
        let type = this.valueElem?.getAttribute("data-content-designer");
        if (!type) throw "failed to identify the element type for ModelDesigner";

        const designerType = type === "page-blocks" ? PageBlocksDesigner : ModelDesigner;
        return new designerType(this);
    }

    saveValue(value: any) {
        throw "method saveValue not implemented in ModelFieldProvider"
    }

    itemUp(index: number, elem: Element) {
        [this.__contents[index], this.__contents[index - 1]] = [this.__contents[index - 1], this.__contents[index]]
        this._refreshIndexses();
        this.request({
            url: '/brandup.pages/content/model/up',
            query: { itemIndex: index.toString() },
            method: "POST",
            success: () => elem.classList.remove("processing")
        });
    }

    itemDown(index: number, elem: Element) {
        [this.__contents[index], this.__contents[index + 1]] = [this.__contents[index + 1], this.__contents[index]]
        this._refreshIndexses();
        this.request({
            url: '/brandup.pages/content/model/down',
            query: { itemIndex: index.toString() },
            method: "POST",
            success: () => elem.classList.remove("processing")
        });
    }

    deleteItem(index: number, path: string) {
        this.request({
            url: '/brandup.pages/content/model',
            query: { itemIndex: index.toString() },
            method: "DELETE",
            success: (() => {
                //this.__contents[index].container?.remove();
                //this.__contents = this.__contents.filter((content, contentIndex) => {console.log(contentIndex, index); return contentIndex !== index})
                //this.content.host.editor.removeContentItem(path);
                //this._refreshIndexses();
            })
        });
    }

    moveItem(itemIndex: number, newIndex: number) {
        this.request({
            url: '/brandup.pages/content/model/move',
            query: { itemIndex: itemIndex.toString(), newIndex: newIndex.toString() },
            method: "POST",
            success: (response: AjaxResponse) => {
                if (response.status === 200) {
                    //this.setValue(response.data.value);
                }
            }
        });
    }
    
    settingItem(contentPath: string) {
        const content = this.__contents.find(content => {
            return content.path === contentPath
        });
        if (!content) return `content with path ${contentPath} not found`;
        editPage(content, contentPath).then(() => {
            // this.__refreshItem(e.target, e.value.index);
        }).catch(() => {
            // this.__refreshItem(e.target, e.value.index);
        });
    }

    refreshItem(elem: Element, index?: number) {
        const urlParams = { itemIndex: index?.toString() || "-1"};

        this.request({
            url: '/brandup.pages/content/model/view',
            query: urlParams,
            method: "GET",
            success: (response: AjaxResponse<string>) => {
                (this.designer as ModelDesigner)?.refreshItem(elem, response.data);
            }
        });
    }
    
    protected _refreshIndexses(start: number = 0) {
        for (let i = start; i < this.__contents.length; i++) {
            this.__contents[i].container?.setAttribute("data-content-path-index", i.toString());
        }
    }

    getItem(index: number) {
        const item = this.__contents[index];
        if (!item) throw `content by index ${index} not found`;
        return item;
    }

    protected getItemIndex(container: HTMLElement) {
        let index = -1;
        for (let i = 0; i < this.__contents.length; i++) {
            if (this.__contents[i].container === container) {
                index = i;
                break;
            }
        }
        return index;
    }
}

export interface ModelFieldValue {
    items: Array<ContentInfoModel>;
}

export interface ContentInfoModel {
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

interface ContentModelResult {
    fieldValue: FieldValueResult;
}

interface AddContentResult extends ContentModelResult {
    content: Array<ContentModel>;
}