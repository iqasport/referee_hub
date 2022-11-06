using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction;

namespace Service.API.Test.WebsiteClient;

public static class ResponseSerializerExtensions
{
	private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
	{
		AllowTrailingCommas = true,
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
		NumberHandling = JsonNumberHandling.AllowReadingFromString,
		Converters =
		{
			new SingleOrArrayConverterFactory(),
			new JsonStringEnumMemberConverter(),
		}
	};

	public static async Task<T?> ReadModelAsync<T>(this HttpContent content) where T : class
	{
		var wrapper = await content.ReadFromJsonAsync<DataWrapper>(serializerOptions);

		if (wrapper == null) throw new JsonException("Could not deserialize data into the expected format.");
		if (wrapper.data == null) return null;

		var otherObjects = ParseIncluded(wrapper.included ?? new List<ObjectDescriptor>());
		var data = wrapper.data;
		List<T> objectList = ParseObjects<T>(otherObjects, data);

		return objectList.Single();
	}

	public static async Task<List<T>?> ReadModelListAsync<T>(this HttpContent content) where T : class
	{
		var wrapper = await content.ReadFromJsonAsync<DataWrapper>(serializerOptions);

		if (wrapper == null) throw new JsonException("Could not deserialize data into the expected format.");
		if (wrapper.data == null) return null;

		var otherObjects = ParseIncluded(wrapper.included ?? new List<ObjectDescriptor>());
		var data = wrapper.data;
		List<T> objectList = ParseObjects<T>(otherObjects, data);

		return objectList;
	}

	private static List<T> ParseObjects<T>(Dictionary<(string type, long id), object> otherObjects, List<ObjectDescriptor> data) where T : class
	{
		var objectList = new List<T>(data.Count);
		foreach (var descriptor in data)
		{
			var obj = ParseObject(descriptor);
			if (obj != null)
			{
				if (obj is not T tObj)
				{
					throw new JsonException($"Type mismatch. Expected {typeof(T)}, but API returned {obj.GetType()}.");
				}

				FillRelationships(tObj, descriptor, otherObjects);
				objectList.Add(tObj);
			}
		}

		return objectList;
	}

	private static object? ParseObject(ObjectDescriptor data)
	{
		var serializedAttributes = JsonSerializer.Serialize(data.attributes);
		var obj = JsonSerializer.Deserialize(serializedAttributes, data.GetDataType(), serializerOptions);
		if (obj is IIdentifiable identifiable)
		{
			identifiable.Id = data.id;
		}

		return obj;
	}


	private static Dictionary<(string type, long id), object> ParseIncluded(List<ObjectDescriptor> objectDescriptors)
	{
		var dict = new Dictionary<(string type, long id), object>();

		foreach (ObjectDescriptor? descriptor in objectDescriptors)
		{
			var obj = ParseObject(descriptor);
			if (obj != null)
			{
				dict.Add((descriptor.type, descriptor.id), obj);
			}
		}

		foreach (ObjectDescriptor? descriptor in objectDescriptors)
		{
			if (dict.TryGetValue((descriptor.type, descriptor.id), out var obj))
			{
				FillRelationships(obj, descriptor, dict);
			}
		}

		return dict;
	}

