using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class ContentListField : Field<ContentListAttribute>, IFieldNavigationSupported
    {
        private ContentMetadataManager metadataManager;
        private ConstructorInfo _valueConstructor;

        public ContentMetadataProvider ValueContentMetadata { get; private set; }

        #region Field members

        protected override void OnInitialize(ContentMetadataManager metadataManager, MemberInfo typeMember, ContentListAttribute attr)
        {
            this.metadataManager = metadataManager;

            var valueType = ValueType;

            if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != typeof(List<>))
                throw new InvalidOperationException();

            var listItemType = valueType.GenericTypeArguments[0];

            _valueConstructor = valueType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
            if (_valueConstructor == null)
                throw new InvalidOperationException();

            if (!ContentMetadataManager.IsContent(listItemType.GetTypeInfo()))
                throw new InvalidOperationException();

            if (!metadataManager.TryGetMetadata(listItemType, out ContentMetadataProvider contentMetadata))
                throw new InvalidOperationException();
            ValueContentMetadata = contentMetadata;
        }
        public override bool HasValue(object value)
        {
            if (!base.HasValue(value))
                return false;

            var list = (IList)value;
            return list.Count > 0;
        }
        public override object ConvetValueToData(object value)
        {
            var list = (IList)value;

            var result = new List<IDictionary<string, object>>();
            foreach (var item in list)
                result.Add(metadataManager.ConvertContentModelToDictionary(item));

            return result;
        }
        public override object ConvetValueFromData(object value)
        {
            var dataList = (IEnumerable<IDictionary<string, object>>)value;
            var result = CreateValue();

            foreach (var itemData in dataList)
            {
                var item = metadataManager.ConvertDictionaryToContentModel(itemData);
                result.Add(item);
            }

            return result;
        }
        public override object ParseValue(string strValue)
        {
            throw new NotImplementedException();
        }

        public override object GetFormValue(object modelValue)
        {
            var list = (IList)modelValue;

            var formValue = new ContentListFieldFormValue
            {
                Items = new List<ContentListItem>()
            };

            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    var itemMetadata = metadataManager.GetMetadata(item.GetType());

                    formValue.Items.Add(new ContentListItem
                    {
                        Type = new ContentListItemType
                        {
                            Name = itemMetadata.Name,
                            Title = itemMetadata.Title
                        }
                    });
                }
            }

            return formValue;
        }
        public override object GetFormOptions()
        {
            var options = new ContentListFieldFormOptions
            {
                ItemTypes = new List<ContentListItemType>()
            };

            foreach (var contentMetadata in ValueContentMetadata.GetDerivedMetadataWithHierarhy(true))
            {
                if (contentMetadata.ModelType.IsAbstract)
                    continue;

                options.ItemTypes.Add(new ContentListItemType
                {
                    Name = contentMetadata.Name,
                    Title = contentMetadata.Title
                });
            }

            return options;
        }

        #endregion

        #region IFieldNavigationSupported members

        bool IFieldNavigationSupported.IsList => true;
        object IFieldNavigationSupported.Navigate(object value, int index)
        {
            if (value == null)
                return null;
            if (index < 0)
                throw new ArgumentException();

            var list = (IList)value;
            return list[index];
        }

        #endregion

        #region Helpers

        public IList CreateValue()
        {
            return (IList)_valueConstructor.Invoke(new object[0]);
        }

        #endregion
    }

    public class ContentListAttribute : FieldAttribute
    {
        public ContentListAttribute(string title) : base(title) { }

        public override Field CreateField()
        {
            return new ContentListField();
        }
    }

    public class ContentListFieldFormOptions
    {
        public List<ContentListItemType> ItemTypes { get; set; }
    }

    public class ContentListFieldFormValue
    {
        public List<ContentListItem> Items { get; set; }
    }

    public class ContentListItemType
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }

    public class ContentListItem
    {
        public ContentListItemType Type { get; set; }
    }
}