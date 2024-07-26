const fieldProviders: { [type: string]: () => Promise<any> } = {};
const fieldDesigners: { [type: string]: () => Promise<any> } = {};
const formFields: { [type: string]: () => Promise<any> } = {};

export default {
    registerProvider: (type: string, resolve: () => Promise<any>) => {
        fieldProviders[type] = resolve;
    },
    resolveProvider: (type: string) => {
        var provderType = fieldProviders[type];
        if (!provderType)
            throw new Error(`Not found field provider by type "${type}"`);
        return provderType();
    },
    registerDesigner: (type: string, resolve: () => Promise<any>) => {
        fieldDesigners[type] = resolve;
    },
    resolveDesigner: (type: string) => {
        var designerType = fieldDesigners[type];
        if (!designerType)
            throw new Error(`Not found field designer by type "${type}"`);
        return designerType();
    },

    registerFormField: (type: string, resolve: () => Promise<any>) => {
        formFields[type] = resolve;
    },
    resolveFormField: (type: string) => {
        var fieldType = formFields[type];
        if (!fieldType)
            throw new Error(`Not found form field by type "${type}"`);
        return fieldType();
    }
};