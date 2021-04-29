using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupaBombastik.TerrainGeneration
{
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField]
        private int VoxelWidth = 1;

        [SerializeField]
        private int TerrainWidth = 10;
        [SerializeField]
        private int TerrainHeight = 10;

        [SerializeField]
        private int Floor = 10;
        [SerializeField]
        private int Ceil = 9;

        // Mesh
        private List<Vector3> Verticies = new List<Vector3>();
        private List<int> Triangles = new List<int>();

        [SerializeField]
        private MeshFilter MeshFilter;

        private Point[,,] Points;

        [SerializeField]
        private GameObject PointPrefab;
        [SerializeField]
        private bool ShowPoints;

        void Start()
        {
            MeshFilter = GetComponent<MeshFilter>();
            GenerateMesh();
        }

		private void GenerateMesh()
        {
            GeneratePoints();
            GenerateMeshData();
            BuildMesh();
        }

		private void GeneratePoints()
		{
            Points = new Point[TerrainWidth + 1, TerrainHeight + 1, TerrainWidth + 1];

            for (int x = 0; x < TerrainWidth + 1; x++)
			{
                for (int y = 0; y < TerrainHeight + 1; y++)
                {
                    for (int z = 0; z < TerrainWidth + 1; z++)
                    {
                        Vector3 pointPosition = new Vector3(x * VoxelWidth, y * VoxelWidth, z * VoxelWidth);

                        float surface = Mathf.PerlinNoise((float)x / TerrainWidth * 5f, (float)z / TerrainWidth * 5f) * Ceil;
                        float distanceFromSurface = (float)(y * VoxelWidth) - surface;

                        Debug.Log(surface);

                        if (ShowPoints)
						{
                            GameObject point = Instantiate(
                                PointPrefab,
                                pointPosition,
                                Quaternion.identity
                            );
                            var cubeRenderer = point.GetComponent<Renderer>();
                            float shade = 0;
                            if (distanceFromSurface <= 0) shade = 1;
                            cubeRenderer.material.SetColor("_Color", new Color(shade, shade, shade));
                        }

						Points[x, y, z] = new Point(pointPosition, distanceFromSurface);
                    }
                }
            }
		}

        private void GenerateMeshData()
        {
            ClearMeshData();
            for (int x = 0; x < TerrainWidth; x++)
            {
                for (int y = 0; y < TerrainHeight; y++)
                {
                    for (int z = 0; z < TerrainWidth; z++)
                    {
                        Vector3 cubePosition = new Vector3(x * VoxelWidth, y * VoxelWidth, z * VoxelWidth);
                        CubeStep(cubePosition);
                    }
                }
            }
        }

        private void CubeStep(Vector3 position)
		{
            int configIndex = GetConfigIndex(position);

            if (configIndex == 0 || configIndex == 255)
                return;

            int edgeIndex = 0;
            for (int i = 0; i < 5; i++)
			{
                for (int j = 0; j < 3; j++)
				{
                    int cubeEdgeIndex = MarchingCubes.TriangleTable[configIndex, edgeIndex];

                    if (cubeEdgeIndex == -1)
                        return;

                    Vector3 vert = getInterpolatedVert(position, cubeEdgeIndex);

                    Verticies.Add(vert);
                    Triangles.Add(Verticies.Count - 1);

                    edgeIndex++;
				}
			}
		}

		private Vector3 getInterpolatedVert(Vector3 position, int cubeEdgeIndex)
		{
            Vector3 vert1 = position + MarchingCubes.EdgeTable[cubeEdgeIndex, 0];
            Vector3 vert2 = position + MarchingCubes.EdgeTable[cubeEdgeIndex, 1];

            Point point1 = getPointOnPosition(vert1);
            Point point2 = getPointOnPosition(vert2);

            float t = (0 - point1.DistanceFromSurface) / (point2.DistanceFromSurface - point1.DistanceFromSurface);
            return vert1 + t * (vert2 - vert1);
        }

        private int GetConfigIndex(Vector3 position)
        {
            int index = 0;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = position + MarchingCubes.CubeCorners[i];
                Point point = getPointOnPosition(corner);
                if (point.DistanceFromSurface > 0)
                {
                    index |= 1 << i;
                }
            }
            return index;
        }

        private Point getPointOnPosition(Vector3 position)
		{
            return Points[(int)position.x, (int)position.y, (int)position.z];
		}

        private void BuildMesh()
		{
            Mesh mesh = new Mesh();
            mesh.vertices = Verticies.ToArray();
            mesh.triangles = Triangles.ToArray();
            mesh.RecalculateNormals();
            MeshFilter.mesh = mesh;
		}

        private void ClearMeshData()
		{
            Verticies.Clear();
            Triangles.Clear();
		}
    }
}

