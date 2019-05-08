using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Fields
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public abstract class FieldProviderAttribute : Attribute, IFieldProvider
    {
        private static readonly Type IEquatableType = typeof(IEquatable<>);
        private MethodInfo equalMethodInfo = null;
        private IFieldModelMember fieldModelMember;

        #region Properties

        internal string JsonPropertyName { get; private set; }

        #endregion

        protected internal FieldProviderAttribute() { }

        #region IFieldProvider members

        public IFieldModelMember Member => fieldModelMember;
        public string Name { get; set; }
        public string Title { get; set; }
        public bool IsRequired { get; set; } = false;
        public Type ValueType { get; private set; }
        public bool AllowNull { get; private set; }

        #endregion

        internal virtual void Initialize(ContentMetadataManager metadataProvider, MemberInfo fieldMember)
        {
            switch (fieldMember.MemberType)
            {
                case MemberTypes.Field:
                    fieldModelMember = new ModelFieldDeclarationAsField((FieldInfo)fieldMember);
                    break;
                case MemberTypes.Property:
                    fieldModelMember = new ModelFieldDeclarationAsProperty((PropertyInfo)fieldMember);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (Name == null)
                Name = fieldModelMember.Name;
            if (Title == null)
                Title = Name;

            JsonPropertyName = Name.Substring(0, 1).ToLower() + Name.Substring(1);

            var valueType = fieldModelMember.ValueType;

            if (IsNullable(valueType))
            {
                AllowNull = true;
                valueType = valueType.GenericTypeArguments[0];
            }
            else if (valueType == typeof(string))
                AllowNull = true;
            else if (!valueType.IsValueType)
                AllowNull = true;

            ValueType = valueType;

            OnInitialize(metadataProvider, fieldMember);
        }
        protected abstract void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember);

        private static bool IsNullable(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        #region Value methods

        [System.Diagnostics.DebuggerStepThrough]
        public virtual bool HasValue(object value)
        {
            return value != null;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public object GetModelValue(object model)
        {
            return fieldModelMember.GetValue(model);
        }
        public bool TryGetValue(object model, out object value)
        {
            var val = fieldModelMember.GetValue(model);
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
            fieldModelMember.SetValue(model, value);
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
                return (bool)equalMethodInfo.Invoke(left, new object[] { right });

            return left.Equals(right);
        }

        #endregion

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

        private class ModelFieldDeclarationAsField : IFieldModelMember
        {
            private readonly FieldInfo field;

            public ModelFieldDeclarationAsField(FieldInfo field)
            {
                this.field = field;
            }

            public MemberInfo Member => field;
            public string Name => field.Name;
            public Type ValueType => field.FieldType;
            public object GetValue(object obj)
            {
                return field.GetValue(obj);
            }
            public void SetValue(object obj, object value)
            {
                field.SetValue(obj, value);
            }
        }
        private class ModelFieldDeclarationAsProperty : IFieldModelMember
        {
            private readonly PropertyInfo property;

            public ModelFieldDeclarationAsProperty(PropertyInfo property)
            {
                this.property = property;
            }

            public MemberInfo Member => property;
            public string Name => property.Name;
            public Type ValueType => property.PropertyType;
            public object GetValue(object obj)
            {
                return property.GetValue(obj);
            }
            public void SetValue(object obj, object value)
            {
                property.SetValue(obj, value);
            }
        }
    }

    public interface IFieldProvider
    {
        IFieldModelMember Member { get; }
        string Name { get; }
        string Title { get; }
        bool IsRequired { get; }
        Type ValueType { get; }
        bool AllowNull { get; }
        bool HasValue(object value);
        object GetModelValue(object model);
        bool TryGetValue(object model, out object value);
        void SetModelValue(object model, object value);
        bool CompareValues(object left, object right);
        object ConvetValueToData(object value);
        object ConvetValueFromData(object value);
        Task<object> GetFormValueAsync(object modelValue, IServiceProvider services);
        object GetFormOptions(IServiceProvider services);
        object ParseValue(string strValue);
    }

    public interface IFieldModelMember
    {
        MemberInfo Member { get; }
        string Name { get; }
        Type ValueType { get; }
        object GetValue(object obj);
        void SetValue(object obj, object value);
    }

    public interface IFieldNavigationSupported
    {
        bool IsList { get; }
        object Navigate(object value, int index);
    }
}