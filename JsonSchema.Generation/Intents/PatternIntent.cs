﻿using JetBrains.Annotations;

namespace Json.Schema.Generation.Intents
{
	public class PatternIntent : ISchemaKeywordIntent
	{
		public string Value { get; }

		public PatternIntent([RegexPattern] string value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Pattern(Value);
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = GetType().GetHashCode();
				hashCode = (hashCode * 397) ^ Value.GetHashCode();
				return hashCode;
			}
		}
	}
}