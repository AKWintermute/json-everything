﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(MaximumKeywordJsonConverter))]
	public class MaximumKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "maximum";

		public decimal Value { get; }

		public MaximumKeyword(decimal value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Number)
			{
				context.IsValid = true;
				return;
			}

			var number = context.LocalInstance.GetDecimal();
			context.IsValid = Value >= number;
			if (!context.IsValid)
				context.Message = $"{number} is greater than or equal to {Value}";
		}
	}

	public class MaximumKeywordJsonConverter : JsonConverter<MaximumKeyword>
	{
		public override MaximumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MaximumKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MaximumKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MaximumKeyword.Name, value.Value);
		}
	}
}