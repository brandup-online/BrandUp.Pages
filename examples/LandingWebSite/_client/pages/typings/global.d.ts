interface Window {
    app: IApplication;
}

interface IApplication {
    model: any;
    uri(path?: string, queryParams?: { [key: string]: string; }): string;
    reload();
    navigate(target: any);
    nav(options: NavigationOptions);
    reRenderPage(content: string);
}

interface NavigationOptions {
    url: string;
    pushState: boolean;
}