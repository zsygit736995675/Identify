using UnityEngine;
using System.Collections;

namespace GestureRecognizer {
	public class MultiStrokePoint {

		public Vector2 Point { get; private set; }
		public int StrokeID { get; private set; }


		public MultiStrokePoint(Vector2 point, int strokeID) {
			this.Point = point;
			this.StrokeID = strokeID;
		}

		public MultiStrokePoint(float x, float y, int strokeID) {
			this.Point = new Vector2(x, y);
			this.StrokeID = strokeID;
		}

		public override string ToString() {
			return this.StrokeID + "; " + "(" + this.Point.x + ", " + this.Point.y + ")";
		}
	} 
}
