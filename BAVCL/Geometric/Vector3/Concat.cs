using System.Collections.Generic;
using System.Linq;


namespace BAVCL.Geometric
{
    public partial class Vector3
    {

        // Appening Verticies
        public static Vector3 Concat(Vector3 vectorA, Vertex vertA)
        {
            return vectorA.Copy().Concat_IP(vertA);
        }
        public static Vector3 Concat(Vector3 vectorA, Vertex[] vertices)
        {
            return vectorA.Copy().Concat_IP(vertices);
        }
        public static Vector3 Concat(Vector3 vectorA, List<Vertex> vertices)
        {
            return vectorA.Copy().Concat_IP(vertices);
        }
        public Vector3 Concat_IP(Vertex vertA)
        {
            SyncCPU();
            UpdateCache(this.Value.Append(vertA.X).Append(vertA.Y).Append(vertA.Z).ToArray());
            return this;
        }
        public Vector3 Concat_IP(Vertex[] vertices)
        {
            SyncCPU();
            for (int i = 0; i < vertices.Length; i++)
            {
                this.Value = this.Value.Append(vertices[i].X).Append(vertices[i].Y).Append(vertices[i].Z).ToArray();
            }
            UpdateCache(this.Value);
            return this;
        }
        public Vector3 Concat_IP(List<Vertex> vertices)
        {
            SyncCPU();
            for (int i = 0; i < vertices.Count; i++)
            {
                this.Value = this.Value.Append(vertices[i].X).Append(vertices[i].Y).Append(vertices[i].Z).ToArray();
            }
            UpdateCache(this.Value);
            return this;
        }


        // Appening Vector3's 
        public static Vector3 Concat(Vector3 vectorA, Vector3 vectorB)
        {
            return vectorA.Copy().Concat_IP(vectorB);
        }
        public static Vector3 Concat(Vector3 vectorA, Vector3[] vectors)
        {
            return vectorA.Copy().Concat_IP(vectors);
        }
        public static Vector3 Concat(Vector3 vectorA, List<Vector3> vectors)
        {
            return vectorA.Copy().Concat_IP(vectors);
        }
        public Vector3 Concat_IP(Vector3 vector)
        {
            SyncCPU();
            UpdateCache(this.Value.Concat(vector.Value).ToArray());
            return this;
        }
        public Vector3 Concat_IP(Vector3[] vectors)
        {
            SyncCPU();
            for (int i = 0; i < vectors.Length; i++)
            {
                this.Value.Concat(vectors[i].Value);
            }
            UpdateCache(this.Value);
            return this;
        }
        public Vector3 Concat_IP(List<Vector3> vectors)
        {
            SyncCPU();
            for (int i = 0; i < vectors.Count; i++)
            {
                this.Value.Concat(vectors[i].Value);
            }
            UpdateCache(this.Value);
            return this;
        }
    
    
    }
}
