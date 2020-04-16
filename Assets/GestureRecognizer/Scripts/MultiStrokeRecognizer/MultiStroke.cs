using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GestureRecognizer {

	public class MultiStroke {

		/// <summary>
		/// Name of the multi stroke. It acts like an ID for this multi stroke,
		/// so you should give your multi strokes unique names.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Points that form this multi stroke. 
		/// </summary>
		public MultiStrokePoint[] Points { get; set; }

		/// <summary>
		/// This gesture will be resampled to have this much of points. 
		/// Best between 32 and 256
		/// </summary>
		int NUMBER_OF_POINTS { get { return 32; } }


		public MultiStroke(MultiStrokePoint[] points, string name = "") {
			this.Name = name;
			this.Points = points;
			this.Scale();
			this.TranslateToCenter();
			this.Resample();
		}


		public Result Recognize(MultiStrokeLibrary multiStrokeLibrary) {

			Result result = new Result();
			result.Score = float.MaxValue;
			result.Name = "";

			foreach (MultiStroke multiStroke in multiStrokeLibrary.Library) {
				float distance = GreedyCloudMatch(this.Points, multiStroke.Points);

				if (distance < result.Score) {
					result.Score = distance;
					result.Name = multiStroke.Name;
				}

			}

			return result;
		}

		/// <summary>
		/// Scale the multi stroke so that it can fit into predefined bounding box 
		/// </summary>
		public void Scale() {
			
			float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
			for (int i = 0; i < this.Points.Length; i++) {
				if (minx > this.Points[i].Point.x) minx = this.Points[i].Point.x;
				if (miny > this.Points[i].Point.y) miny = this.Points[i].Point.y;
				if (maxx < this.Points[i].Point.x) maxx = this.Points[i].Point.x;
				if (maxy < this.Points[i].Point.y) maxy = this.Points[i].Point.y;
			}

			MultiStrokePoint[] scaledPoints = new MultiStrokePoint[this.Points.Length];
			float scale = Math.Max(maxx - minx, maxy - miny);

			for (int i = 0; i < this.Points.Length; i++) {
				scaledPoints[i] = new MultiStrokePoint((this.Points[i].Point.x - minx) / scale, (this.Points[i].Point.y - miny) / scale, this.Points[i].StrokeID);
			}

			this.Points = scaledPoints;
		}

		/// <summary>
		/// Move the multi stroke to the center
		/// </summary>
		/// <param name="point">Points to move</param>
		/// <returns>List of moved points</returns>
		public void TranslateToCenter() {
			
			Vector2 p = this.GetCenter();
			MultiStrokePoint[] translatedPoints = new MultiStrokePoint[this.Points.Length];

			for (int i = 0; i < this.Points.Length; i++) {
				translatedPoints[i] = new MultiStrokePoint(this.Points[i].Point.x - p.x, this.Points[i].Point.y - p.y, this.Points[i].StrokeID);
			}
			
			this.Points = translatedPoints;
		}


		/// <summary>
		/// Resample the point list so that the list has NUMBER_OF_POINTS number of points
		/// and points are equidistant to each other.
		/// 
		/// First calculate the length of the path. Divided it by (numberOfPoints - 1)
		/// to find the increment. Step through the path, and if the distance covered is
		/// equal to or greater than the increment add a new point to the list by lineer
		/// interpolation.
		/// </summary>
		public void Resample() {

			MultiStrokePoint[] resampledPoints = new MultiStrokePoint[NUMBER_OF_POINTS];
			resampledPoints[0] = new MultiStrokePoint(this.Points[0].Point, this.Points[0].StrokeID);
			int n = 1;

			float increment = GetPathLength() / (NUMBER_OF_POINTS - 1);
			float distanceCovered = 0;

			for (int i = 1; i < this.Points.Length; i++) {

				if (this.Points[i].StrokeID == this.Points[i - 1].StrokeID) {
					float distance = Vector2.Distance(this.Points[i - 1].Point, this.Points[i].Point);

					if (distanceCovered + distance >= increment) {

						MultiStrokePoint firstPoint = this.Points[i - 1];

						while (distanceCovered + distance >= increment) {

							float t = Mathf.Min(Mathf.Max((increment - distanceCovered) / distance, 0.0f), 1.0f);

							if (float.IsNaN(t)) t = 0.5f;

							resampledPoints[n++] = new MultiStrokePoint(
								(1.0f - t) * firstPoint.Point.x + t * this.Points[i].Point.x,
								(1.0f - t) * firstPoint.Point.y + t * this.Points[i].Point.y,
								this.Points[i].StrokeID
							);

							distance = distanceCovered + distance - increment;
							distanceCovered = 0;
							firstPoint = resampledPoints[n - 1];
						}

						distanceCovered = distance;

					} else distanceCovered += distance;
				}
			}

			if (n == NUMBER_OF_POINTS - 1) {
				resampledPoints[n++] = new MultiStrokePoint(
					this.Points[this.Points.Length - 1].Point.x,
					this.Points[this.Points.Length - 1].Point.y,
					this.Points[this.Points.Length - 1].StrokeID
				);
			}

			this.Points = resampledPoints;
		}


		/// <summary>
		/// Calculate the center of the points
		/// </summary>
		/// <param name="points">List of points</param>
		/// <returns></returns>
		public Vector2 GetCenter() {

			Vector2 total = Vector2.zero;

			for (int i = 0; i < this.Points.Length; i++) {
				total += this.Points[i].Point;
			}
			return new Vector2(total.x / this.Points.Length, total.y / this.Points.Length);
		}


		/// <summary>
		/// Calculate total path length: sum of distance between each points
		/// </summary>
		/// <param name="points">List of points</param>
		/// <returns></returns>
		public float GetPathLength() {

			float length = 0;
			
			for (int i = 1; i < this.Points.Length; i++) {
				if (this.Points[i].StrokeID == this.Points[i - 1].StrokeID) {
					length += Vector2.Distance(this.Points[i - 1].Point, this.Points[i].Point);
				}
			}

			return length;
		}


		private static float GreedyCloudMatch(MultiStrokePoint[] points1, MultiStrokePoint[] points2) {
			float e = 0.5f;
			int step = Mathf.FloorToInt(Mathf.Pow(points1.Length, 1.0f - e));
			float minDistance = float.MaxValue;

			for (int i = 0; i < points1.Length; i += step) {
				float distance1 = CloudDistance(points1, points2, i);
				float distance2 = CloudDistance(points2, points1, i);
				minDistance = Mathf.Min(minDistance, Mathf.Min(distance1, distance2));
			}
			return minDistance;
		}


		private static float CloudDistance(MultiStrokePoint[] points1, MultiStrokePoint[] points2, int startIndex) {
			bool[] matched = new bool[points1.Length];
			Array.Clear(matched, 0, points1.Length);

			float sum = 0;
			int i = startIndex;

			do {
				int index = -1;
				float minDistance = float.MaxValue;

				for (int j = 0; j < points1.Length; j++) {
					if (!matched[j]) {
						float distance = Vector2.Distance(points1[i].Point, points2[j].Point);
						if (distance < minDistance) {
							minDistance = distance;
							index = j;
						}
					}
				}

				matched[index] = true;
				float weight = 1.0f - ((i - startIndex + points1.Length) % points1.Length) / (1.0f * points1.Length);
				sum += weight * minDistance;
				i = (i + 1) % points1.Length;

			} while (i != startIndex);

			return sum;
		}


		public override string ToString() {
			string result = this.Name;

			for (int i = 0; i < this.Points.Length; i++) {
				result += "\n" + this.Points[i];
			}

			return result;
		}
	}
}
