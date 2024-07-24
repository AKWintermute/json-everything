using NUnit.Framework;

namespace Json.Schema.Generation.Tests;

public class ExternalReferenceTests
{
	internal class HasExternalSchema
	{
		public int Value { get; set; }
	}

	internal class ShouldRefToExternalSchema
	{
		public HasExternalSchema ShouldRef { get; set; }
	}

	[Test]
	public void GeneratesRefForExternalReference()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("ShouldRef", new JsonSchemaBuilder().Ref("https://test.json-everything.net/has-external-schema"))
			);

		var config = new SchemaGeneratorConfiguration
		{
			ExternalReferences =
			{
				[typeof(HasExternalSchema)] = new("https://test.json-everything.net/has-external-schema")
			}
		};

		JsonSchema actual = new JsonSchemaBuilder().FromType<ShouldRefToExternalSchema>(config);

		AssertionExtensions.AssertEqual(expected, actual);
	}

	internal class RefWithAttributes
	{
		[Required]
		[Title("this one has attributes")]
		public HasExternalSchema ShouldRef { get; set; }
	}


	[Test]
	public void GeneratesRefForExternalReferenceWithAttributes()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("ShouldRef", new JsonSchemaBuilder()
					.Ref("https://test.json-everything.net/has-external-schema")
					.Title("this one has attributes")
				)
			)
			.Required("ShouldRef");

		var config = new SchemaGeneratorConfiguration
		{
			ExternalReferences =
			{
				[typeof(HasExternalSchema)] = new("https://test.json-everything.net/has-external-schema")
			}
		};

		JsonSchema actual = new JsonSchemaBuilder().FromType<RefWithAttributes>(config);

		AssertionExtensions.AssertEqual(expected, actual);
	}
}