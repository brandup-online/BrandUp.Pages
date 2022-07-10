var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
import { Middleware } from "brandup-ui-app";
//import { Page, PageModel } from "brandup-ui-website";
//import ContentPage from "./pages/content";
import "./styles.less";
var PagesMiddleware = /** @class */ (function (_super) {
    __extends(PagesMiddleware, _super);
    function PagesMiddleware() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    PagesMiddleware.prototype.start = function (context, next) {
        next();
        //this._showUI(context.items);
    };
    PagesMiddleware.prototype.navigating = function (context, next) {
        next();
    };
    PagesMiddleware.prototype.navigate = function (context, next) {
        next();
        //this._showUI(context.items);
    };
    return PagesMiddleware;
}(Middleware));
export { PagesMiddleware };
