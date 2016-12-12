﻿using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Facility.Definition.UnitTests.Fsd
{
	public sealed class AttributeTests
	{
		[Test]
		public void NoParameters()
		{
			var service = TestUtility.ParseTestApi("[x] service TestApi{}");

			var attribute = service.Attributes.Single();
			attribute.Name.ShouldBe("x");
			attribute.Parameters.Count.ShouldBe(0);

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"[x]",
				"service TestApi",
				"{",
				"}",
				"",
			});
		}

		[Test]
		public void ZeroParameter()
		{
			var service = TestUtility.ParseTestApi("[x(y:0)] service TestApi{}");

			var attribute = service.Attributes.Single();
			attribute.Name.ShouldBe("x");
			var parameter = attribute.Parameters.Single();
			parameter.Name.ShouldBe("y");
			parameter.Value.ShouldBe("0");

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"[x(y: 0)]",
				"service TestApi",
				"{",
				"}",
				"",
			});
		}

		[Test]
		public void TokenParameter()
		{
			var service = TestUtility.ParseTestApi("[x(y:1b-3D_5f.7H+9J)] service TestApi{}");

			var attribute = service.Attributes.Single();
			attribute.Name.ShouldBe("x");
			var parameter = attribute.Parameters.Single();
			parameter.Name.ShouldBe("y");
			parameter.Value.ShouldBe("1b-3D_5f.7H+9J");

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"[x(y: 1b-3D_5f.7H+9J)]",
				"service TestApi",
				"{",
				"}",
				"",
			});
		}

		[Test]
		public void EmptyStringParameter()
		{
			var service = TestUtility.ParseTestApi("[x(y:\"\")] service TestApi{}");

			var attribute = service.Attributes.Single();
			attribute.Name.ShouldBe("x");
			var parameter = attribute.Parameters.Single();
			parameter.Name.ShouldBe("y");
			parameter.Value.ShouldBe("");

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"[x(y: \"\")]",
				"service TestApi",
				"{",
				"}",
				"",
			});
		}

		[Test]
		public void JsonStringParameter()
		{
			var service = TestUtility.ParseTestApi(@"[x(y:""á\\\""\/\b\f\n\r\t\u1234!"")] service TestApi{}");

			var attribute = service.Attributes.Single();
			attribute.Name.ShouldBe("x");
			var parameter = attribute.Parameters.Single();
			parameter.Name.ShouldBe("y");
			parameter.Value.ShouldBe("á\\\"/\b\f\n\r\t\u1234!");

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"[x(y: \"á\\\\\\\"/\\b\\f\\n\\r\\t\u1234!\")]",
				"service TestApi",
				"{",
				"}",
				"",
			});
		}

		[Test]
		public void ManyAttributesAndParameters()
		{
			var service = TestUtility.ParseTestApi("[x, x(x:0)] [x(x:0,y:1)] service TestApi{}");

			service.Attributes.Count.ShouldBe(3);

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"[x]",
				"[x(x: 0)]",
				"[x(x: 0, y: 1)]",
				"service TestApi",
				"{",
				"}",
				"",
			});
		}
	}
}