	private static void FillRelationships(object obj, ObjectDescriptor objectDescriptor, Dictionary<(string type, long id), object> otherObjects)
	{
		var type = objectDescriptor.GetDataType();
		foreach (var relationship in objectDescriptor.relationships)
		{
			var property = type.GetProperty(CapitalizeFirstLetter(relationship.Key));
			if (property == null)
			{
				continue;
			}

			var descriptors = relationship.Value.data ?? new List<ObjectDescriptor>();
			var objects = descriptors.Select(d => otherObjects[(d.type, d.id)]);

			if (property.PropertyType.Name.StartsWith(nameof(ICollection)))
			{
				var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments))!;
				foreach (var value in objects)
				{
					list.Add(value);
				}
				property.SetValue(obj, list);
			}
			else
			{
				property.SetValue(obj, objects.SingleOrDefault());
			}
		}
	}

	private static string CapitalizeFirstLetter(string word)
	{
		return new StringBuilder().Append(char.ToUpper(word[0])).Append(word.Substring(1)).ToString();
	}
	
	private class ObjectDescriptor
	{
		private static readonly Assembly modelsAssembly = Assembly.GetAssembly(typeof(ManagementHub.Models.User))!;

		public long id;
		public string type = string.Empty;
		public Dictionary<string, object> attributes = new();
		public Dictionary<string, RelationshipWrapper> relationships = new();

		public Type GetDataType()
		{
			if (type.Any(c => !char.IsLetter(c)))
			{
				throw new InvalidOperationException("Type contains illegal characters");
			}

			if (string.IsNullOrWhiteSpace(type))
			{
				throw new Exception($"Type field was empty.");
			}

			var typeName = CapitalizeFirstLetter(type);
			var fullTypeName = $"ManagementHub.Models.{typeName}";
			return modelsAssembly.GetType(fullTypeName) ?? throw new Exception($"Could not find type: {fullTypeName}");
		}
	}

	private class DataWrapper
	{
		[JsonConverter(typeof(SingleOrArrayConverter<List<ObjectDescriptor>, ObjectDescriptor>))]
		public List<ObjectDescriptor>? data;
		public List<ObjectDescriptor>? included;
	}

	private class RelationshipWrapper
	{
		[JsonConverter(typeof(SingleOrArrayConverter<List<ObjectDescriptor>, ObjectDescriptor>))]
		public List<ObjectDescriptor>? data;
	}

	/// <summary>
	/// Converter that transforms json type <c>T | [T]</c> into a list of objects.
	/// <see href="https://stackoverflow.com/a/59430729"/>
	/// </summary>
	public class SingleOrArrayConverterFactory : JsonConverterFactory
	{
		public bool CanWrite { get; }

		public SingleOrArrayConverterFactory() : this(true) { }

		public SingleOrArrayConverterFactory(bool canWrite) => CanWrite = canWrite;

		public override bool CanConvert(Type typeToConvert)
		{
			var itemType = GetItemType(typeToConvert);
			if (itemType == null)
				return false;
			if (itemType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(itemType))
				return false;
			if (typeToConvert.GetConstructor(Type.EmptyTypes) == null || typeToConvert.IsValueType)
				return false;
			return true;
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var itemType = GetItemType(typeToConvert);
			var converterType = typeof(SingleOrArrayConverter<,>).MakeGenericType(typeToConvert, itemType!);
			return (JsonConverter)Activator.CreateInstance(converterType, new object[] { CanWrite })!;
		}

		static Type? GetItemType(Type? type)
		{
			// Quick reject for performance
			if (type == null || type.IsPrimitive || type.IsArray || type == typeof(string))
				return null;
			while (type != null)
			{
				if (type.IsGenericType)
				{
					var genType = type.GetGenericTypeDefinition();
					if (genType == typeof(List<>))
						return type.GetGenericArguments()[0];
					// Add here other generic collection types as required, e.g. HashSet<> or ObservableCollection<> or etc.
				}
				type = type.BaseType;
			}
			return null;
		}
	}

	public class SingleOrArrayConverter<TCollection, TItem> : JsonConverter<TCollection> where TCollection : class, ICollection<TItem>, new()
	{
		public SingleOrArrayConverter() : this(true) { }
		public SingleOrArrayConverter(bool canWrite) => CanWrite = canWrite;

		public bool CanWrite { get; }

		public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			TCollection collection;
			TItem? item;
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.StartArray:
					collection = new TCollection();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndArray)
							break;
						item = JsonSerializer.Deserialize<TItem>(ref reader, options);
						if (item != null)
						{
							collection.Add(item);
						}
					}
					return collection;
				default:
					collection = new TCollection();
					item = JsonSerializer.Deserialize<TItem>(ref reader, options);
					if (item != null)
					{
						collection.Add(item);
					}
					return collection;
			}
		}

		public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
		{
			if (CanWrite && value.Count == 1)
			{
				JsonSerializer.Serialize(writer, value.First(), options);
			}
			else
			{
				writer.WriteStartArray();
				foreach (var item in value)
					JsonSerializer.Serialize(writer, item, options);
				writer.WriteEndArray();
			}
		}
	}
}
