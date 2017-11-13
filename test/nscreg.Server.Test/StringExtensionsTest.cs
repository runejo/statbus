using nscreg.Utilities.Extensions;
using Xunit;

namespace nscreg.Server.Test
{
    public class StringExtensionsTest
    {
        [Fact]
        void IsPrintableString()
        {
            Assert.True("123qwe".IsPrintable());
        }

        [Fact]
        void IsNotPrintableString()
        {
            Assert.False("¤•2€3©".IsPrintable());
        }

        [Fact]
        void OnlyFirstLetterCaseLowered()
        {
            Assert.Equal("abCd", "AbCd".LowerFirstLetter());
        }

        [Fact]
        void SingleLetterStringCaseLowered()
        {
            Assert.Equal("a", "A".LowerFirstLetter());
        }

        [Fact]
        void SingleLetterStringCaseUppered()
        {
            Assert.Equal("A", "a".UpperFirstLetter());
        }

        [Fact]
        void OnlyFirstLetterCaseUppered()
        {
            Assert.Equal("AbCd", "abCd".UpperFirstLetter());
        }
    }
}
