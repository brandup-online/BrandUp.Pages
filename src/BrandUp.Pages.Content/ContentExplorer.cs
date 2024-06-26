using BrandUp.Pages.Content.Fields;

namespace BrandUp.Pages.Content
{
    public class ContentExplorer
    {
        #region Fields

        public const char Delimiter = '.';
        public const char IndexStart = '[';
        public const char IndexEnd = ']';
        public static readonly char[] IndexTrimChars = new char[] { IndexStart, IndexEnd };
        readonly ContentExplorer rootExplorer;
        readonly string name;

        #endregion

        #region Properties

        public ContentMetadataProvider Metadata { get; }
        public IModelField Field { get; }
        public object Model { get; }
        public string ModelPath { get; }
        public string FieldPath { get; }
        public string Title => Metadata.GetContentTitle(Model) ?? Metadata.Title;
        public ContentExplorer Root => rootExplorer;
        public ContentExplorer Parent { get; }
        public int Index { get; } = -1;
        public bool IsRoot => Field == null;

        #endregion

        private ContentExplorer(object model, ContentMetadataProvider contentMetadata)
        {
            Field = null;
            Model = model;
            Metadata = contentMetadata;
            ModelPath = string.Empty;
            Parent = null;

            rootExplorer = null;
            name = contentMetadata.Name;
        }
        private ContentExplorer(ContentExplorer parent, IModelField field, int index, object model, ContentMetadataProvider contentMetadata)
        {
            Field = field;
            Model = model;
            Metadata = contentMetadata;
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));

            FieldPath = field.Name;
            if (index >= 0)
                FieldPath += "[" + index + "]";

            ModelPath = !parent.IsRoot ? string.Concat(parent.ModelPath, Delimiter, FieldPath) : FieldPath;
            Index = index;

            rootExplorer = parent.rootExplorer ?? parent;
            name = rootExplorer.name + ":" + ModelPath;
        }

        public static ContentExplorer Create(ContentMetadataManager metadataManager, object model)
        {
            ArgumentNullException.ThrowIfNull(metadataManager);
            ArgumentNullException.ThrowIfNull(model);

            var contentMetadata = metadataManager.GetMetadata(model.GetType());
            return new ContentExplorer(model, contentMetadata);
        }
        public static ContentExplorer Create(ContentMetadataManager metadataManager, object model, string modelPath)
        {
            var explorer = Create(metadataManager, model);
            return explorer.Navigate(modelPath);
        }

        public ContentExplorer Navigate(string modelPath)
        {
            ArgumentNullException.ThrowIfNull(modelPath);

            ContentExplorer navExplorer = this;

            while (modelPath != string.Empty)
            {
                var fieldName = ExtractFirstFieldName(ref modelPath);

                if (!VisitField(ref navExplorer, fieldName))
                    return null;
            }

            return navExplorer;
        }

        #region Helper methods

        static bool VisitField(ref ContentExplorer parentExplorer, string fieldName)
        {
            var charIndex = fieldName.IndexOf(IndexStart);
            var itemIndex = -1;
            if (charIndex > 0)
            {
                itemIndex = int.Parse(fieldName[charIndex..].Trim(IndexTrimChars));
                fieldName = fieldName[..charIndex];
            }

            var parentModel = parentExplorer.Model;

            if (!parentExplorer.Metadata.TryGetField(fieldName, out FieldProviderAttribute field))
                throw new InvalidOperationException(string.Format("Не найдено поле {0}.", fieldName));

            if (field is not IModelField modelField)
                throw new InvalidOperationException($"Поле {fieldName} не является полем модели.");

            var fieldValue = field.GetModelValue(parentModel);
            var contentModel = modelField.Navigate(fieldValue, itemIndex);

            if (contentModel == null)
            {
                parentExplorer = null;
                return false;
            }

            var contentMetadata = parentExplorer.Metadata.Manager.GetMetadata(contentModel.GetType());

            parentExplorer = new ContentExplorer(parentExplorer, modelField, itemIndex, contentModel, contentMetadata);
            return true;
        }
        static string ExtractFirstFieldName(ref string modelPath)
        {
            var firstDotIndex = modelPath.IndexOf(Delimiter);
            string fieldPath;

            if (firstDotIndex > -1)
            {
                fieldPath = modelPath[..firstDotIndex];
                modelPath = modelPath[(firstDotIndex + 1)..];
            }
            else
            {
                fieldPath = modelPath;
                modelPath = string.Empty;
            }

            return fieldPath;
        }

        #endregion

        #region Object members

        public override string ToString()
        {
            return name;
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        #endregion
    }
}