using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupaBombastik.TerrainGeneration
{
	public class Point
	{
		public Vector3 Position;
		public float DistanceFromSurface;
		public Point(Vector3 position, float distanceFromSurface)
		{
			Position = position;
			DistanceFromSurface = distanceFromSurface;
		}
	}
}
