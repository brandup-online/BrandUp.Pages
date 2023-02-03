using LandingWebSite.Blog.Documents;
using MongoDB.Driver;

namespace LandingWebSite.Blog
{
    public interface IBlogContext
    {
        public IMongoCollection<BlogPostDocument> BlogPosts { get; }
    }
}