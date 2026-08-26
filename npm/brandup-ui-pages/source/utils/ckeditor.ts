// CKEditor 5 48.4 exposes the emitter API (on/once/off) through EmitterMixinConstructor —
// a conditional type that only resolves when strictNullChecks is on. This package compiles
// with strict: false, so ModelDocument, FocusTracker and other emitters come out without
// those members. Reading them through this minimal interface keeps the event handlers typed
// without switching the whole package to strict mode.
export interface CKEditorEmitter {
    on<TArgs extends unknown[] = unknown[]>(event: string, callback: (evt: unknown, ...args: TArgs) => void): void;
}

export const asEmitter = (source: object): CKEditorEmitter => source as unknown as CKEditorEmitter;
