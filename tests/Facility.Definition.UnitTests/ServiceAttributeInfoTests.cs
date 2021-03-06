﻿using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceAttributeInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			var position = new NamedTextPosition("source");
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceAttributeInfo(name: "4u", position: position), position);
		}
	}
}
