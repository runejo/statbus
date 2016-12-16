﻿using nscreg.Utilities;
using Xunit;

namespace nscreg.Server.Test
{
    public class StringExtensionsTest
    {
        [Fact]
        public void IsPrintableString()
        {
            Assert.True("123qwe".IsPrintable());
        }

        [Fact]
        public void IsNotPrintableString()
        {
            Assert.False("¤•2€3©".IsPrintable());
        }

        [Fact]
        public void Pascal2CamelStringTest()
        {
            Assert.Equal("aaBb", "AaBb".Pascal2Camel());
        }
    }
}
