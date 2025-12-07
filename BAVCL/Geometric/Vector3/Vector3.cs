using System;
using System.Linq;
using BAVCL.Core;

namespace BAVCL.Geometric
{

	public sealed partial class Vector3 : VectorBase<float>
	{
		#region "Variables"
		public override int Columns { get { return _columns; } set { _columns = 3; } }
		#endregion

		// CONSTRUCTOR
		public Vector3(GPU gpu, float[] value, bool cache = true) : base(gpu, ValidateVectorLength(value), 3, cache) { }

		public Vector3(GPU gpu, int length) : base(gpu, ValidateVectorLength(length), 3) { }

		private static float[] ValidateVectorLength(float[] values)
		{
			if (values.Length % 3 != 0) throw new Exception($"Vector3 must have a length that is a multiple of 3. Recieved {values.Length}");
			return values;
		}
		private static int ValidateVectorLength(int Length)
		{
			if (Length % 3 != 0) throw new Exception($"Vector3 must have a length that is a multiple of 3. Recieved {Length}");
			return Length;
		}

		// CONVERT TO GENERIC VECTOR
		/*
		* TODO: Conversion between the vector types can be optimised by simply passing the 
		* Memory buffer reference to the new vector, i.e. the ID if not 0
		*/
		public Vector ToVector(bool cache = true)
		{
			if (_id != 0)
			{
				return new Vector(this.Gpu, Pull(), this.Columns, cache);
			}
			return new Vector(this.Gpu, this.Value, this.Columns, cache);
		}
		public Vector ToVector(int Columns, bool cache = true)
		{
			if (_id != 0)
			{
				return new Vector(this.Gpu, Pull(), this.Columns, cache);
			}
			return new Vector(this.Gpu, this.Value, Columns, cache);
		}


		// MATHEMATICAL PROPERTIES 
		// TODO: needs rework... not suitable for vec3
		#region
		public override float Mean()
		{
			SyncCPU();
			return this.Value.Average();
		}
		public override float Range()
		{
			return Max() - Min();
		}
		public override float Sum()
		{
			SyncCPU();
			return this.Value.Sum();
		}


		#endregion




	}



}
