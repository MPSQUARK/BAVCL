using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	public T[] GetRowAsArray(int row) => Values[(row * Columns)..(++row * Columns)];
	public T[] GetSyncedRowAsArray(int row) => SyncCPUSelf().GetRowAsArray(row);

	public static T[] GetRowAsArray(Vec<T> vector, int row) => vector.GetRowAsArray(row);
	public static T[] GetSyncedRowAsArray(Vec<T> vector, int row) => vector.GetSyncedRowAsArray(row);


	public Vec<T> GetRowAsVector(int row) => new(Gpu, GetRowAsArray(row));
	public Vec<T> GetSyncedRowAsVector(int row) => SyncCPUSelf().GetRowAsVector(row);

	public static Vec<T> GetRowAsVector(Vec<T> vector, int row) => vector.GetRowAsVector(row);
	public static Vec<T> GetSyncedRowAsVector(Vec<T> vector, int row) => vector.GetSyncedRowAsVector(row);

}
