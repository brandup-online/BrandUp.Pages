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
import { Page } from "brandup-ui-website";
var ContentPage = /** @class */ (function (_super) {
    __extends(ContentPage, _super);
    function ContentPage() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Object.defineProperty(ContentPage.prototype, "typeName", {
        get: function () { return "BrandUp.ContentPage"; },
        enumerable: false,
        configurable: true
    });
    return ContentPage;
}(Page));
export default ContentPage;
