using System;
using ComAdmin.Extensions;
using NUnit.Framework;

namespace ComAdmin.Tests.Extensions
{
    public class GuidExtensionsTests
    {
        [Test]
        public static void ToRegistryString_Can_Write_Guid_In_Registry_Format()
        {
            Assert.That(new Guid("00000000-1111-2222-3333-444444444444").ToRegistryString(),
                Is.EqualTo(@"{00000000-1111-2222-3333-444444444444}"));
        }
    }
}
