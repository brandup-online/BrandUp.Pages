using BrandUp.Pages.Content.Fields;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataManager : IContentMetadataManager
    {
        private const string ContentTypePrefix = "Content";
        public const string ContentTypeNameDataKey = "_type";
        private readonly List<ContentMetadataProvider> metadatas = new List<ContentMetadataProvider>();
        private readonly Dictionary<Type, int> metadataTypes = new Dictionary<Type, int>();
        private readonly Dictionary<string, int> metadataNames = new Dictionary<string, int>();

        public ContentMetadataManager(IContentTypeResolver typeResolver)
        {
            if (typeResolver == null)
                throw new ArgumentNullException(nameof(typeResolver));

            foreach (var contentModelType in typeResolver.GetContentTypes())
                TryRegisterType(contentModelType, out ContentMetadataProvider typeMetadata);

            foreach (var contentMetadata in metadatas)
                contentMetadata.Initialize(this);
        }

        private bool TryRegisterType(Type contentType, out ContentMetadataProvider contentMetadata)
        {
            if (TryGetMetadata(contentType, out contentMetadata))
                return true;

            if (!IsContent(contentType.GetTypeInfo()))
                return false;

            var contentModelAttribute = contentType.GetCustomAttribute<ContentModelAttribute>(false);
            if (contentModelAttribute == null)
                return false;

            ContentMetadataProvider baseTypeMetadata = null;
            if (contentType.BaseType != null)
                TryRegisterType(contentType.BaseType, out baseTypeMetadata);

            contentMetadata = new ContentMetadataProvider(contentType, contentModelAttribute, baseTypeMetadata);

            var index = metadatas.Count;
            metadatas.Add(contentMetadata);
            metadataTypes.Add(contentType, index);
            metadataNames.Add(contentMetadata.Name.ToLower(), index);

            return true;
        }
        private bool TryGetMetadata(Type contentType, out ContentMetadataProvider contentMetadata)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            if (!metadataTypes.TryGetValue(contentType, out int index))
            {
                contentMetadata = null;
                return false;
            }

            contentMetadata = metadatas[index];
            return true;
        }
        public static bool IsContent(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
                return false;
            if (!typeInfo.IsPublic)
                return false;
            if (typeInfo.ContainsGenericParameters)
                return false;
            if (!typeInfo.Name.EndsWith(ContentTypePrefix, StringComparison.OrdinalIgnoreCase) || !typeInfo.IsDefined(typeof(ContentModelAttribute), false))
                return false;
            return true;
        }

        #region IContentMetadataManager members

        public bool IsRegisterdContentType(Type contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            return metadataTypes.ContainsKey(contentType);
        }
        public IContentMetadataProvider GetMetadata(Type contentType)
        {
            if (!TryGetMetadata(contentType, out IContentMetadataProvider contentMetadata))
                throw new ArgumentException($"Тип \"{contentType.AssemblyQualifiedName}\" не является контентом.");
            return contentMetadata;
        }
        public bool TryGetMetadata(Type contentType, out IContentMetadataProvider metadata)
        {
            if (!TryGetMetadata(contentType, out ContentMetadataProvider contentMetadata))
            {
                metadata = null;
                return false;
            }

            metadata = contentMetadata;
            return true;
        }
        public bool TryGetMetadata(string contentTypeName, out IContentMetadataProvider metadata)
        {
            if (contentTypeName == null)
                throw new ArgumentNullException(nameof(contentTypeName));

            if (!metadataNames.TryGetValue(contentTypeName.ToLower(), out int index))
            {
                metadata = null;
                return false;
            }

            metadata = metadatas[index];
            return true;
        }
        public IEnumerable<IContentMetadataProvider> GetAllMetadata()
        {
            return metadatas;
        }
        public string GetContentViewName(object contentModel)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));
            if (!TryGetMetadata(contentModel.GetType(), out ContentMetadataProvider contentMetadata))
                throw new ArgumentException();

            if (!contentMetadata.SupportViews)
                throw new InvalidOperationException("Тип контента не поддерживает представления.");

            return (string)contentMetadata.ViewField.GetModelValue(contentModel);
        }
        public IDictionary<string, object> ConvertContentModelToDictionary(object contentModel)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));
            if (!TryGetMetadata(contentModel.GetType(), out IContentMetadataProvider contentMetadata))
                throw new ArgumentException();

            var result = new SortedDictionary<string, object>
            {
                { ContentTypeNameDataKey, contentMetadata.Name }
            };

            foreach (var field in contentMetadata.Fields)
            {
                var fieldValue = field.GetModelValue(contentModel);
                if (!field.HasValue(fieldValue))
                    continue;

                var dataValue = field.ConvetValueToData(fieldValue);
                result.Add(field.JsonPropertyName, dataValue);
            }

            return result;
        }
        public object ConvertDictionaryToContentModel(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (!dictionary.TryGetValue(ContentTypeNameDataKey, out object contentTypeNameValue))
                throw new ArgumentException("Справочник данных не содержит названия типа контента.");
            dictionary.Remove(ContentTypeNameDataKey);

            if (!TryGetMetadata((string)contentTypeNameValue, out IContentMetadataProvider contentMetadata))
                throw new ArgumentException();

            var contentModel = contentMetadata.CreateModelInstance();

            foreach (var kv in dictionary)
            {
                if (!contentMetadata.TryGetField(kv.Key, out Field field))
                    continue;

                var dataValue = kv.Value;
                var value = field.ConvetValueFromData(dataValue);
                field.SetModelValue(contentModel, value);
            }

            return contentModel;
        }

        #endregion

        private class ContentMetadataProvider : IContentMetadataProvider
        {
            private static readonly object[] ModelConstructorParameters = new object[0];
            private readonly ConstructorInfo _modelConstructor = null;
            private readonly List<ContentMetadataProvider> _derivedContents = new List<ContentMetadataProvider>();
            private readonly List<Field> _fields = new List<Field>();
            private readonly Dictionary<string, int> _fieldNames = new Dictionary<string, int>();
            private ViewField viewField = null;

            public ViewField ViewField => viewField;

            public ContentMetadataProvider(Type type, ContentModelAttribute contentModelAttribute, ContentMetadataProvider baseTypeMetadata)
            {
                ModelType = type;
                ParentMetadata = baseTypeMetadata;

                if (!type.IsAbstract)
                {
                    _modelConstructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
                    if (_modelConstructor == null)
                        throw new InvalidOperationException(string.Format("Для типа {0} не найден подходящий конструктор", type.AssemblyQualifiedName));
                }

                if (baseTypeMetadata != null)
                    baseTypeMetadata._derivedContents.Add(this);

                Name = contentModelAttribute.Name ?? GetTypeName(type);
                Title = contentModelAttribute.Title ?? Name;
                Description = contentModelAttribute.Description;
            }

            private string GetTypeName(Type type)
            {
                return type.Name.Substring(0, type.Name.LastIndexOf(ContentTypePrefix));
            }
            public void Initialize(ContentMetadataManager metadataProvider)
            {
                var baseModelMetadata = ParentMetadata;
                if (baseModelMetadata != null)
                {
                    foreach (var field in baseModelMetadata.Fields)
                        AddField(field);
                }

                foreach (var fieldInfo in ModelType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    var attr = fieldInfo.GetCustomAttribute<FieldAttribute>(false);
                    if (attr == null)
                        continue;

                    var field = attr.CreateField();
                    if (field == null)
                        throw new InvalidOperationException();

                    InitializeField(metadataProvider, field, fieldInfo, attr);
                }

                foreach (var propertyInfo in ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty))
                {
                    var attr = propertyInfo.GetCustomAttribute<FieldAttribute>(false);
                    if (attr == null)
                        continue;

                    var field = attr.CreateField();
                    if (field == null)
                        throw new InvalidOperationException();

                    InitializeField(metadataProvider, field, propertyInfo, attr);
                }
            }

            private void InitializeField(ContentMetadataManager metadataProvider, Field field, MemberInfo typeMember, FieldAttribute attr)
            {
                field.Initialize(metadataProvider, typeMember, attr);

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

            #region IContentMetadata members

            public Type ModelType { get; }
            public string Name { get; }
            public string Title { get; }
            public string Description { get; }
            public IContentMetadataProvider ParentMetadata { get; }
            public IEnumerable<IContentMetadataProvider> DerivedContents => _derivedContents;
            public bool SupportViews => viewField != null;
            public IEnumerable<Field> Fields => _fields;

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

            #endregion

            public object CreateModelInstance()
            {
                return _modelConstructor.Invoke(new object[0]);
            }
        }
    }

    public interface IContentMetadataManager
    {
        bool IsRegisterdContentType(Type contentType);
        IContentMetadataProvider GetMetadata(Type contentType);
        bool TryGetMetadata(Type contentType, out IContentMetadataProvider metadata);
        bool TryGetMetadata(string contentTypeName, out IContentMetadataProvider metadata);
        IEnumerable<IContentMetadataProvider> GetAllMetadata();
        string GetContentViewName(object contentModel);
        IDictionary<string, object> ConvertContentModelToDictionary(object contentModel);
        object ConvertDictionaryToContentModel(IDictionary<string, object> dictionary);
    }

    public interface IContentMetadataProvider
    {
        Type ModelType { get; }
        string Name { get; }
        string Title { get; }
        string Description { get; }
        IContentMetadataProvider ParentMetadata { get; }
        IEnumerable<IContentMetadataProvider> DerivedContents { get; }
        bool SupportViews { get; }
        IEnumerable<Field> Fields { get; }
        bool TryGetField(string fieldName, out Field field);
        string GetViewName(object model);
        void SetViewName(object model, string viewName);
        object CreateModelInstance();
    }

    public interface IContentTypeResolver
    {
        IList<TypeInfo> GetContentTypes();
    }

    public interface IContentDefaultModelResolver
    {
        IDictionary<string, object> GetDefaultModel(string contentTypeName, Type contentType);
    }
}