﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Json.More;

/// <summary>
/// Enum to string converter.
/// </summary>
/// <typeparam name="T">The supported enum.</typeparam>
/// <remarks>
///	This serializer supports the <see cref="DescriptionAttribute"/> to indicate custom value naming.
///
/// The <see cref="FlagsAttribute"/> is supported via serializing to an array of base values.  Inclusion
/// of composite values is not supported.
/// </remarks>
/// <example>
/// The attribute can be applied to both the enum type itself:
/// ```c#
/// [JsonConverter(typeof(EnumStringConverter<MyEnum>))]
/// public enum MyEnum {
///     Foo,
///     Bar
/// }
/// ```
/// 
/// or to a property of the enum type:
///
/// ```c#
/// public class MyClass {
///	    [JsonConverter(typeof(EnumStringConverter<MyEnum>))]
///	    public MyEnum Value { get; set; }
/// }
/// ```
/// </example>
public class EnumStringConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T> : WeaklyTypedJsonConverter<T>
	where T : Enum
{
	private static Dictionary<string, T>? _readValues;
	private static Dictionary<T, string>? _writeValues;
	private static Func<T, T, T>? _aggregator;
	// ReSharper disable once StaticMemberInGenericType
	private static readonly object _lock = new();

	private static Dictionary<string, T> ReadValues
	{
		get
		{
			EnsureMap();
			return _readValues!;
		}
	}

	private static Dictionary<T, string> WriteValues
	{
		get
		{
			EnsureMap();
			return _writeValues!;
		}
	}

	private static Func<T, T, T> Aggregator => _aggregator ??= BuildAggregator();

	/// <summary>Reads and converts the JSON to type <typeparamref name="T" />.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	/// <exception cref="JsonException">Element was not a string or could not identify the JSON value as a known enum value.</exception>
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		EnsureMap();

		string str;
		if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.PropertyName))
		{
			if (typeToConvert.GetCustomAttribute<FlagsAttribute>() != null && reader.TokenType == JsonTokenType.StartArray)
			{
				reader.Read(); // waste the start array
				var values = new List<T>();
				while (reader.TokenType != JsonTokenType.EndArray)
				{
					str = reader.GetString()!;

					if (!ReadValues.TryGetValue(str, out var immediate))
						throw new JsonException($"Could not find appropriate value for {str} in type {typeToConvert.Name}");

					values.Add(immediate);
					reader.Read();
				}

				return values.Aggregate(Aggregator);
			}
			throw new JsonException("Expected string");
		}

		str = reader.GetString()!;

		return ReadValues.TryGetValue(str, out var value)
			? value
			: throw new JsonException($"Could not find appropriate value for {str} in type {typeToConvert.Name}");
	}

	/// <summary>Reads a dictionary key from a JSON property name.</summary>
	/// <param name="reader">The <see cref="T:System.Text.Json.Utf8JsonReader" /> to read from.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">The options to use when reading the value.</param>
	/// <returns>The value that was converted.</returns>
	public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Read(ref reader, typeToConvert, options);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		EnsureMap();

		if (typeof(T).GetCustomAttribute<FlagsAttribute>() != null && !WriteValues.ContainsKey(value))
		{
			writer.WriteStartArray();
			foreach (var name in WriteValues.Keys)
			{
				if (value.HasFlag(name) && !name.Equals(default(T)))
					writer.WriteStringValue(WriteValues[name]);
			}
			writer.WriteEndArray();
			return;
		}

		writer.WriteStringValue(WriteValues[value]);
	}

	/// <summary>Writes a dictionary key as a JSON property name.</summary>
	/// <param name="writer">The <see cref="T:System.Text.Json.Utf8JsonWriter" /> to write to.</param>
	/// <param name="value">The value to convert. The value of <see cref="P:System.Text.Json.Serialization.JsonConverter`1.HandleNull" /> determines if the converter handles <see langword="null" /> values.</param>
	/// <param name="options">The options to use when writing the value.</param>
	public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
        EnsureMap();
        
        writer.WritePropertyName(WriteValues[value]);
	}

	private static Func<T, T, T> BuildAggregator()
	{
		var underlyingType = Enum.GetUnderlyingType(typeof(T));
		var currentParameter = Expression.Parameter(typeof(T), "current");
		var nextParameter = Expression.Parameter(typeof(T), "next");

		return Expression.Lambda<Func<T, T, T>>(
			Expression.Convert(
				Expression.Or(
					Expression.Convert(currentParameter, underlyingType),
					Expression.Convert(nextParameter, underlyingType)
				),
				typeof(T)
			),
			currentParameter,
			nextParameter
		).Compile();
	}

	private static void EnsureMap()
	{
		if (_readValues != null && _writeValues != null) return;
		lock (_lock)
		{
			if (_readValues != null && _writeValues != null) return;

			var fields = typeof(T).GetFields();

			var readValues = new Dictionary<string, T>(capacity: fields.Length);
			var writeValues = new Dictionary<T, string>(capacity: fields.Length);

			foreach (var field in fields.Where(f => !f.IsSpecialName))
			{
				var value = (T) Enum.Parse(typeof(T), field.Name);
				var description = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name;

				if (!readValues.ContainsKey(description))
					readValues.Add(description, value);
				if (!writeValues.ContainsKey(value))
					writeValues.Add(value, description);
			}

			_readValues = readValues;
			_writeValues = writeValues;
		}
	}
}
