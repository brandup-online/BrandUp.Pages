using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public abstract class FieldProviderAttribute : Attribute, IFieldProvider
    {
        const string TypeValueSuffiks = "Attribute";
        static readonly Type IEquatableType = typeof(IEquatable<>);
        readonly MethodInfo equalMethodInfo = null;
        IModelBinding modelBinding;

        #region Properties

        internal string JsonPropertyName { get; private set; }

        #endregion

        protected internal FieldProviderAttribute() { }

        internal virtual void Initialize(ContentMetadataProvider contentMetadata, IModelBinding modelBinding)
        {
            ContentMetadata = contentMetadata;
            this.modelBinding = modelBinding;

            Type = GetType().Name;
            if (Type.EndsWith(TypeValueSuffiks))
                Type = Type[..^TypeValueSuffiks.Length];

            Name ??= this.modelBinding.Name;
            Title ??= Name;

            JsonPropertyName = string.Concat(Name[..1].ToLower(), Name.AsSpan(1));

            var valueType = this.modelBinding.ValueType;

            if (valueType.IsNullable())
            {
                AllowNull = true;
                valueType = valueType.GenericTypeArguments[0];
            }
            else if (valueType == typeof(string))
                AllowNull = true;
            else if (!valueType.IsValueType)
                AllowNull = true;

            ValueType = valueType;

            OnInitialize();
        }
        protected abstract void OnInitialize();

        #region IFieldProvider members

        public ContentMetadataProvider ContentMetadata { get; private set; }
        public IModelBinding Binding => modelBinding;
        public string Type { get; private set; }
        public string Name { get; private set; }
        public string Title { get; set; }
        public Type ValueType { get; private set; }
        public bool AllowNull { get; private set; }

        [System.Diagnostics.DebuggerStepThrough]
        public virtual bool HasValue(object value)
        {
            return value != null;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public object GetModelValue(object model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return modelBinding.GetValue(model);
        }
        public bool TryGetModelValue(object model, out object value)
        {
            var val = GetModelValue(model);
            if (!HasValue(val))
            {
                value = null;
                return false;
            }

            value = val;
            return true;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public void SetModelValue(object model, object value)
        {
            ArgumentNullException.ThrowIfNull(model);

            modelBinding.SetValue(model, value);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public virtual bool CompareValues(object left, object right)
        {
            var leftIsNull = left == null;
            var rightIsNull = right == null;

            // Если левое значение равно null, а правок нет.
            if (leftIsNull && !rightIsNull)
                return false;
            // Если левое значение не равно null, а правое да.
            if (!leftIsNull && rightIsNull)
                return false;
            // Если оба значения равны null, то считаем значения одинаковыми.
            if (leftIsNull && rightIsNull)
                return true;

            if (left.GetType() != right.GetType())
                return false;

            if (ReferenceEquals(left, right))
                return true;

            if (equalMethodInfo != null)
                return (bool)equalMethodInfo.Invoke(left, [right]);

            return left.Equals(right);
        }
        public virtual object ConvetValueToData(object value)
        {
            return value;
        }
        public virtual object ConvetValueFromData(object value)
        {
            return value;
        }
        public virtual Task<object> GetFormValueAsync(object modelValue, IServiceProvider services)
        {
            return Task.FromResult(modelValue);
        }
        public virtual object GetFormOptions(IServiceProvider services)
        {
            return null;
        }
        public abstract object ParseValue(string strValue);

        #endregion

        #region Object members

        public override string ToString()
        {
            return $"{ContentMetadata.Name}.{Name} ({Type})";
        }

        #endregion
    }
}