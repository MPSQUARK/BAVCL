using System;
using System.Numerics;
using SimdVector = System.Numerics.Vector;

namespace BAVCL.Core.Helpers.Numerics;

/// <summary>Scalar float64 Neumaier accumulator. See citations.md [9].</summary>
internal struct Float64NeumaierState
{
	public double Sum;
	public double Compensation;

	public static Float64NeumaierState Zero => default;

	/// <summary>One compensated add without allocating a wrapper struct.</summary>
	public static void Step(ref double sum, ref double compensation, float value)
	{
		double input = value;
		double total = sum + input;
		if (Math.Abs(sum) >= Math.Abs(input))
			compensation += (sum - total) + input;
		else
			compensation += (input - total) + sum;
		sum = total;
	}

	public void Add(float value)
	{
		double input = value;
		double total = Sum + input;
		if (Math.Abs(Sum) >= Math.Abs(input))
			Compensation += (Sum - total) + input;
		else
			Compensation += (input - total) + Sum;
		Sum = total;
	}

	public void Add(double input)
	{
		double total = Sum + input;
		if (Math.Abs(Sum) >= Math.Abs(input))
			Compensation += (Sum - total) + input;
		else
			Compensation += (input - total) + Sum;
		Sum = total;
	}

	public readonly float ToSingle() => (float)(Sum + Compensation);
}

/// <summary>
/// Four-lane float64 Neumaier accumulator (one <see cref="Vector{Double}"/> sum + compensation).
/// </summary>
internal struct SimdFloat64NeumaierState(Vector<double> sum, Vector<double> compensation)
{
	public Vector<double> Sum = sum;
	public Vector<double> Compensation = compensation;

	public static SimdFloat64NeumaierState Zero =>
		new(Vector<double>.Zero, Vector<double>.Zero);

    public void Add(Vector<double> input)
	{
		Vector<double> total = Sum + input;
		Vector<long> sumDominates = SimdVector.GreaterThanOrEqual(SimdVector.Abs(Sum), SimdVector.Abs(input));
		Vector<double> compensationUpdate = SimdVector.ConditionalSelect(
			sumDominates,
			(Sum - total) + input,
			(input - total) + Sum);
		Compensation += compensationUpdate;
		Sum = total;
	}

	public readonly void FoldInto(ref Float64NeumaierState target)
	{
		// Each lane already holds a compensated partial; fold (sum + compensation) per lane.
		for (int lane = 0; lane < Vector<double>.Count; lane++)
			target.Add(Sum[lane] + Compensation[lane]);
	}
}
