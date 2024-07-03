using System.Collections;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class ModelAttribute : FieldProviderAttribute, IModelField
    {
        static readonly Type ListInterfaceType = typeof(IList);
        static readonly Type ListGenericTypeDefinition = typeof(List<>);
        ConstructorInfo listConstructor;

        public string AddText { get; set; }

        #region  IModelField members

        public ContentMetadata ValueContentMetadata { get; private set; }
        public bool IsListValue { get; private set; }
        public object Navigate(object value, int index)
        {
            if (IsListValue)
            {
                if (value == null)
                    return null;
                if (index < 0)
                    throw new ArgumentException();

                var list = (IList)value;
                return list[index];
            }
            else
                return value;
        }
        public object ChangeType(object value, string newTypeName)
        {
            return null;
        }

        #endregion

        #region FieldProviderAttribute members

        protected override void OnInitialize()
        {
            var valueType = ValueType;
            IsListValue = ListInterfaceType.IsAssignableFrom(valueType);

            Type contentType;
            if (IsListValue)
            {
                if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != ListGenericTypeDefinition)
                    throw new InvalidOperationException();

                listConstructor = valueType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, [], null);
                if (listConstructor == null)
                    throw new InvalidOperationException();

                contentType = valueType.GenericTypeArguments[0];
            }
            else
                contentType = valueType;

            if (!ContentMetadataManager.TypeIsContent(contentType.GetTypeInfo()))
                throw new InvalidOperationException();

            if (!ContentMetadata.Manager.TryGetMetadata(contentType, out ContentMetadata contentMetadata))
                throw new InvalidOperationException();
            ValueContentMetadata = contentMetadata;
        }

        public override bool HasValue(object value)
        {
            if (!base.HasValue(value))
                return false;

            if (IsListValue)
            {
                var list = (IList)value;
                return list.Count > 0;
            }

            return true;
        }

        public override object ConvetValueToData(object value)
        {
            if (IsListValue)
            {
                var list = (IList)value;
                var result = new List<IDictionary<string, object>>();
                foreach (var item in list)
                    result.Add(ValueContentMetadata.ConvertContentModelToDictionary(item));
                return result;
            }
            else
                return ValueContentMetadata.ConvertContentModelToDictionary(value);
        }

        public override object ConvetValueFromData(object value)
        {
            if (IsListValue)
            {
                var dataList = (IEnumerable<IDictionary<string, object>>)value;
                var result = CreateListValue();
                foreach (var itemData in dataList)
                {
                    var item = ValueContentMetadata.ConvertDictionaryToContentModel(itemData);
                    result.Add(item);
                }
                return result;
            }
            else
                return ValueContentMetadata.ConvertDictionaryToContentModel((IDictionary<string, object>)value);
        }

        public override object ParseValue(string strValue, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }

        public override object GetFormOptions(IServiceProvider services)
        {
            var options = new ModelFieldFormOptions
            {
                AddText = AddText,
                IsListValue = IsListValue,
                ItemTypes = []
            };

            foreach (var contentMetadata in ValueContentMetadata.GetDerivedMetadataWithHierarhy(true))
            {
                if (contentMetadata.ModelType.IsAbstract)
                    continue;

                options.ItemTypes.Add(new ContentItemType
                {
                    Name = contentMetadata.Name,
                    Title = contentMetadata.Title
                });
            }

            return options;
        }

        public override Task<object> GetFormValueAsync(object modelValue, IServiceProvider services)
        {
            var formValue = new ModelFieldFormValue
            {
                Items = []
            };

            if (modelValue != null)
            {
                if (IsListValue)
                {
                    var list = (IList)modelValue;
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            var itemMetadata = ContentMetadata.Manager.GetMetadata(item.GetType());
                            formValue.Items.Add(new ContentItem
                            {
                                Title = itemMetadata.GetContentTitle(item) ?? itemMetadata.Title,
                                Type = new ContentItemType
                                {
                                    Name = itemMetadata.Name,
                                    Title = itemMetadata.Title
                                }
                            });
                        }
                    }
                }
                else
                {
                    var itemMetadata = ContentMetadata.Manager.GetMetadata(modelValue.GetType());
                    formValue.Items.Add(new ContentItem
                    {
                        Title = itemMetadata.GetContentTitle(modelValue) ?? itemMetadata.Title,
                        Type = new ContentItemType
                        {
                            Name = itemMetadata.Name,
                            Title = itemMetadata.Title
                        }
                    });
                }
            }

            return Task.FromResult<object>(formValue);
        }

        #endregion

        #region Helpers

        private IList CreateListValue()
        {
            return (IList)listConstructor.Invoke([]);
        }

        #endregion
    }

    public interface IModelField : IFieldProvider
    {
        bool IsListValue { get; }
        object Navigate(object value, int index);
        ContentMetadata ValueContentMetadata { get; }
        object ChangeType(object value, string newTypeName);
    }

    public class ModelFieldFormOptions
    {
        public bool IsListValue { get; set; }
        public string AddText { get; set; }
        public ContentItemType ItemType { get; set; }
        public List<ContentItemType> ItemTypes { get; set; }
    }

    public class ModelFieldFormValue
    {
        public List<ContentItem> Items { get; set; }
    }

    public class ContentItemType
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }

    public class ContentItem
    {
        public string Title { get; set; }
        public ContentItemType Type { get; set; }
    }
}