using System;
using Website.Services;
using Xunit;

namespace WebsiteTest
{
    public class ShoppingServiceTests
    {
        [Fact]
        public void Test1()
        {
            var shoppingService = new ShoppingService(null);
            Assert.True(1 == 2);
        }
    }
}
