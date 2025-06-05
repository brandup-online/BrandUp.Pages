import { PagesMiddleware } from "./middleware"
export * from "./pages/content"

export const pagesMiddlewareFactory = () => new PagesMiddleware();