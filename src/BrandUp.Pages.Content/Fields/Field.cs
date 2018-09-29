using System;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public abstract class Field
    {
        private static readonly Type IEquatableType = typeof(IEquatable<>);
        private MethodInfo equalMethodInfo = null;
        private IFieldMember fieldDeclaration;

        #region Properties

        public IFieldMember Member => fieldDeclaration;
        public string Name { get; private set; }
        internal string JsonPropertyName { get; private set; }
        public string Title { get; private set; }
        public bool IsRequired { get; private set; }
        public Type ValueType { get; private set; }
        public bool AllowNull { get; private set; }

        #endregion

        protected internal Field() { }

        internal virtual void Initialize(ContentMetadataManager metadataProvider, MemberInfo fieldMember, FieldAttribute fieldAttribute)
        {
            switch (fieldMember.MemberType)
            {
                case MemberTypes.Field:
                    fieldDeclaration = new ModelFieldDeclarationAsField((FieldInfo)fieldMember);
                    break;
                case MemberTypes.Property:
                    fieldDeclaration = new ModelFieldDeclarationAsProperty((PropertyInfo)fieldMember);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            IsRequired = fieldAttribute.IsRequired;
            Title = fieldAttribute.Title;
            Name = fieldAttribute.Name;
            if (Name == null)
                Name = fieldDeclaration.Name;

            JsonPropertyName = Name.Substring(0, 1).ToLower() + Name.Substring(1);

            var valueType = fieldDeclaration.ValueType;

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
        }

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
            return fieldDeclaration.GetValue(model);
        }
        public bool TryGetValue(object model, out object value)
        {
            var val = fieldDeclaration.GetValue(model);
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
            fieldDeclaration.SetValue(model, value);
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

        public virtual object GetFormValue(object modelValue)
        {
            return modelValue;
        }
        public virtual object GetFormOptions()
        {
            return null;
        }

        public abstract object ParseValue(string strValue);

        private class ModelFieldDeclarationAsField : IFieldMember
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
        private class ModelFieldDeclarationAsProperty : IFieldMember
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

    public abstract class Field<TAttribute> : Field
        where TAttribute : FieldAttribute
    {
        protected Field() { }

        #region ViewModelField members

        internal override void Initialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, FieldAttribute fieldAttribute)
        {
            base.Initialize(metadataProvider, typeMember, fieldAttribute);

            OnInitialize(metadataProvider, typeMember, (TAttribute)fieldAttribute);
        }

        #endregion

        protected abstract void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, TAttribute attr);
    }

    public interface IFieldMember
    {
        MemberInfo Member { get; }
        string Name { get; }
        Type ValueType { get; }
        object GetValue(object obj);
        void SetValue(object obj, object value);
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public abstract class FieldAttribute : Attribute
    {
        public string Name { get; set; }
        public string Title { get; }
        public bool IsRequired { get; set; } = false;

        protected FieldAttribute(string title)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
        }

        public abstract Field CreateField();
    }

    public interface IFieldNavigationSupported
    {
        bool IsList { get; }
        object Navigate(object value, int index);
    }
}