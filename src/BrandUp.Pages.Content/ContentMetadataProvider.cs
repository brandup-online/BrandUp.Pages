using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Content.Views;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataProvider
    {
        #region Fields

        private static readonly object[] ModelConstructorParameters = new object[0];
        private readonly ConstructorInfo modelConstructor = null;
        private readonly List<ContentMetadataProvider> derivedContents = new List<ContentMetadataProvider>();
        private readonly List<Field> fields = new List<Field>();
        private readonly Dictionary<string, int> fieldNames = new Dictionary<string, int>();
        private ViewField viewField = null;
        private readonly List<ContentView> views = new List<ContentView>();
        private readonly Dictionary<string, int> viewNames = new Dictionary<string, int>();
        private ContentView defaultView;

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
                modelConstructor = modelType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
                if (modelConstructor == null)
                    throw new InvalidOperationException($"Тип модели контента \"{modelType}\" не содержит публичный конструктор без параметров.");
            }

            if (baseMetadata != null)
                baseMetadata.derivedContents.Add(this);

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
        public IEnumerable<ContentMetadataProvider> DerivedContents => derivedContents;
        public bool SupportViews => viewField != null;
        public IEnumerable<Field> Fields => fields;
        public ViewField ViewField => viewField;
        public IEnumerable<ContentView> Views => views;
        public bool HasViews => views.Count > 0;

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
                foreach (var field in baseModelMetadata.fields)
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
            var fIndex = fields.Count;

            fieldNames.Add(field.Name.ToLower(), fIndex);
            fields.Add(field);

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

            if (!fieldNames.TryGetValue(fieldName.ToLower(), out int index))
            {
                field = null;
                return false;
            }
            field = fields[index];
            return true;
        }

        internal void InitializeViews(IContentViewConfiguration viewConfiguration)
        {
            var baseModelMetadata = BaseMetadata;
            if (baseModelMetadata != null)
            {
                foreach (var view in baseModelMetadata.views)
                    AddView(view);

                defaultView = baseModelMetadata.defaultView;
            }

            if (viewConfiguration != null)
            {
                foreach (var viewDefinition in viewConfiguration.Views)
                    AddView(new ContentView(this, viewDefinition));

                if (viewConfiguration.DefaultViewName != null)
                {
                    if (!TryGetView(viewConfiguration.DefaultViewName, out defaultView))
                        throw new InvalidOperationException($"Не найдено представление {viewConfiguration.DefaultViewName}, которое предпологалось использовать по умолчанию для типа контента {Name}.");
                }
            }

            if (defaultView == null && views.Count > 0)
                defaultView = views[0];
        }
        private void AddView(ContentView view)
        {
            var index = views.Count;

            views.Add(view);
            viewNames.Add(view.Name.ToLower(), index);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public bool TryGetView(string viewName, out ContentView field)
        {
            CheckSupportedViews();

            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (!viewNames.TryGetValue(viewName.ToLower(), out int index))
            {
                field = null;
                return false;
            }
            field = views[index];
            return true;
        }
        public string GetViewName(object model)
        {
            CheckSupportedViews();

            if (!viewField.TryGetValue(model, out object viewName))
                return defaultView.Name;

            return (string)viewName;
        }
        public void SetViewName(object model, string viewName)
        {
            CheckSupportedViews();

            viewField.SetModelValue(model, viewName);
        }
        private void CheckSupportedViews()
        {
            if (viewField == null)
                throw new InvalidOperationException($"Content \"{Name}\" is not support views.");
        }

        public object CreateModelInstance()
        {
            return modelConstructor.Invoke(new object[0]);
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