using System;
using System.Collections.Generic;
using UnityEngine;

namespace AO.Core.Utils.ColliderOptimizer
{
	public static class ShapeOptimizationHelper
	{
		// c# implementation of the Ramer-Douglas-Peucker-Algorithm by Craig Selbert slightly adapted for Unity Vector Types
		//http://www.codeproject.com/Articles/18936/A-Csharp-Implementation-of-Douglas-Peucker-Line-Ap
		public static List<Vector2> DouglasPeuckerReduction
		(List<Vector2> Points, double Tolerance)
		{
			if (Points == null || Points.Count < 3)
				return Points;
	
	        int firstPoint = 0;
			int lastPoint = Points.Count - 1;
	        List<int> pointIndexsToKeep = new List<int>
	        {
	
	            //Add the first and last index to the keepers
	            firstPoint,
	            lastPoint
	        };
	
	        //The first and the last point cannot be the same
	        while (Points[firstPoint].Equals(Points[lastPoint]))
			{
				lastPoint--;
			}
	
			DouglasPeuckerReductionRecursive(Points, firstPoint, lastPoint,
				Tolerance, ref pointIndexsToKeep);
	
			List<Vector2> returnPoints = new List<Vector2>();
			pointIndexsToKeep.Sort();
			foreach (int index in pointIndexsToKeep)
			{
				returnPoints.Add(Points[index]);
			}
	
			return returnPoints;
		}
	
		private static void DouglasPeuckerReductionRecursive(List<Vector2>
			points, int firstPoint, int lastPoint, double tolerance,
			ref List<int> pointIndexsToKeep)
		{
			double maxDistance = 0;
			int indexFarthest = 0;
	
			for (int index = firstPoint; index < lastPoint; index++)
			{
				double distance = (double)PerpendicularDistance
					(points[firstPoint], points[lastPoint], points[index]);
				if (distance > maxDistance)
				{
					maxDistance = distance;
					indexFarthest = index;
				}
			}
	
			if (maxDistance > tolerance && indexFarthest != 0)
			{
				//Add the largest point that exceeds the tolerance
				pointIndexsToKeep.Add(indexFarthest);
	
				DouglasPeuckerReductionRecursive(points, firstPoint,
					indexFarthest, tolerance, ref pointIndexsToKeep);
				DouglasPeuckerReductionRecursive(points, indexFarthest,
					lastPoint, tolerance, ref pointIndexsToKeep);
			}
		}
	
		public static double PerpendicularDistance
		(Vector2 Point1, Vector2 Point2, Vector2 Point)
		{
			double area = Math.Abs(.5f * (Point1.x * Point2.y + Point2.x *
				Point.y + Point.x * Point1.y - Point2.x * Point1.y - Point.x *
				Point2.y - Point1.x * Point.y));
			double bottom = Math.Sqrt(Mathf.Pow(Point1.x - Point2.x, 2f) +
				Math.Pow(Point1.y - Point2.y, 2f));
			double height = area / bottom * 2f;
	
			return height;
	
		}
	}
}
