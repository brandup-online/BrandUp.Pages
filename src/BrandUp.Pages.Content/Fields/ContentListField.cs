using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Fields
{
    public class ContentListAttribute : FieldProviderAttribute, IFieldNavigationSupported
    {
        private ConstructorInfo _valueConstructor;

        public ContentMetadataProvider ValueContentMetadata { get; private set; }

        #region FieldProviderAttribute members

        protected override void OnInitialize()
        {
            var valueType = ValueType;

            if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != typeof(List<>))
                throw new InvalidOperationException();

            var listItemType = valueType.GenericTypeArguments[0];

            _valueConstructor = valueType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
            if (_valueConstructor == null)
                throw new InvalidOperationException();

            if (!ContentMetadataManager.IsContent(listItemType.GetTypeInfo()))
                throw new InvalidOperationException();

            if (!ContentMetadata.Manager.TryGetMetadata(listItemType, out ContentMetadataProvider contentMetadata))
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
            {
                result.Add(ValueContentMetadata.ConvertContentModelToDictionary(item));
            }

            return result;
        }
        public override object ConvetValueFromData(object value)
        {
            var dataList = (IEnumerable<IDictionary<string, object>>)value;
            var result = CreateValue();

            foreach (var itemData in dataList)
            {
                var item = ValueContentMetadata.ConvertDictionaryToContentModel(itemData);
                result.Add(item);
            }

            return result;
        }
        public override object ParseValue(string strValue)
        {
            throw new NotImplementedException();
        }

        public override Task<object> GetFormValueAsync(object modelValue, IServiceProvider services)
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
                    var itemMetadata = ContentMetadata.Manager.GetMetadata(item.GetType());

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

            return Task.FromResult<object>(formValue);
        }
        public override object GetFormOptions(IServiceProvider services)
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