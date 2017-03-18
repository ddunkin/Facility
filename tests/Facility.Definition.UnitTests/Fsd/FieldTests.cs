﻿using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Facility.Definition.UnitTests.Fsd
{
	public sealed class FieldTests
	{
		[TestCase("string", ServiceTypeKind.String)]
		[TestCase("boolean", ServiceTypeKind.Boolean)]
		[TestCase("double", ServiceTypeKind.Double)]
		[TestCase("int32", ServiceTypeKind.Int32)]
		[TestCase("int64", ServiceTypeKind.Int64)]
		[TestCase("decimal", ServiceTypeKind.Decimal)]
		[TestCase("bytes", ServiceTypeKind.Bytes)]
		[TestCase("object", ServiceTypeKind.Object)]
		[TestCase("error", ServiceTypeKind.Error)]
		public void PrimitiveFields(string name, ServiceTypeKind kind)
		{
			var service = TestUtility.ParseTestApi("service TestApi { data One { x: xyzzy; } }".Replace("xyzzy", name));

			var dto = service.Dtos.Single();
			var field = dto.Fields.Single();
			field.Name.ShouldBe("x");
			field.Attributes.Count.ShouldBe(0);
			field.Summary.ShouldBe("");
			var type = service.GetFieldType(field);
			type.Kind.ShouldBe(kind);
			type.Dto.ShouldBeNull();
			type.Enum.ShouldBeNull();
			type.ValueType.ShouldBeNull();

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"service TestApi",
				"{",
				"\tdata One",
				"\t{",
				$"\t\tx: {name};",
				"\t}",
				"}",
				"",
			});
		}

		[Test]
		public void CaseSensitivePrimitive()
		{
			TestUtility.ParseInvalidTestApi("service TestApi { data One { x: Boolean; } }");
		}

		[Test]
		public void EnumField()
		{
			var service = TestUtility.ParseTestApi("service TestApi { enum MyEnum { X } data One { x: MyEnum; } }");

			var dto = service.Dtos.Single();
			var field = dto.Fields.Single();
			field.Name.ShouldBe("x");
			field.Attributes.Count.ShouldBe(0);
			field.Summary.ShouldBe("");
			var type = service.GetFieldType(field);
			type.Kind.ShouldBe(ServiceTypeKind.Enum);
			type.Dto.ShouldBeNull();
			type.Enum.Name.ShouldBe("MyEnum");
			type.ValueType.ShouldBeNull();

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"service TestApi",
				"{",
				"\tenum MyEnum",
				"\t{",
				"\t\tX,",
				"\t}",
				"",
				"\tdata One",
				"\t{",
				"\t\tx: MyEnum;",
				"\t}",
				"}",
				"",
			});
		}

		[Test]
		public void DtoField()
		{
			var service = TestUtility.ParseTestApi("service TestApi { data MyDto { x: int32; } data One { x: MyDto; } }");

			var dto = service.Dtos.First(x => x.Name == "One");
			var field = dto.Fields.Single();
			field.Name.ShouldBe("x");
			field.Attributes.Count.ShouldBe(0);
			field.Summary.ShouldBe("");
			var type = service.GetFieldType(field);
			type.Kind.ShouldBe(ServiceTypeKind.Dto);
			type.Dto.Name.ShouldBe("MyDto");
			type.Enum.ShouldBeNull();
			type.ValueType.ShouldBeNull();

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"service TestApi",
				"{",
				"\tdata MyDto",
				"\t{",
				"\t\tx: int32;",
				"\t}",
				"",
				"\tdata One",
				"\t{",
				"\t\tx: MyDto;",
				"\t}",
				"}",
				"",
			});
		}

		[Test]
		public void RecursiveDtoField()
		{
			var service = TestUtility.ParseTestApi("service TestApi { data MyDto { x: MyDto; } }");

			var dto = service.Dtos.Single();
			var field = dto.Fields.Single();
			field.Name.ShouldBe("x");
			field.Attributes.Count.ShouldBe(0);
			field.Summary.ShouldBe("");
			var type = service.GetFieldType(field);
			type.Kind.ShouldBe(ServiceTypeKind.Dto);
			type.Dto.Name.ShouldBe("MyDto");
			type.Enum.ShouldBeNull();
			type.ValueType.ShouldBeNull();

			TestUtility.GenerateFsd(service).ShouldBe(new[]
			{
				"// DO NOT EDIT: generated by TestUtility",
				"",
				"service TestApi",
				"{",
				"\tdata MyDto",
				"\t{",
				"\t\tx: MyDto;",
				"\t}",
				"}",
				"",
			});
		}

		[Test]
		public void TwoFieldsSameName()
		{
			TestUtility.ParseInvalidTestApi("service TestApi { data One { x: int32; x: int64;} }")
				.Message.ShouldBe("TestApi.fsd(1,40): Duplicate field: x");
		}

		[Test]
		public void InvalidFieldType()
		{
			TestUtility.ParseInvalidTestApi("service TestApi { data One { x: X; } }")
				.Message.ShouldBe("TestApi.fsd(1,30): Unknown field type 'X'.");
		}

		[Test]
		public void ResultOfDto()
		{
			var service = TestUtility.ParseTestApi("service TestApi { data One { x: result<One>; } }");

			var dto = service.Dtos.Single();
			var field = dto.Fields.Single();
			field.Name.ShouldBe("x");
			field.Attributes.Count.ShouldBe(0);
			field.Summary.ShouldBe("");
			var type = service.GetFieldType(field);
			type.Kind.ShouldBe(ServiceTypeKind.Result);
			type.ValueType.Kind.ShouldBe(ServiceTypeKind.Dto);
			type.ValueType.Dto.Name.ShouldBe("One");
		}

		[Test]
		public void ResultOfEnumInvalid()
		{
			TestUtility.ParseInvalidTestApi("service TestApi { enum Xs { x, xx }; data One { x: result<Xs>; } }");
		}

		[TestCase("string", ServiceTypeKind.String)]
		[TestCase("boolean", ServiceTypeKind.Boolean)]
		[TestCase("double", ServiceTypeKind.Double)]
		[TestCase("int32", ServiceTypeKind.Int32)]
		[TestCase("int64", ServiceTypeKind.Int64)]
		[TestCase("decimal", ServiceTypeKind.Decimal)]
		[TestCase("bytes", ServiceTypeKind.Bytes)]
		[TestCase("object", ServiceTypeKind.Object)]
		[TestCase("error", ServiceTypeKind.Error)]
		[TestCase("Dto", ServiceTypeKind.Dto)]
		[TestCase("Enum", ServiceTypeKind.Enum)]
		[TestCase("result<Dto>", ServiceTypeKind.Result)]
		[TestCase("int32[]", ServiceTypeKind.Array)]
		[TestCase("map<int32>", ServiceTypeKind.Map)]
		public void ArrayOfAnything(string name, ServiceTypeKind kind)
		{
			var service = TestUtility.ParseTestApi("service TestApi { enum Enum { x, y } data Dto { x: xyzzy[]; } }".Replace("xyzzy", name));

			var dto = service.Dtos.Single();
			var field = dto.Fields.Single();
			field.Name.ShouldBe("x");
			field.Attributes.Count.ShouldBe(0);
			field.Summary.ShouldBe("");
			var type = service.GetFieldType(field);
			type.Kind.ShouldBe(ServiceTypeKind.Array);
			type.ValueType.Kind.ShouldBe(kind);
		}

		[TestCase("string", ServiceTypeKind.String)]
		[TestCase("boolean", ServiceTypeKind.Boolean)]
		[TestCase("double", ServiceTypeKind.Double)]
		[TestCase("int32", ServiceTypeKind.Int32)]
		[TestCase("int64", ServiceTypeKind.Int64)]
		[TestCase("decimal", ServiceTypeKind.Decimal)]
		[TestCase("bytes", ServiceTypeKind.Bytes)]
		[TestCase("object", ServiceTypeKind.Object)]
		[TestCase("error", ServiceTypeKind.Error)]
		[TestCase("Dto", ServiceTypeKind.Dto)]
		[TestCase("Enum", ServiceTypeKind.Enum)]
		[TestCase("result<Dto>", ServiceTypeKind.Result)]
		[TestCase("int32[]", ServiceTypeKind.Array)]
		[TestCase("map<int32>", ServiceTypeKind.Map)]
		public void MapOfAnything(string name, ServiceTypeKind kind)
		{
			var service = TestUtility.ParseTestApi("service TestApi { enum Enum { x, y } data Dto { x: map<xyzzy>; } }".Replace("xyzzy", name));

			var dto = service.Dtos.Single();
			var field = dto.Fields.Single();
			field.Name.ShouldBe("x");
			field.Attributes.Count.ShouldBe(0);
			field.Summary.ShouldBe("");
			var type = service.GetFieldType(field);
			type.Kind.ShouldBe(ServiceTypeKind.Map);
			type.ValueType.Kind.ShouldBe(kind);
		}

		[TestCase("string", ServiceTypeKind.String)]
		[TestCase("boolean", ServiceTypeKind.Boolean)]
		[TestCase("double", ServiceTypeKind.Double)]
		[TestCase("int32", ServiceTypeKind.Int32)]
		[TestCase("int64", ServiceTypeKind.Int64)]
		[TestCase("decimal", ServiceTypeKind.Decimal)]
		[TestCase("bytes", ServiceTypeKind.Bytes)]
		[TestCase("object", ServiceTypeKind.Object)]
		[TestCase("error", ServiceTypeKind.Error)]
		[TestCase("Dto", ServiceTypeKind.Dto)]
		[TestCase("Enum", ServiceTypeKind.Enum)]
		[TestCase("result<Dto>", ServiceTypeKind.Result)]
		[TestCase("int32[]", ServiceTypeKind.Array)]
		[TestCase("map<int32>", ServiceTypeKind.Map)]
		public void ResultOfAnything(string name, ServiceTypeKind kind)
		{
			var service = TestUtility.ParseTestApi("service TestApi { enum Enum { x, y } data Dto { x: result<xyzzy>; } }".Replace("xyzzy", name));

			var dto = service.Dtos.Single();
			var field = dto.Fields.Single();
			field.Name.ShouldBe("x");
			field.Attributes.Count.ShouldBe(0);
			field.Summary.ShouldBe("");
			var type = service.GetFieldType(field);
			type.Kind.ShouldBe(ServiceTypeKind.Result);
			type.ValueType.Kind.ShouldBe(kind);
		}
	}
}
