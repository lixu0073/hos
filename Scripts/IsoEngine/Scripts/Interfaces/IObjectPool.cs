/// <summary>
/// Interface for all objects with object pool functionality.
/// </summary>
/// <typeparam name="PooledObject"></typeparam>
public interface IObjectPool <PooledObject>
{
	/// <summary>
	/// Get the free object from pool.
	/// </summary>
	/// <returns>Object from pool</returns>
	PooledObject GetObject();

	/// <summary>
	/// Return the object to the pool.
	/// </summary>
	/// <param name="pooledObject">Object from pool</param>
	void ReleaseObject(PooledObject pooledObject);
}
