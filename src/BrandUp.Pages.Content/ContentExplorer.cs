using BrandUp.Pages.Content.Fields;
using System;

namespace BrandUp.Pages.Content
{
    public class ContentExplorer
    {
        #region Fields

        public const char Delimiter = '.';
        public const char IndexStart = '[';
        public const char IndexEnd = ']';
        public static readonly char[] IndexTrimChars = new char[] { IndexStart, IndexEnd };
        private readonly IContentMetadataManager metadataManager;
        private readonly IContentViewManager viewManager;
        private readonly ContentExplorer rootExplorer;
        private readonly string name;

        #endregion

        #region Properties

        public IContentMetadataManager MetadataManager => metadataManager;
        public IContentViewManager ViewManager => viewManager;
        public ContentMetadataProvider Metadata { get; }
        public Field Field { get; }
        public object Content { get; }
        public string FieldPath { get; }
        public string Path { get; }
        public ContentExplorer Root => rootExplorer;
        public ContentExplorer Parent { get; }
        public int Index { get; } = -1;
        public bool IsRoot => Field == null;

        #endregion

        private ContentExplorer(IContentMetadataManager metadataManager, IContentViewManager viewManager, object content, ContentMetadataProvider contentMetadata)
        {
            this.metadataManager = metadataManager;
            this.viewManager = viewManager;
            Field = null;
            Content = content;
            Metadata = contentMetadata;
            Path = string.Empty;
            Parent = null;

            rootExplorer = null;
            name = contentMetadata.Name;
        }
        private ContentExplorer(ContentExplorer parent, Field field, int index, object content, ContentMetadataProvider contentMetadata)
        {
            metadataManager = parent.metadataManager;
            viewManager = parent.viewManager;
            Field = field;
            Content = content;
            Metadata = contentMetadata;
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));

            FieldPath = field.Name;
            if (index >= 0)
                FieldPath += "[" + index + "]";

            Path = !parent.IsRoot ? string.Concat(parent.Path, Delimiter, FieldPath) : FieldPath;
            Index = index;

            rootExplorer = parent.rootExplorer ?? parent;
            name = rootExplorer.name + ":" + Path;
        }

        public static ContentExplorer Create(IContentMetadataManager metadataManager, IContentViewManager viewManager, object content)
        {
            if (metadataManager == null)
                throw new ArgumentNullException(nameof(metadataManager));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var contentMetadata = metadataManager.GetMetadata(content.GetType());
            return new ContentExplorer(metadataManager, viewManager, content, contentMetadata);
        }
        public static ContentExplorer Create(IContentMetadataManager metadataManager, IContentViewManager viewManager, object content, string path)
        {
            var explorer = Create(metadataManager, viewManager, content);
            return explorer.Navigate(path);
        }

        public ContentExplorer Navigate(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            ContentExplorer navExplorer = this;

            while (path != string.Empty)
            {
                var fieldName = ExtractFirstFieldName(ref path);

                if (!VisitField(ref navExplorer, fieldName))
                    return null;
            }

            return navExplorer;
        }
        public ContentEditor Edit()
        {
            return new ContentEditor(this);
        }

        private static bool VisitField(ref ContentExplorer parentExplorer, string fieldName)
        {
            var charIndex = fieldName.IndexOf(IndexStart);
            var itemIndex = -1;
            if (charIndex > 0)
            {
                itemIndex = int.Parse(fieldName.Substring(charIndex).Trim(IndexTrimChars));
                fieldName = fieldName.Substring(0, charIndex);
            }

            var parentModel = parentExplorer.Content;

            if (!parentExplorer.Metadata.TryGetField(fieldName, out Field field))
                throw new InvalidOperationException(string.Format("Не найдено поле {0}.", fieldName));

            if (!(field is IFieldNavigationSupported fieldNavigation))
                throw new InvalidOperationException($"Поле {fieldName} не поддерживает навигацию по своему контенту.");

            var fieldValue = field.GetModelValue(parentModel);
            var contentModel = fieldNavigation.Navigate(fieldValue, itemIndex);

            if (contentModel == null)
            {
                parentExplorer = null;
                return false;
            }

            var contentMetadata = parentExplorer.metadataManager.GetMetadata(contentModel.GetType());

            parentExplorer = new ContentExplorer(parentExplorer, field, itemIndex, contentModel, contentMetadata);
            return true;
        }

        private static string ExtractFirstFieldName(ref string modelPath)
        {
            var firstDotIndex = modelPath.IndexOf(Delimiter);
            string fieldPath;

            if (firstDotIndex > -1)
            {
                fieldPath = modelPath.Substring(0, firstDotIndex);
                modelPath = modelPath.Substring(firstDotIndex + 1);
            }
            else
            {
                fieldPath = modelPath;
                modelPath = string.Empty;
            }

            return fieldPath;
        }

        public override string ToString()
        {
            return name;
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}