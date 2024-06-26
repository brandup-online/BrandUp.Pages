using BrandUp.Pages.Content;

namespace BrandUp.Pages.Features
{
    public class ContentEditFeature
    {
        public IContentEdit Edit { get; }
        public object Content { get; }

        internal ContentEditFeature(IContentEdit contentEdit, object content)
        {
            Edit = contentEdit;
            Content = content;
        }

        public bool IsEdit(string contentKey)
        {
            ArgumentNullException.ThrowIfNull(contentKey);

            return contentKey.Equals(Edit.ContentKey, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}