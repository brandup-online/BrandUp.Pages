export interface IEditorConfig {
    language?: string;
    toolbar?: Array<string>;
}

export class EditorInstance {
    isReadOnly: boolean;
    readonly state: 'initializing' | 'ready' | 'destroyed';
    readonly model: Model;
    readonly data: DataController;

    on(event: string, callback: () => void, options?: { priority: PriorityString | number }) { }
    once(event: string, callback: () => void, options?: { priority: PriorityString | number }) { }
    off(event: string, callback: () => void) { }
    destroy(): Promise<any> {
        return null;
    }
}

export class DataController {
    readonly model: Model;

    on(event: string, callback: () => void, options?: { priority: PriorityString | number }) { }
    once(event: string, callback: () => void, options?: { priority: PriorityString | number }) { }
    off(event: string, callback: () => void) { }
    get(options?: { rootName?: string, trim?: "none" | "empty" }): string { return null; }
    set(data: string | { [key: string]: string }) { }
}

export class Model {
    readonly document: Document;

    on(event: string, callback: () => void, options?: { priority: PriorityString | number }) { }
    once(event: string, callback: () => void, options?: { priority: PriorityString | number }) { }
    off(event: string, callback: () => void) { }

    hasContent(rangeOrElement: Element, options?: { ignoreWhitespaces: boolean }): boolean { return false; }
}

export class Document {
    readonly model: Model;
    readonly differ: Differ;
    readonly graveyard: RootElement;

    on(event: string, callback: () => void, options?: { priority: PriorityString | number }) { }
    once(event: string, callback: () => void, options?: { priority: PriorityString | number }) { }
    off(event: string, callback: () => void) { }
    getRoot(name?: string): RootElement {
        return null;
    }
    getRootNames(): Array<string> { return []; }
    toJSON(): string { return null; }
}

export class Differ {
    readonly isEmpty: boolean;
    getChangedMarkers(): Array<any> { return []; }
    getChanges(options?: { includeChangesInGraveyard: boolean }): Array<any> { return []; }
    getMarkersToAdd(): Array<any> { return []; }
    getMarkersToRemove(): Array<any> { return []; }
    hasDataChanges(): boolean {
        return false;
    }
    reset() { }
}

export class Element {

}

export class RootElement extends Element {

}

export type PriorityString = 'highest' | 'high' | 'normal' | 'low' | 'lowest';

const createEditor = (elem: Element, config: IEditorConfig): Promise<EditorInstance> => {
    return null;
};

export default createEditor;