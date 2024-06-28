using BrandUp.Pages.Content;

namespace BrandUp.Pages.Features
{
    internal class ContentEditFeature : IContentEditContext
    {
        public IContentEdit Edit { get; }
        public object Content { get; }

        public ContentEditFeature(IContentEdit contentEdit, object content)
        {
            Edit = contentEdit;
            Content = content;
        }

        public bool IsCurrentEdit(IContent content)
        {
            ArgumentNullException.ThrowIfNull(content);

            return content.Id == Edit.ContentId;
        }
    }

    public interface IContentEditContext
    {
        IContentEdit Edit { get; }
        object Content { get; }
    }
}