using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Fields
{
    public class PagesAttribute : FieldProviderAttribute, IPagesField
    {
        private ConstructorInfo valueConstructor;

        public string Placeholder { get; set; }
        public Type PageModelType { get; private set; }

        #region FieldProviderAttribute members

        protected override void OnInitialize()
        {
            var valueType = ValueType;
            if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != typeof(PageCollectionReference<>))
                throw new InvalidOperationException();

            valueConstructor = valueType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Guid) }, null);
            if (valueConstructor == null)
                throw new InvalidOperationException();

            PageModelType = valueType.GenericTypeArguments[0];
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

            var pageMetadata = pageMetadataManager.FindPageMetadataByContentType(PageModelType);
            if (pageMetadata == null)
                throw new InvalidOperationException();

            return new PagesFieldFormOptions
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

            return new PagesFieldFormValue
            {
                Id = pageCollection.Id,
                Title = pageCollection.Title
            };
        }

        #endregion
    }

    public interface IPagesField : IFieldProvider
    {
        Type PageModelType { get; }
    }

    public class PagesFieldFormOptions
    {
        public string Placeholder { get; set; }
        public string PageType { get; set; }
    }

    public class PagesFieldFormValue
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }
}