using UnityEngine;

namespace Framework.DeferredDecalling
{
    public static class CubeBuilder
    {
        private static Mesh m_cube;
        public static Mesh Cube
        {
            get
            {
                if (m_cube == null)
                {
                    m_cube = CreateCube();
                }
                return m_cube;
            }
        }

        public static Mesh CreateCube()
        {
            Vector3[] vertices = {
                new Vector3 (-0.5f, -0.5f, -0.5f),
                new Vector3 (0.5f, -0.5f, -0.5f),
                new Vector3 (0.5f, 0.5f, -0.5f),
                new Vector3 (-0.5f, 0.5f, -0.5f),
                new Vector3 (-0.5f, 0.5f, 0.5f),
                new Vector3 (0.5f, 0.5f, 0.5f),
                new Vector3 (0.5f, -0.5f, 0.5f),
                new Vector3 (-0.5f, -0.5f, 0.5f),
            };

            int[] triangles = {
                0, 2, 1, //face front
			    0, 3, 2,
                2, 3, 4, //face top
			    2, 4, 5,
                1, 2, 5, //face right
			    1, 5, 6,
                0, 7, 4, //face left
			    0, 4, 3,
                5, 4, 7, //face back
			    5, 7, 6,
                0, 6, 7, //face bottom
			    0, 1, 6,
            };

            Mesh cube = new Mesh();
            cube.name = "Cube";
            cube.Clear();
            cube.vertices = vertices;
            cube.triangles = triangles;
            cube.bounds = new Bounds(Vector3.zero, Vector3.one);
            cube.RecalculateNormals();
            cube.UploadMeshData(true);
            return cube;
        }
    }
}
