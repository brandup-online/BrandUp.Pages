export interface PageNavState {
    url: string;
    title: string;
}

export interface AppSetupOptions {
    onCreatePage: (scriptName: string) => Promise<any>;
}

export interface AppClientModel {
    baseUrl: string;
    nav: NavigationModel;
}

export interface NavigationModel {
    isAuthenticated: boolean,
    url: string;
    validationToken: string;
    page: PageClientModel;
}

export interface PageClientModel {
    id: string;
    title: string;
    cssClass: string;
    scriptName: string;
}