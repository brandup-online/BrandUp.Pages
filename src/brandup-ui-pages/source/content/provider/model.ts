import { AjaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { ModelDesigner, ModelDesignerOptions } from "../designer/model";
import { FieldProvider } from "./base";
import { ContentFieldModel, IContentFieldDesigner } from "../../typings/content";
import { PageBlocksDesigner } from "../../content/designer/page-blocks";
import { Content, Editor } from "../../content/editor";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { editPage } from "../../dialogs/pages/edit";
import { ModelFieldFormValue } from "../../content/field/model";

export class ModelFieldProvider extends FieldProvider<ModelFieldFormValue, ModelDesignerOptions> {
    protected __designerType;
    protected __items: Content[];
    set items(contents: Content[]) {
        this.__items = contents;
        this.__items.forEach((item, index) => {
            item.containerDataset.contentPathIndex = index.toString();
        })
    }

    constructor(editor: Editor, model: ContentFieldModel<ModelFieldFormValue, ModelDesignerOptions>, valueElem: HTMLElement, designerType: typeof ModelDesigner = ModelDesigner) {
        super(editor, model, valueElem);
        this.__designerType = designerType;
    }

    createDesigner() {
        const designer = new this.__designerType(this.__editor, this.__valueElem, this.__model.options);
        designer.setCallback("add-item", (e) => {
            const elem = e.target;
            const itemType = elem.getAttribute("data-item-type");
            let itemIndex = e.value;

            if (!itemType) {
                selectContentType(this.__model.options.itemTypes).then((type) => {
                    this.addItem(type.name, itemIndex);  
                });
            }
            else
                this.addItem(itemType, itemIndex);
        });

        designer.setCallback("item-settings", (e) => {
            editPage(this.__editor.editId, e.value.contentPath).then(() => {
                this.__refreshItem(e.target, e.value.index);
            }).catch(() => {
                this.__refreshItem(e.target, e.value.index);
            });
        })

        designer.setCallback("item-delete", (e) => {
            this.request({
                url: '/brandup.pages/content/model',
                urlParams: { itemIndex: e.value.index.toString() },
                method: "DELETE",
            });
        })

        designer.setCallback("item-refresh", (e) => {
            this.__refreshItem(e.target, e.value);
        })
        designer.setCallback("item-up", (e) => {
            this.request({
                url: '/brandup.pages/content/model/up',
                urlParams: { itemIndex: e.value },
                method: "POST",
                success: () => e.target.classList.remove("processing")
            });
        })
        designer.setCallback("item-down", (e) => {
            this.request({
                url: '/brandup.pages/content/model/down',
                urlParams: { itemIndex: e.value },
                method: "POST",
                success: () => e.target.classList.remove("processing")
            });
        })
        return designer;
    }

    protected __refreshItem(elem: Element, index?: number) {
        const urlParams = { itemIndex: index?.toString() };

        this.request({
            url: '/brandup.pages/content/model/view',
            urlParams: urlParams,
            method: "GET",
            success: (response: AjaxResponse<string>) => {
                (this.designer as PageBlocksDesigner).refreshItem(elem, response.data);
            }
        });
    }

    addItem(itemType: string, index: number) {
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
                                const container = (this.designer as PageBlocksDesigner).addItem(response.data, index);
                                const content = this.__editor.createContentFromHtml(container);
                                content.parent = this;
                                this.__items.splice(index, 0, content);
                                content.renderDesigners();
                            }
                        }
                    });
                }
            }
        });
    }
}