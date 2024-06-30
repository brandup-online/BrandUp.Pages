namespace BrandUp.Pages
{
    public readonly struct ImageValue : IEquatable<ImageValue>
    {
        public ImageValueType ValueType { get; }
        public string Value { get; }
        public bool HasValue => Value != null;

        public ImageValue(Guid fileId)
        {
            if (fileId == Guid.Empty)
                throw new ArgumentException();

            ValueType = ImageValueType.Id;
            Value = fileId.ToString();
        }
        public ImageValue(Uri url)
        {
            ArgumentNullException.ThrowIfNull(url);

            ValueType = ImageValueType.Url;
            Value = url.ToString();
        }

        public static bool TryParse(string value, out ImageValue imageValue)
        {
            ArgumentNullException.ThrowIfNull(value);

            imageValue = default;

            var temp = value.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
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
                        var url = new Uri(temp[1], UriKind.RelativeOrAbsolute);
                        imageValue = new ImageValue(url);
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }

            return true;
        }

        #region IEquatable members

        public bool Equals(ImageValue other)
        {
            return ValueType == other.ValueType && string.Compare(Value, other.Value, true) == 0;
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