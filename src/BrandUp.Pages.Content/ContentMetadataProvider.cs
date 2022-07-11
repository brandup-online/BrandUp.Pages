using BrandUp.Pages.Content.Fields;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataProvider : IEquatable<ContentMetadataProvider>
    {
        #region Fields

        public const string ContentTypeNameDataKey = "_type";
        public static readonly string[] ContentTypePrefixes = new string[] { "Content", "Model" };
        readonly ConstructorInfo modelConstructor = null;
        readonly List<ContentMetadataProvider> derivedContents = new List<ContentMetadataProvider>();
        readonly List<FieldProviderAttribute> fields = new List<FieldProviderAttribute>();
        readonly Dictionary<string, int> fieldNames = new Dictionary<string, int>();
        ITextField titleField;
        readonly List<PropertyInfo> injectProperties;

        #endregion

        internal ContentMetadataProvider(ContentMetadataManager metadataManager, Type modelType, ContentMetadataProvider baseMetadata)
        {
            Manager = metadataManager;
            ModelType = modelType;
            BaseMetadata = baseMetadata;

            var contentTypeAttribute = modelType.GetCustomAttribute<ContentTypeAttribute>(false);
            if (contentTypeAttribute == null)
                throw new ArgumentException($"Для типа модели контента \"{modelType}\" не определён атрибут {nameof(ContentTypeAttribute)}.");

            if (!modelType.IsAbstract)
            {
                modelConstructor = modelType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
                if (modelConstructor == null)
                    throw new InvalidOperationException($"Тип модели контента \"{modelType}\" не содержит публичный конструктор без параметров.");

                injectProperties = new List<PropertyInfo>();
                InitializeInjectProperties();
            }

            if (baseMetadata != null)
                baseMetadata.derivedContents.Add(this);

            Name = contentTypeAttribute.Name ?? GetTypeName(modelType);
            Title = contentTypeAttribute.Title ?? Name;
            Description = contentTypeAttribute.Description;
        }

        #region Properties

        public IContentMetadataManager Manager { get; }
        public Type ModelType { get; }
        public string Name { get; }
        public string Title { get; }
        public string Description { get; }
        public ContentMetadataProvider BaseMetadata { get; }
        public IEnumerable<ContentMetadataProvider> DerivedContents => derivedContents;
        public IEnumerable<FieldProviderAttribute> Fields => fields;
        public bool IsAbstract => ModelType.IsAbstract;
        public bool IsDefinedTitleField => Title != null;

        #endregion

        #region Methods

        private void InitializeInjectProperties()
        {
            foreach (var propery in ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                var injectAttribute = propery.GetCustomAttribute<ContentInjectAttribute>(true);
                if (injectAttribute == null)
                    continue;

                injectProperties.Add(propery);
            }
        }
        internal void InitializeFields()
        {
            var baseModelMetadata = BaseMetadata;
            if (baseModelMetadata != null)
            {
                foreach (var field in baseModelMetadata.fields)
                    AddField(field);
            }

            foreach (var fieldInfo in ModelType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var field = fieldInfo.GetCustomAttribute<FieldProviderAttribute>(false);
                if (field == null)
                    continue;

                InitializeField(field, fieldInfo);
            }

            foreach (var propertyInfo in ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                var field = propertyInfo.GetCustomAttribute<FieldProviderAttribute>(false);
                if (field == null)
                    continue;

                InitializeField(field, propertyInfo);
            }

            foreach (var field in fields)
            {
                if (titleField != null)
                    throw new InvalidOperationException($"Title field already defined for content type {Name}.");

                var titleAttribute = field.Binding.Member.GetCustomAttribute<TitleAttribute>(true);
                if (titleAttribute != null)
                {
                    if (!(field is ITextField title))
                        throw new InvalidOperationException();

                    titleField = title;
                    break;
                }
            }
        }
        private void InitializeField(FieldProviderAttribute field, MemberInfo fieldMember)
        {
            IModelBinding modelBinding;

            switch (fieldMember.MemberType)
            {
                case MemberTypes.Field:
                    modelBinding = new FieldModelBinding((FieldInfo)fieldMember);
                    break;
                case MemberTypes.Property:
                    modelBinding = new PropertyModelBinding((PropertyInfo)fieldMember);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            field.Initialize(this, modelBinding);

            AddField(field);
        }
        private void AddField(FieldProviderAttribute field)
        {
            var fieldName = field.Name.ToLower();
            if (fieldNames.ContainsKey(fieldName))
                throw new InvalidOperationException($"Field by name {field.Name} already exists.");

            var fieldIndex = fields.Count;
            fieldNames.Add(fieldName, fieldIndex);
            fields.Add(field);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public bool TryGetField(string fieldName, out IFieldProvider field)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            if (!fieldNames.TryGetValue(fieldName.ToLower(), out int index))
            {
                field = null;
                return false;
            }
            field = fields[index];
            return true;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public object CreateModelInstance()
        {
            if (modelConstructor == null)
                throw new InvalidOperationException($"Content type {Name} is abstract.");

            return modelConstructor.Invoke(new object[0]);
        }
        public void ApplyInjections(object model, IServiceProvider serviceProvider, bool injectInnerModels)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (model.GetType() != ModelType)
                throw new ArgumentException();
            if (ModelType.IsAbstract)
                throw new InvalidOperationException();

            foreach (var injectProperty in injectProperties)
            {
                var injectValue = serviceProvider.GetService(injectProperty.PropertyType);
                injectProperty.SetValue(model, injectValue);
            }

            if (injectInnerModels)
            {
                foreach (var field in fields.OfType<IModelField>())
                {
                    var fieldValue = field.GetModelValue(model);
                    if (!field.HasValue(fieldValue))
                        continue;

                    if (field.IsListValue)
                    {
                        var list = (IList)fieldValue;
                        foreach (var item in list)
                        {
                            var fieldValueContentMetadata = Manager.GetMetadata(item.GetType());
                            fieldValueContentMetadata.ApplyInjections(item, serviceProvider, injectInnerModels);
                        }
                    }
                    else
                    {
                        var fieldValueContentMetadata = Manager.GetMetadata(fieldValue.GetType());
                        fieldValueContentMetadata.ApplyInjections(fieldValue, serviceProvider, injectInnerModels);
                    }
                }
            }
        }
        public IDictionary<string, object> ConvertContentModelToDictionary(object contentModel)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));

            var contentModelType = contentModel.GetType();
            var isSubClass = contentModelType.IsSubclassOf(ModelType);
            if (contentModelType != ModelType && !isSubClass)
                throw new ArgumentException("Is not valid content model type.", nameof(contentModel));

            if (isSubClass)
            {
                var deriverMetadata = Manager.GetMetadata(contentModelType);
                return deriverMetadata.ConvertContentModelToDictionary(contentModel);
            }

            var result = new SortedDictionary<string, object>
            {
                { ContentTypeNameDataKey, Name }
            };

            foreach (var field in Fields)
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

            if (dictionary.TryGetValue(ContentTypeNameDataKey, out object contentTypeNameValue))
            {
                var contentTypeName = (string)contentTypeNameValue;
                if (string.Compare(contentTypeName, Name, true) != 0)
                {
                    if (!Manager.TryGetMetadata(contentTypeName, out ContentMetadataProvider deriverMetadata))
                        throw new InvalidOperationException($"Не найден тип контента с именем {contentTypeName}.");
                    if (!deriverMetadata.ModelType.IsSubclassOf(ModelType))
                        throw new InvalidOperationException();

                    return deriverMetadata.ConvertDictionaryToContentModel(dictionary);
                }
            }
            else if (ModelType.IsAbstract)
                throw new InvalidOperationException($"Content data required property {ContentTypeNameDataKey}.");

            var contentModel = CreateModelInstance();

            foreach (var kv in dictionary)
            {
                if (!TryGetField(kv.Key, out IFieldProvider field))
                    continue;

                var dataValue = kv.Value;
                var value = field.ConvetValueFromData(dataValue);
                field.SetModelValue(contentModel, value);
            }

            return contentModel;
        }
        public object ApplyDataToModel(IDictionary<string, object> data, object contentModel)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));

            if (data.TryGetValue(ContentTypeNameDataKey, out object contentTypeNameValue))
            {
                var contentTypeName = (string)contentTypeNameValue;
                if (string.Compare(contentTypeName, Name, true) != 0)
                    throw new InvalidOperationException();
            }

            foreach (var kv in data)
            {
                if (!TryGetField(kv.Key, out IFieldProvider field))
                    continue;

                var dataValue = kv.Value;
                var value = field.ConvetValueFromData(dataValue);
                field.SetModelValue(contentModel, value);
            }

            return contentModel;
        }
        public bool IsInherited(ContentMetadataProvider baseMetadataProvider)
        {
            if (baseMetadataProvider == null)
                throw new ArgumentNullException(nameof(baseMetadataProvider));

            return IsInherited(baseMetadataProvider.ModelType);
        }
        public bool IsInherited(Type baseModelType)
        {
            if (baseModelType == null)
                throw new ArgumentNullException(nameof(baseModelType));

            return ModelType.IsSubclassOf(baseModelType);
        }
        public bool IsInheritedOrEqual(ContentMetadataProvider baseMetadataProvider)
        {
            return this == baseMetadataProvider || IsInherited(baseMetadataProvider);
        }
        public bool IsInheritedOrEqual(Type baseModelType)
        {
            return this == baseModelType || IsInherited(baseModelType);
        }
        public string GetContentTitle(object contentModel)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));

            if (titleField != null)
                return (string)titleField.GetModelValue(contentModel);
            return Title;
        }
        public void SetContentTitle(object contentModel, string title)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));
            if (!IsDefinedTitleField)
                throw new InvalidOperationException($"Title field is not defined by content type {Name}.");

            titleField.SetModelValue(contentModel, title);
        }

        #endregion

        #region Helper methods

        private static string GetTypeName(Type type)
        {
            var name = type.Name;
            foreach (var namePrefix in ContentTypePrefixes)
            {
                if (name.EndsWith(namePrefix))
                    return type.Name.Substring(0, type.Name.LastIndexOf(namePrefix));
            }
            return name;
        }

        #endregion

        #region IEquatable members

        public bool Equals(ContentMetadataProvider other)
        {
            if (other == null || !(other is ContentMetadataProvider))
                return false;

            return ModelType == other.ModelType;
        }

        #endregion

        #region Object members

        public override int GetHashCode()
        {
            return ModelType.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as ContentMetadataProvider);
        }
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Operators

        public static bool operator ==(ContentMetadataProvider x, ContentMetadataProvider y)
        {
            var xIsNull = Equals(x, null);
            var yIsNull = Equals(y, null);

            if (yIsNull && xIsNull)
                return true;

            if (xIsNull || yIsNull)
                return false;

            return x.Equals(y);
        }

        public static bool operator !=(ContentMetadataProvider x, ContentMetadataProvider y)
        {
            return !(x == y);
        }

        public static implicit operator Type(ContentMetadataProvider metadataProvider)
        {
            if (metadataProvider == null)
                return null;

            return metadataProvider.ModelType;
        }

        #endregion
    }
}