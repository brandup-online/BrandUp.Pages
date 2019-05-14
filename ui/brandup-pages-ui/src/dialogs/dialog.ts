import { UIControl, DOM } from "brandup-ui";
import "./dialog.less";

export abstract class Dialog<TResult> extends UIControl<DialogOptions> {
    protected headerElem: HTMLElement;
    protected headerTitleElem: HTMLElement;
    protected footerElem: HTMLElement;
    protected footerNotesElem: HTMLElement;
    protected contentElem: HTMLElement;
    private __errorElem: HTMLElement;
    private __parentDialog: Dialog<any> = null;
    private __childDialog: Dialog<any> = null;

    constructor(options?: DialogOptions) {
        super(options);
    }

    get content(): HTMLElement {
        return this.contentElem;
    }

    protected _getHtmlTemplate(): string {
        return '<div class="website-dialog-header">' +
            '    <span class="title"></span>' +
            '</div>' +
            '<div class="website-dialog-content"></div>' +
            '<div class="website-dialog-footer">' +
            '   <span class="notes"></span>' +
            '</div>';
    }
    protected _onRender() {
        this.element.classList.add("website-dialog");

        this.headerElem = DOM.getElementByClass(this.element, "website-dialog-header");
        this.headerTitleElem = DOM.getElementByClass(this.headerElem, "title");
        this.contentElem = DOM.getElementByClass(this.element, "website-dialog-content");
        this.footerElem = DOM.getElementByClass(this.element, "website-dialog-footer");
        this.footerNotesElem = DOM.getElementByClass(this.footerElem, "notes");

        if (this.options.header)
            this.setHeader(this.options.header);
        if (this.options.notes)
            this.setHeader(this.options.notes);
        if (this.options.disablePageScroll)
            document.body.classList.add("");

        if (this.__parentDialog) {
            this.headerElem.insertAdjacentElement("afterbegin", DOM.tag("a", { href: "", class: "button back", "data-command": "close" }));
        }
        else {
            this.headerElem.insertAdjacentElement("beforeend", DOM.tag("a", { href: "", class: "button x", "data-command": "close" }));
        }

        this.registerCommand("close", () => {
            this._onClose();
        });

        this._onRenderContent();
    }

    protected abstract _onRenderContent();
    protected _onClose() {
        this.destroy();
    }

    setHeader(html: string) {
        if (html) {
            this.headerTitleElem.innerHTML = html;
            this.element.classList.add("has-header");
        }
        else {
            this.headerTitleElem.innerText = "";
            this.element.classList.remove("has-header");
        }
    }
    setNotes(html: string) {
        if (html) {
            this.footerNotesElem.innerHTML = html;
            this.element.classList.add("has-notes");
        }
        else {
            this.footerNotesElem.innerHTML = "";
            this.element.classList.remove("has-notes");
        }
    }
    setLoading(value: boolean) {
        if (value)
            this.element.classList.add("loading");
        else
            this.element.classList.remove("loading");
    }
    addAction(name: string, title: string, isAccent: boolean = false) {
        var b = DOM.tag("button", { class: "button", "data-command": name }, title);
        if (isAccent)
            b.classList.add("accent");
        this.footerElem.appendChild(b);

        this.element.classList.add("has-actions");
    }

    setError(title: string, notes?: string) {
        this.element.classList.add("has-error");

        this.__errorElem = DOM.tag("div", { class: "website-dialog-error" }, title);

        this.content.insertAdjacentElement("beforebegin", this.__errorElem);
    }
    removeError() {
        this.element.classList.remove("has-error");

        if (this.__errorElem) {
            this.__errorElem.remove();
            this.__errorElem = null;
        }
    }

    private __resolve: (value: TResult) => void;
    private __reject: (reason: any) => void;

    open(): Promise<TResult> {
        if (currentDialog) {
            currentDialog.element.classList.add("hide");
            this.__parentDialog = currentDialog;
            currentDialog.__childDialog = this;
        }
        else
            document.body.classList.add("website-state-showdialog");

        currentDialog = this;

        this.render(dialogsPanelElem);
        this.element.classList.add("opened");

        return new Promise<TResult>((resolve, reject) => {
            this.__resolve = resolve;
            this.__reject = reject;
        });
    }

    protected resolve(value: TResult) {
        if (this.__resolve)
            this.__resolve(value);

        this.destroy();
    }
    protected reject(reason: any) {
        if (this.__reject)
            this.__reject(reason);
    }
    
    destroy() {
        if (this.__childDialog) {
            this.__childDialog.destroy();
            this.__childDialog = null;
        }

        if (!this.__parentDialog) {
            currentDialog = null;

            document.body.classList.remove("website-state-showdialog");
        }
        else {
            currentDialog = this.__parentDialog;
            currentDialog.element.classList.remove("hide");

            this.__parentDialog.__childDialog = null;
            this.__parentDialog = null;
        }

        super.destroy();
    }
}
export interface DialogOptions {
    header?: string;
    notes?: string;
    disablePageScroll?: boolean;
}

var dialogsPanelElem: HTMLElement = DOM.tag("div", { class: "brandup-pages-elem website-dialogs-panel" });
var currentDialog: Dialog<any> = null;
document.body.appendChild(dialogsPanelElem);