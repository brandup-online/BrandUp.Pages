export * from "./middleware"
export * from "./pages/content"

import { PagesMiddleware } from "./middleware";

export const pagesMiddleware = () => new PagesMiddleware();