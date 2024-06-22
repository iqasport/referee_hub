using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User;

public record UserAttribute(string Prefix, string Key, JsonDocument AttributeValue);

public class UserAttributes
{
	public UserAttributes() { }

	public UserAttributes(IEnumerable<UserAttribute> attributes) =>
		this.AttributesByPrefix = attributes.GroupBy(att => att.Prefix).ToDictionary(g => g.Key, g => g.ToList());

	public Dictionary<string, List<UserAttribute>> AttributesByPrefix { get; } = new();

	public IReadOnlyDictionary<string, JsonDocument> RootAttributes =>
		this.AttributesByPrefix.TryGetValue(string.Empty, out var attributes)
		? attributes.ToDictionary(x => $".{x.Key}", x => x.AttributeValue)
		: new Dictionary<string, JsonDocument>();

	public IReadOnlyDictionary<string, JsonDocument> GetPrefixedByConstraint(NgbConstraint ngbConstraint) => this.AttributesByPrefix
		.Where(kvp => ngbConstraint.AppliesToAny || NgbIdentifier.TryParse(kvp.Key, out var ngbId) && ngbConstraint.AppliesTo(ngbId))
		.SelectMany(kvp => kvp.Value)
		.ToDictionary(att => $"{att.Prefix}.{att.Key}", att => att.AttributeValue);

	public IReadOnlyDictionary<string, JsonDocument> GetNonRoot() => this.AttributesByPrefix
		.Where(kvp => kvp.Key != string.Empty)
		.SelectMany(kvp => kvp.Value)
		.ToDictionary(att => $"{att.Prefix}.{att.Key}", att => att.AttributeValue);
}
