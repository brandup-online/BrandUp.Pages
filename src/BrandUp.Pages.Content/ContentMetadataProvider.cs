using BrandUp.Pages.Content.Fields;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataProvider
    {
        #region Fields

        private static readonly object[] ModelConstructorParameters = new object[0];
        private readonly ConstructorInfo _modelConstructor = null;
        private readonly List<ContentMetadataProvider> _derivedContents = new List<ContentMetadataProvider>();
        private readonly List<Field> _fields = new List<Field>();
        private readonly Dictionary<string, int> _fieldNames = new Dictionary<string, int>();
        private ViewField viewField = null;

        #endregion

        internal ContentMetadataProvider(ContentMetadataManager metadataManager, Type modelType, ContentMetadataProvider baseMetadata)
        {
            Manager = metadataManager;
            ModelType = modelType;
            BaseMetadata = baseMetadata;

            var contentModelAttribute = modelType.GetCustomAttribute<ContentModelAttribute>(false);
            if (contentModelAttribute == null)
                throw new ArgumentException($"Для типа модели контента \"{modelType}\" не определён атрибут {nameof(ContentModelAttribute)}.");

            if (!modelType.IsAbstract)
            {
                _modelConstructor = modelType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
                if (_modelConstructor == null)
                    throw new InvalidOperationException($"Тип модели контента \"{modelType}\" не содержит публичный конструктор без параметров.");
            }

            if (baseMetadata != null)
                baseMetadata._derivedContents.Add(this);

            Name = contentModelAttribute.Name ?? GetTypeName(modelType);
            Title = contentModelAttribute.Title ?? Name;
            Description = contentModelAttribute.Description;
        }

        #region Properties

        public ContentMetadataManager Manager { get; }
        public Type ModelType { get; }
        public string Name { get; }
        public string Title { get; }
        public string Description { get; }
        public ContentMetadataProvider BaseMetadata { get; }
        public IEnumerable<ContentMetadataProvider> DerivedContents => _derivedContents;
        public bool SupportViews => viewField != null;
        public IEnumerable<Field> Fields => _fields;
        public ViewField ViewField => viewField;

        #endregion

        #region Methods

        private static string GetTypeName(Type type)
        {
            return type.Name.Substring(0, type.Name.LastIndexOf(ContentMetadataManager.ContentTypePrefix));
        }

        internal void InitializeFields()
        {
            var baseModelMetadata = BaseMetadata;
            if (baseModelMetadata != null)
            {
                foreach (var field in baseModelMetadata.Fields)
                    AddField(field);
            }

            var metadataManager = Manager;

            foreach (var fieldInfo in ModelType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var attr = fieldInfo.GetCustomAttribute<FieldAttribute>(false);
                if (attr == null)
                    continue;

                var field = attr.CreateField();
                if (field == null)
                    throw new InvalidOperationException();

                InitializeField(metadataManager, field, fieldInfo, attr);
            }

            foreach (var propertyInfo in ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                var attr = propertyInfo.GetCustomAttribute<FieldAttribute>(false);
                if (attr == null)
                    continue;

                var field = attr.CreateField();
                if (field == null)
                    throw new InvalidOperationException();

                InitializeField(metadataManager, field, propertyInfo, attr);
            }
        }
        private void InitializeField(ContentMetadataManager metadataManager, Field field, MemberInfo typeMember, FieldAttribute attr)
        {
            field.Initialize(metadataManager, typeMember, attr);

            AddField(field);
        }
        private void AddField(Field field)
        {
            var fIndex = _fields.Count;

            _fieldNames.Add(field.Name.ToLower(), fIndex);
            _fields.Add(field);

            if (field is ViewField)
            {
                if (viewField != null)
                    throw new InvalidOperationException();

                viewField = (ViewField)field;
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public bool TryGetField(string fieldName, out Field field)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            if (!_fieldNames.TryGetValue(fieldName.ToLower(), out int index))
            {
                field = null;
                return false;
            }
            field = _fields[index];
            return true;
        }
        public string GetViewName(object model)
        {
            return (string)viewField.GetModelValue(model);
        }
        public void SetViewName(object model, string viewName)
        {
            viewField.SetModelValue(model, viewName);
        }
        public object CreateModelInstance()
        {
            return _modelConstructor.Invoke(new object[0]);
        }

        #endregion

        #region Object members

        public override string ToString()
        {
            return Name;
        }
        public override int GetHashCode()
        {
            return ModelType.GetHashCode();
        }

        #endregion
    }
}