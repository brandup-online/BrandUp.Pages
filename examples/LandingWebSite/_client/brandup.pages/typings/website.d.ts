import { AjaxRequestOptions } from "brandup-ui";

export interface PageNavState {
    url: string;
    title: string;
}

export interface AppClientModel {
    baseUrl: string;
    nav: NavigationModel;
    antiforgery: AntiforgeryModel;
}

export interface AntiforgeryModel {
    headerName: string;
    formFieldName: string;
}

export interface NavigationModel {
    enableAdministration: boolean;
    isAuthenticated: boolean,
    url: string;
    validationToken: string;
    page: PageClientModel;
}

export interface PageClientModel {
    title: string;
    cssClass: string;
    scriptName: string;
}

export interface IApplication {
    model: any;
    navigation: NavigationModel;
    request(options: AjaxRequestOptions)
    uri(path?: string, queryParams?: { [key: string]: string; }): string;
    reload();
    navigate(target: any);
    nav(options: NavigationOptions);
    script(name: string): Promise<{ default: any }>;
}

export interface NavigationOptions {
    url: string;
    pushState: boolean;
}

export interface IPage {
    app: IApplication;

    refreshScripts();
}