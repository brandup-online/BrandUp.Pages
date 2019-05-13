import "./styles.less";
import { AppClientModel } from "./pages/typings/website";
import { Application } from "./pages/app";

export interface LandingWebSiteModel extends AppClientModel {
}

Application.setup<LandingWebSiteModel>({
    onCreatePage: (scriptName: string) => {
        throw "";
    }
}, (app) => {
});