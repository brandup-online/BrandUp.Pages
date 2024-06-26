using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BrandUp.Pages.Content.Fields;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataProvider : IEquatable<ContentMetadataProvider>
    {
        #region Fields

        public const string ContentTypeNameDataKey = "_type";
        public static readonly string[] ContentTypePrefixes = ["Content", "Model"];
        readonly ConstructorInfo modelConstructor = null;
        readonly List<ContentMetadataProvider> derivedContents = [];
        readonly List<FieldProviderAttribute> fields = [];
        readonly Dictionary<string, int> fieldNames = [];
        ITextField titleField;
        readonly List<PropertyInfo> injectProperties;
        readonly bool isValidatable = false;

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
                modelConstructor = modelType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, [], null);
                if (modelConstructor == null)
                    throw new InvalidOperationException($"Тип модели контента \"{modelType}\" не содержит публичный конструктор без параметров.");

                injectProperties = [];
                InitializeInjectProperties();
            }

            if (typeof(IValidatableObject).IsAssignableFrom(modelType))
                isValidatable = true;

            baseMetadata?.derivedContents.Add(this);

            Name = contentTypeAttribute.Name ?? GetTypeName(modelType);
            Title = contentTypeAttribute.Title ?? Name;
            Description = contentTypeAttribute.Description;
        }

        #region Properties

        public ContentMetadataManager Manager { get; }
        public Type ModelType { get; }
        public string Name { get; }
        public string Title { get; }
        public string Description { get; }
        public ContentMetadataProvider BaseMetadata { get; }
        public IEnumerable<ContentMetadataProvider> DerivedContents => derivedContents;
        public IEnumerable<FieldProviderAttribute> Fields => fields;
        public bool IsAbstract => ModelType.IsAbstract;
        public bool IsValidatable => isValidatable;
        public bool IsDefinedTitleField => Title != null;

        #endregion

        #region Methods

        void InitializeInjectProperties()
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
                    if (field is not ITextField title)
                        throw new InvalidOperationException();

                    titleField = title;
                    break;
                }
            }
        }

        void InitializeField(FieldProviderAttribute field, MemberInfo fieldMember)
        {
            IModelBinding modelBinding = fieldMember.MemberType switch
            {
                MemberTypes.Field => new FieldModelBinding((FieldInfo)fieldMember),
                MemberTypes.Property => new PropertyModelBinding((PropertyInfo)fieldMember),
                _ => throw new InvalidOperationException(),
            };

            field.Initialize(this, modelBinding);

            AddField(field);
        }

        void AddField(FieldProviderAttribute field)
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
            ArgumentNullException.ThrowIfNull(fieldName);

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

            return modelConstructor.Invoke([]);
        }

        public void ApplyInjections(object model, IServiceProvider serviceProvider, bool injectInnerModels)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(serviceProvider);
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
            ArgumentNullException.ThrowIfNull(contentModel);

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
            ArgumentNullException.ThrowIfNull(dictionary);

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
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(contentModel);

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
            ArgumentNullException.ThrowIfNull(baseMetadataProvider);

            return IsInherited(baseMetadataProvider.ModelType);
        }

        public bool IsInherited(Type baseModelType)
        {
            ArgumentNullException.ThrowIfNull(baseModelType);

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
            ArgumentNullException.ThrowIfNull(contentModel);

            if (titleField != null)
                return (string)titleField.GetModelValue(contentModel);

            return null;
        }

        public void SetContentTitle(object contentModel, string title)
        {
            ArgumentNullException.ThrowIfNull(contentModel);

            if (!IsDefinedTitleField)
                throw new InvalidOperationException($"Title field is not defined by content type {Name}.");

            titleField.SetModelValue(contentModel, title);
        }

        #endregion

        #region Helper methods

        static string GetTypeName(Type type)
        {
            var name = type.Name;

            foreach (var namePrefix in ContentTypePrefixes)
            {
                if (name.EndsWith(namePrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    name = type.Name[..type.Name.LastIndexOf(namePrefix)];
                    break;
                }
            }

            if (type.IsNestedPublic)
            {
                var nestedName = GetTypeName(type.ReflectedType);
                return $"{nestedName}.{name}";
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