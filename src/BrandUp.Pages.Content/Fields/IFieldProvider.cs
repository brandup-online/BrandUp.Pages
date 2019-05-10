using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Fields
{
    public interface IFieldProvider
    {
        ContentMetadataProvider ContentMetadata { get; }
        IModelBinding Binding { get; }
        string Type { get; }
        string Name { get; }
        string Title { get; }
        bool IsRequired { get; }
        Type ValueType { get; }
        bool AllowNull { get; }
        bool HasValue(object value);
        object GetModelValue(object model);
        bool TryGetModelValue(object model, out object value);
        void SetModelValue(object model, object value);
        bool CompareValues(object left, object right);
        object ConvetValueToData(object value);
        object ConvetValueFromData(object value);
        Task<object> GetFormValueAsync(object modelValue, IServiceProvider services);
        object GetFormOptions(IServiceProvider services);
        object ParseValue(string strValue);
    }
}