export interface IEditorConfig {
    language?: string;
    placeholder?: string;
    toolbar?: Array<string>;
}
export declare class EditorInstance {
    isReadOnly: boolean;
    readonly state: 'initializing' | 'ready' | 'destroyed';
    readonly model: Model;
    readonly data: DataController;
    on(event: string, callback: () => void, options?: {
        priority: PriorityString | number;
    }): void;
    once(event: string, callback: () => void, options?: {
        priority: PriorityString | number;
    }): void;
    off(event: string, callback: () => void): void;
    destroy(): Promise<any>;
}
export declare class DataController {
    readonly model: Model;
    on(event: string, callback: () => void, options?: {
        priority: PriorityString | number;
    }): void;
    once(event: string, callback: () => void, options?: {
        priority: PriorityString | number;
    }): void;
    off(event: string, callback: () => void): void;
    get(options?: {
        rootName?: string;
        trim?: "none" | "empty";
    }): string;
    set(data: string | {
        [key: string]: string;
    }): void;
}
export declare class Model {
    readonly document: Document;
    on(event: string, callback: () => void, options?: {
        priority: PriorityString | number;
    }): void;
    once(event: string, callback: () => void, options?: {
        priority: PriorityString | number;
    }): void;
    off(event: string, callback: () => void): void;
    hasContent(rangeOrElement: Element, options?: {
        ignoreWhitespaces: boolean;
    }): boolean;
}
export declare class Document {
    readonly model: Model;
    readonly differ: Differ;
    readonly graveyard: RootElement;
    on(event: string, callback: () => void, options?: {
        priority: PriorityString | number;
    }): void;
    once(event: string, callback: () => void, options?: {
        priority: PriorityString | number;
    }): void;
    off(event: string, callback: () => void): void;
    getRoot(name?: string): RootElement;
    getRootNames(): Array<string>;
    toJSON(): string;
}
export declare class Differ {
    readonly isEmpty: boolean;
    getChangedMarkers(): Array<any>;
    getChanges(options?: {
        includeChangesInGraveyard: boolean;
    }): Array<any>;
    getMarkersToAdd(): Array<any>;
    getMarkersToRemove(): Array<any>;
    hasDataChanges(): boolean;
    reset(): void;
}
export declare class Element {
}
export declare class RootElement extends Element {
}
export declare type PriorityString = 'highest' | 'high' | 'normal' | 'low' | 'lowest';
declare const createEditor: (elem: Element, config: IEditorConfig) => Promise<EditorInstance>;
export default createEditor;
