using System;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class HyperLinkValueTests
    {
        #region Test methods

        [Fact]
        public void Check_DefaultValue()
        {
            var value = default(HyperLinkValue);

            Assert.Equal(HyperLinkType.Page, value.ValueType);
            Assert.Null(value.Value);
            Assert.False(value.HasValue);
        }

        [Fact]
        public void Check_Constructor_String()
        {
            var pagePath = "test";
            var value = new HyperLinkValue(pagePath);

            Assert.Equal(HyperLinkType.Url, value.ValueType);
            Assert.Equal(pagePath, value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void Check_Constructor_Uri()
        {
            var fileUrl = new Uri("http://test/test.jpg");
            var value = new HyperLinkValue(fileUrl);

            Assert.Equal(HyperLinkType.Url, value.ValueType);
            Assert.Equal(fileUrl.ToString(), value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void Check_Constructor_Guid()
        {
            var pageId = Guid.NewGuid();
            var value = new HyperLinkValue(pageId);

            Assert.Equal(HyperLinkType.Page, value.ValueType);
            Assert.Equal(pageId.ToString(), value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void TryParse_Id()
        {
            var pageId = Guid.NewGuid();
            var pagePath = "url(" + pageId.ToString() + ")";
            var result = HyperLinkValue.TryParse(pagePath, out HyperLinkValue value);

            Assert.True(result);
            Assert.Equal(HyperLinkType.Url, value.ValueType);
            Assert.Equal(pageId.ToString(), value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void TryParse_Url()
        {
            var fileUrl = "url(http://test/test.jpg)";
            var result = HyperLinkValue.TryParse(fileUrl, out HyperLinkValue value);

            Assert.True(result);
            Assert.Equal(HyperLinkType.Url, value.ValueType);
            Assert.Equal("http://test/test.jpg", value.Value);
            Assert.True(value.HasValue);
        }

        [Fact]
        public void Equality_Equal_Url()
        {
            var url = new Uri("http://test/test.jpg");
            var value1 = new HyperLinkValue(url);
            var value2 = new HyperLinkValue(url);

            var result = value1.Equals(value2);

            Assert.True(result);
        }

        [Fact]
        public void Equality_Equal_Id()
        {
            var pagePath = "url(test)";
            var value1 = new HyperLinkValue(pagePath);
            var value2 = new HyperLinkValue(pagePath);

            var result = value1.Equals(value2);

            Assert.True(result);
        }

        [Fact]
        public void Equality_Equal_Null()
        {
            var value1 = new HyperLinkValue(new Uri("http://test/test.jpg"));

            var result = value1.Equals((object)null);

            Assert.False(result);
        }

        [Fact]
        public void Equality_Equal_ObjectNull()
        {
            var value1 = (object)new HyperLinkValue(new Uri("http://test/test.jpg"));

            var result = value1 != null;

            Assert.True(result);
        }

        [Fact]
        public void Equality_Equal_ObjectNull2()
        {
            var value1 = (object)default(HyperLinkValue);

            var result = value1 != null;

            Assert.True(result);
        }

        [Fact]
        public void Implicit_String_HyperLinkValue()
        {
            var value = new HyperLinkValue(new Uri("http://test/test.jpg"));

            string result = value;

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void Implicit_HyperLinkValue_String()
        {
            var value = "url(http://test/test.jpg)";

            HyperLinkValue result = value;

            Assert.Equal(value, result.ToString());
        }

        [Fact]
        public void Implicit_Uri_HyperLinkValue()
        {
            var url = new Uri("http://test/test.jpg");
            var value = new HyperLinkValue(url);

            Uri result = value;

            Assert.Equal(url, result);
        }

        [Fact]
        public void Implicit_HyperLinkValue_Uri()
        {
            var url = new Uri("http://test/test.jpg");

            HyperLinkValue result = url;

            Assert.Equal(url.ToString(), result.Value);
        }


        [Fact]
        public void Implicit_Guid_HyperLinkValue()
        {
            var id = Guid.NewGuid();
            var value = new HyperLinkValue(id);

            Guid result = value;

            Assert.Equal(id, result);
        }

        [Fact]
        public void Implicit_HyperLinkValue_Guid()
        {
            var id = Guid.NewGuid();

            HyperLinkValue result = id;

            Assert.Equal(id.ToString(), result.Value);
        }

        #endregion
    }
}