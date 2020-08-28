﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[JsonConverter(typeof(ExamplesKeywordJsonConverter))]
	public class ExamplesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "examples";

		public IReadOnlyList<JsonElement> Values { get; }

		public ExamplesKeyword(params JsonElement[] values)
		{
			Values = values.Select(e => e.Clone()).ToList();
		}

		public ExamplesKeyword(IEnumerable<JsonElement> values)
		{
			Values = values.Select(e => e.Clone()).ToList();
		}

		public void Validate(ValidationContext context)
		{
			context.SetAnnotation(Name, Values);
			context.IsValid = true;
		}
	}

	public class ExamplesKeywordJsonConverter : JsonConverter<ExamplesKeyword>
	{
		public override ExamplesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var document = JsonDocument.ParseValue(ref reader);

			if (document.RootElement.ValueKind != JsonValueKind.Array)
				throw new JsonException("Expected array");

			return new ExamplesKeyword(document.RootElement.EnumerateArray());
		}
		public override void Write(Utf8JsonWriter writer, ExamplesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ExamplesKeyword.Name);
			writer.WriteStartArray();
			foreach (var element in value.Values)
			{
				writer.WriteValue(element);
			}
			writer.WriteEndArray();
		}
	}
}