﻿using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Generation.Tests;

public class CommentsTests
{
	/// <summary>
	/// Type commented model description
	/// </summary>
	private class TypeCommentedModel
	{
		public int Foo { get; set; }
		public TypeCommentedInnerModel Inner { get; set; }
	}

	/// <summary>
	/// Type commented inner model description
	/// </summary>
	private class TypeCommentedInnerModel
	{
		public int Bar { get; set; }
	}

	[Test]
	public void TypeCommentsAreAdded()
	{
		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<TypeCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		JsonSchema schema = new JsonSchemaBuilder().FromType<TypeCommentedModel>(config);

		Assert.Multiple(() =>
		{
			Assert.That(schema.GetDescription(), Is.EqualTo("Type commented model description"));

			Assert.That(schema.GetProperties()?["Inner"].GetDescription(), Is.EqualTo("Type commented inner model description"));
		});
	}

	private class MemberCommentedModel
	{
		/// <summary>
		/// Bar is for counting
		/// </summary>
		public int Bar { get; set; }
	}

	[Test]
	public void MemberCommentsAreAdded()
	{
		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<MemberCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		JsonSchema schema = new JsonSchemaBuilder().FromType<MemberCommentedModel>(config);

		Assert.That(schema.GetProperties()?["Bar"].GetDescription(), Is.EqualTo("Bar is for counting"));
	}

	/// <summary>
	/// Type commented model description
	/// </summary>
	private class TypeAndMemberCommentedModel
	{
		/// <summary>
		/// This overrides the type description
		/// </summary>
		public TypeCommentedInnerModel Inner { get; set; }
	}

	[Test]
	public void MemberCommentOverridesTypeComment()
	{
		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<TypeAndMemberCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		JsonSchema schema = new JsonSchemaBuilder().FromType<TypeAndMemberCommentedModel>(config);

		Assert.That(schema.GetProperties()?["Inner"].GetDescription(), Is.EqualTo("This overrides the type description"));
	}

	/// <summary>
	/// Type commented model description
	/// </summary>
	private class TypeAndSingleMemberCommentedModel
	{
		/// <summary>
		/// This overrides the type description
		/// </summary>
		public TypeCommentedInnerModel Inner { get; set; }
		public TypeCommentedInnerModel Inner2 { get; set; }
		public TypeCommentedInnerModel Inner3 { get; set; }
	}

	[Test]
	public void DefinitionContainsTypeCommentAndRefContainsMemberComment()
	{
		var config = new SchemaGeneratorConfiguration();
		config.RegisterXmlCommentFile<TypeAndSingleMemberCommentedModel>("JsonSchema.Net.Generation.Tests.xml");

		JsonSchema schema = new JsonSchemaBuilder().FromType<TypeAndSingleMemberCommentedModel>(config);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, TestEnvironment.SerializerOptions));

		Assert.Multiple(() =>
		{
			Assert.That(schema.GetProperties()?["Inner"].GetDescription(), Is.EqualTo("This overrides the type description"));
			Assert.That(schema.GetProperties()?["Inner2"].GetDescription(), Is.Null);
			Assert.That(schema.GetProperties()?["Inner3"].GetDescription(), Is.Null);
			Assert.That(schema.GetDefs()?["typeCommentedInnerModelInCommentsTests"].GetDescription(), Is.EqualTo("Type commented inner model description"));
		});
	}
}