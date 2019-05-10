using System;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ImageValueTests
    {
        #region Test methods

        [Fact]
        public void Check_DefaultValue()
        {
            var value = default(ImageValue);

            Assert.Equal(ImageValueType.Id, value.ValueType);
            Assert.Null(value.Value);
            Assert.False(value.HasValue);
        }

        [Fact]
        public void Check_IdValue()
        {
            var fileId = Guid.NewGuid();
            var value = new ImageValue(fileId);

            Assert.Equal(ImageValueType.Id, value.ValueType);
            Assert.Equal(fileId.ToString(), value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void Check_UrlValue()
        {
            var fileUrl = new Uri("http://test/test.jpg");
            var value = new ImageValue(fileUrl);

            Assert.Equal(ImageValueType.Url, value.ValueType);
            Assert.Equal(fileUrl.ToString(), value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void TryParse_Id()
        {
            var fileId = Guid.NewGuid();
            var result = ImageValue.TryParse($"Id({fileId})", out ImageValue value);

            Assert.Equal(ImageValueType.Id, value.ValueType);
            Assert.Equal(fileId.ToString(), value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void TryParse_Url()
        {
            var fileUrl = new Uri("http://test/test.jpg");
            var result = ImageValue.TryParse($"Url({fileUrl})", out ImageValue value);

            Assert.Equal(ImageValueType.Url, value.ValueType);
            Assert.Equal(fileUrl.ToString(), value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void Equality_Equal_Url()
        {
            var url = new Uri("http://test/test.jpg");
            var value1 = new ImageValue(url);
            var value2 = new ImageValue(url);

            var result = value1.Equals(value2);

            Assert.True(result);
        }

        [Fact]
        public void Equality_Equal_Id()
        {
            var id = Guid.NewGuid();
            var value1 = new ImageValue(id);
            var value2 = new ImageValue(id);

            var result = value1.Equals(value2);

            Assert.True(result);
        }

        [Fact]
        public void Equality_Equal_Null()
        {
            var value1 = new ImageValue(new Uri("http://test/test.jpg"));

            var result = value1.Equals((object)null);

            Assert.False(result);
        }

        [Fact]
        public void Equality_Equal_ObjectNull()
        {
            var value1 = (object)new ImageValue(new Uri("http://test/test.jpg"));

            var result = value1 != null;

            Assert.True(result);
        }

        [Fact]
        public void Equality_Equal_ObjectNull2()
        {
            var value1 = (object)default(ImageValue);

            var result = value1 != null;

            Assert.True(result);
        }

        [Fact]
        public void Implicit_String_ImageValue()
        {
            var value = new ImageValue(new Uri("http://test/test.jpg"));

            string result = value;

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void Implicit_ImageValue_String()
        {
            var value = "url(http://test/test.jpg)";

            ImageValue result = value;

            Assert.Equal(value, result.ToString());
        }

        [Fact]
        public void Implicit_Uri_ImageValue()
        {
            var url = new Uri("http://test/test.jpg");
            var value = new ImageValue(url);

            Uri result = value;

            Assert.Equal(url, result);
        }

        [Fact]
        public void Implicit_ImageValue_Uri()
        {
            var url = new Uri("http://test/test.jpg");

            ImageValue result = url;

            Assert.Equal(url.ToString(), result.Value);
        }

        [Fact]
        public void Implicit_Guid_ImageValue()
        {
            var id = Guid.NewGuid();
            var value = new ImageValue(id);

            Guid result = value;

            Assert.Equal(id, result);
        }

        [Fact]
        public void Implicit_ImageValue_Guid()
        {
            var id = Guid.NewGuid();

            ImageValue result = id;

            Assert.Equal(id.ToString(), result.Value);
        }

        #endregion
    }
}