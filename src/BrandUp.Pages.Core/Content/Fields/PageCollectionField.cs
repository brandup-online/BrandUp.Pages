using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Fields
{
    public class PageCollectionAttribute : FieldAttribute
    {
        public string Placeholder { get; set; }

        public override Field CreateField()
        {
            return new PageCollectionField();
        }
    }

    public class PageCollectionField : Field<PageCollectionAttribute>
    {
        private Type pageModelType;
        private ConstructorInfo valueConstructor;

        public string Placeholder { get; private set; }

        internal PageCollectionField() : base() { }

        #region ModelField members

        protected override void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, PageCollectionAttribute attr)
        {
            Placeholder = attr.Placeholder;

            var valueType = ValueType;
            if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != typeof(PageCollectionReference<>))
                throw new InvalidOperationException();

            valueConstructor = valueType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Guid) }, null);
            if (valueConstructor == null)
                throw new InvalidOperationException();

            pageModelType = valueType.GenericTypeArguments[0];
        }

        public override bool HasValue(object value)
        {
            if (!base.HasValue(value))
                return false;

            var colRef = (IPageCollectionReference)value;
            if (colRef.CollectionId == Guid.Empty)
                return false;
            return true;
        }

        public override object ParseValue(string strValue)
        {
            throw new NotImplementedException();
        }

        public override object ConvetValueToData(object value)
        {
            var img = (IPageCollectionReference)value;
            return img.CollectionId.ToString();
        }

        public override object ConvetValueFromData(object value)
        {
            var collectionId = Guid.Parse((string)value);
            return valueConstructor.Invoke(new object[] { collectionId });
        }

        public override object GetFormOptions(IServiceProvider services)
        {
            var pageMetadataManager = services.GetRequiredService<Metadata.IPageMetadataManager>();

            var pageMetadata = pageMetadataManager.FindPageMetadataByContentType(pageModelType);
            if (pageMetadata == null)
                throw new InvalidOperationException();

            return new PageCollectionFieldFormOptions
            {
                Placeholder = Placeholder,
                PageType = pageMetadata.Name
            };
        }

        public override async Task<object> GetFormValueAsync(object modelValue, IServiceProvider services)
        {
            if (!HasValue(modelValue))
                return null;

            var pageCollectionService = services.GetRequiredService<Interfaces.IPageCollectionService>();

            var value = (IPageCollectionReference)modelValue;
            var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(value.CollectionId);
            if (pageCollection == null)
                return null;

            return new PageCollectionFieldFormValue
            {
                Id = pageCollection.Id,
                Title = pageCollection.Title
            };
        }

        #endregion
    }

    public class PageCollectionFieldFormOptions
    {
        public string Placeholder { get; set; }
        public string PageType { get; set; }
    }

    public class PageCollectionFieldFormValue
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }
}