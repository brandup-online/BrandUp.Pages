var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var Manager = /** @class */ (function () {
        function Manager() {
        }
        return Manager;
    }());
    exports.Manager = Manager;
    var PageManager = /** @class */ (function (_super) {
        __extends(PageManager, _super);
        function PageManager() {
            return _super.call(this) || this;
        }
        return PageManager;
    }(Manager));
    exports.PageManager = PageManager;
});
//# sourceMappingURL=manager.js.map