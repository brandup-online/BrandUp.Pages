namespace BrandUp.Pages
{
	public readonly struct HyperLinkValue
	{
		public HyperLinkType ValueType { get; }
		public string Value { get; }
		public bool HasValue => Value != null;
		public Uri Url
		{
			get
			{
				if (!HasValue)
					return null;

				switch (ValueType)
				{
					case HyperLinkType.Url:
						return new Uri(Value, UriKind.Absolute);
					case HyperLinkType.Page:
						return new Uri(Value, UriKind.Relative);
					default:
						throw new InvalidOperationException();
				}
			}
		}

		public HyperLinkValue(Guid pageId)
		{
			if (pageId == Guid.Empty)
				throw new ArgumentException();

			ValueType = HyperLinkType.Page;
			Value = pageId.ToString();
		}
		public HyperLinkValue(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentException();

			ValueType = HyperLinkType.Url;
			Value = new Uri(value, UriKind.RelativeOrAbsolute).ToString();
		}
		public HyperLinkValue(Uri url)
		{
			if (url == null)
				throw new ArgumentException();

			ValueType = HyperLinkType.Url;
			Value = url.ToString();
		}

		public static bool TryParse(string value, out HyperLinkValue hyperLink)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			hyperLink = default;

			var temp = value.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
			if (temp.Length != 2)
				return false;

			if (!Enum.TryParse(temp[0], true, out HyperLinkType valueType))
				return false;

			switch (valueType)
			{
				case HyperLinkType.Page:
					{
						if (!Guid.TryParse(temp[1], out Guid pageId))
							return false;
						hyperLink = new HyperLinkValue(pageId);
						break;
					}
				case HyperLinkType.Url:
					{
						if (!Uri.TryCreate(temp[1], UriKind.RelativeOrAbsolute, out Uri uri))
							return false;

						hyperLink = new HyperLinkValue(uri);
						break;
					}
				default:
					throw new InvalidOperationException();
			}

			return true;
		}

		#region IEquatable members

		public bool Equals(HyperLinkValue other)
		{
			return ValueType == other.ValueType && string.Compare(Value, other.Value, true) == 0;
		}

		#endregion

		#region Object members

		public override bool Equals(object obj)
		{
			if (!(obj is HyperLinkValue))
				return false;

			return Equals((HyperLinkValue)obj);
		}
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
		public override string ToString()
		{
			if (!HasValue)
				return string.Empty;

			return $"{ValueType.ToString().ToLower()}({Value})";
		}

		#endregion

		#region Operators

		public static bool operator ==(HyperLinkValue x, HyperLinkValue y)
		{
			return x.Equals(y);
		}
		public static bool operator !=(HyperLinkValue x, HyperLinkValue y)
		{
			return !(x == y);
		}

		public static implicit operator string(HyperLinkValue hyperLink)
		{
			return hyperLink.ToString();
		}
		public static implicit operator HyperLinkValue(string str)
		{
			if (!TryParse(str, out HyperLinkValue imageValue))
				throw new FormatException();

			return imageValue;
		}
		public static implicit operator Uri(HyperLinkValue hyperLink)
		{
			if (hyperLink.ValueType != HyperLinkType.Url)
				throw new ArgumentException();

			return new Uri(hyperLink.Value, UriKind.RelativeOrAbsolute);
		}
		public static implicit operator HyperLinkValue(Uri url)
		{
			return new HyperLinkValue(url);
		}

		public static implicit operator Guid(HyperLinkValue imageValue)
		{
			if (imageValue.ValueType != HyperLinkType.Page)
				throw new ArgumentException();

			return new Guid(imageValue.Value);
		}
		public static implicit operator HyperLinkValue(Guid id)
		{
			return new HyperLinkValue(id);
		}

		#endregion
	}

	public enum HyperLinkType
	{
		Page,
		Url
	}
}