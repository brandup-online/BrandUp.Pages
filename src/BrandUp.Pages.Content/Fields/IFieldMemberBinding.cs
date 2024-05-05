using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
	public interface IModelBinding
	{
		MemberInfo Member { get; }
		string Name { get; }
		Type ValueType { get; }
		object GetValue(object obj);
		void SetValue(object obj, object value);
	}

	internal class FieldModelBinding : IModelBinding
	{
		private readonly FieldInfo field;

		public FieldModelBinding(FieldInfo field)
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

	internal class PropertyModelBinding : IModelBinding
	{
		private readonly PropertyInfo property;

		public PropertyModelBinding(PropertyInfo property)
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