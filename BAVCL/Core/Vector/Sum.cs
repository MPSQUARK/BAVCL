using System;

namespace BAVCL;



public partial class Vector

{

    public override float Sum()

    {

        ReadOnlySpan<float> data = RetrieveReadOnlySpan();

        int vectorSize = System.Numerics.Vector<float>.Count;

        int i = 0;



        System.Numerics.Vector<float> sumVector = System.Numerics.Vector<float>.Zero;



        if (data.Length >= 10_000)

        {

            System.Numerics.Vector<float> c = System.Numerics.Vector<float>.Zero;

            for (; i <= data.Length - vectorSize; i += vectorSize)

            {

                System.Numerics.Vector<float> input = new(data.Slice(i, vectorSize));

                System.Numerics.Vector<float> y = input - c;

                System.Numerics.Vector<float> t = sumVector + y;

                c = (t - sumVector) - y;

                sumVector = t;

            }

        }

        else

        {

            for (; i <= data.Length - vectorSize; i += vectorSize)

            {

                System.Numerics.Vector<float> vector = new(data.Slice(i, vectorSize));

                sumVector = System.Numerics.Vector.Add(sumVector, vector);

            }

        }



        float result = 0;

        for (int j = 0; j < vectorSize; j++)

            result += sumVector[j];



        for (; i < data.Length; i++)

            result += data[i];



        return result;

    }

}

