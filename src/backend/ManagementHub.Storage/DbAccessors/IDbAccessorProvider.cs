namespace ManagementHub.Storage.DbAccessors;

public interface IDbAccessorProvider
{
	/// <summary>
	/// Gets an instance of the accessor for the specified <typeparamref name="TId"/>.
	/// </summary>
	/// <typeparam name="TId">Type of the object identifier.</typeparam>
	/// <returns>A datbase accessor.</returns>
	IDbAccessor<TId> GetDbAccessor<TId>();
}
