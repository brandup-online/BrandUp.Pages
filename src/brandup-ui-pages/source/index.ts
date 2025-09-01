export * from "./control"
export * from "./pages/content"
export * from "./dialogs/dialog"
import { PagesMiddleware } from "./middleware";

export const pagesMiddleware = () => new PagesMiddleware();