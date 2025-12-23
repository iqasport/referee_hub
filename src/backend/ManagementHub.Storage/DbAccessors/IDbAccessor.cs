using System.Linq;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Storage.DbAccessors;

public interface IDbAccessor<TIdentifier>
{
	/// <summary>
	/// Selects the database rows by mapping the identifier from the domain to db objects.
	/// </summary>
	/// <param name="identifier">Object identifier.</param>
	/// <returns>A singleton set with the object identified by the <paramref name="identifier"/>.</returns>
	IQueryable<IIdentifiable> SelectWithId(TIdentifier identifier);
}
