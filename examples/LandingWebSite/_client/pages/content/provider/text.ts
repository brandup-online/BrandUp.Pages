export class TextProvider implements IContentProvider {
    getValue(): Promise<any> {
        throw new Error("Method not implemented.");
    }

}

export interface IContentProvider {
    getValue(): Promise<any>;
}