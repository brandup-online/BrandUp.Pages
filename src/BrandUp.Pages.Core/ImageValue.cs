namespace BrandUp.Pages
{
    public readonly struct ImageValue : IEquatable<ImageValue>, IComparable<ImageValue>, IParsable<ImageValue>
    {
        static readonly char[] ValueSeparator = ['(', ')'];

        public ImageValueType ValueType { get; }
        public string Value { get; }
        public bool HasValue => Value != null;

        public ImageValue(Guid fileId)
        {
            if (fileId == Guid.Empty)
                throw new ArgumentException("File ID value require not equal Guid.Empty.");

            ValueType = ImageValueType.Id;
            Value = fileId.ToString();
        }
        public ImageValue(Uri url)
        {
            ArgumentNullException.ThrowIfNull(url);

            ValueType = ImageValueType.Url;
            Value = url.ToString();
        }

        #region IParsable members

        public static ImageValue Parse(string value, IFormatProvider formatProvider)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!TryParse(value, formatProvider, out var result))
                throw new FormatException($"Unable parse value {value} as {typeof(ImageValue).FullName}.");

            return result;
        }

        public static bool TryParse(string value, out ImageValue imageValue)
        {
            return TryParse(value, null, out imageValue);
        }

        public static bool TryParse(string value, IFormatProvider formatProvider, out ImageValue imageValue)
        {
            ArgumentNullException.ThrowIfNull(value);

            imageValue = default;

            var temp = value.Split(ValueSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length != 2)
                return false;

            if (!Enum.TryParse(temp[0], true, out ImageValueType valueType))
                return false;

            switch (valueType)
            {
                case ImageValueType.Id:
                    {
                        if (!Guid.TryParse(temp[1], out Guid fileId))
                            return false;

                        imageValue = new ImageValue(fileId);
                        break;
                    }
                case ImageValueType.Url:
                    {
                        if (!Uri.TryCreate(temp[1], UriKind.RelativeOrAbsolute, out Uri url))
                            return false;

                        imageValue = new ImageValue(url);
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }

            return true;
        }

        #endregion

        #region IEquatable members

        public bool Equals(ImageValue other)
        {
            return ValueType == other.ValueType && string.Compare(Value, other.Value, true) == 0;
        }

        #endregion

        #region IComparable members

        int IComparable<ImageValue>.CompareTo(ImageValue other)
        {
            var value = Value ?? string.Empty;
            return value.CompareTo(other.Value ?? string.Empty);
        }

        #endregion

        #region Object members

        public override bool Equals(object obj)
        {
            if (obj is not ImageValue)
                return false;

            return Equals((ImageValue)obj);
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

        public static bool operator ==(ImageValue x, ImageValue y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(ImageValue x, ImageValue y)
        {
            return !(x == y);
        }

        public static implicit operator string(ImageValue imageValue)
        {
            return imageValue.ToString();
        }
        public static implicit operator ImageValue(string str)
        {
            if (!TryParse(str, out ImageValue imageValue))
                throw new FormatException();

            return imageValue;
        }
        public static implicit operator Uri(ImageValue imageValue)
        {
            if (imageValue.ValueType != ImageValueType.Url)
                throw new ArgumentException();

            return new Uri(imageValue.Value);
        }
        public static implicit operator ImageValue(Uri url)
        {
            return new ImageValue(url);
        }
        public static implicit operator Guid(ImageValue imageValue)
        {
            if (imageValue.ValueType != ImageValueType.Id)
                throw new ArgumentException();

            return new Guid(imageValue.Value);
        }
        public static implicit operator ImageValue(Guid id)
        {
            return new ImageValue(id);
        }

        #endregion
    }

    public enum ImageValueType
    {
        Id,
        Url
    }
}