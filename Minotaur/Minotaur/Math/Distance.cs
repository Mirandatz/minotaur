namespace Minotaur.Math {
	using System;
	using Minotaur.Collections;

	public static class Distance {

		public static double SquaredEuclidean(Array<float> lhs, Array<float> rhs) {
			if (lhs.Length != rhs.Length)
				throw new ArgumentException();
			if (lhs.Length == 0)
				throw new ArgumentException();

			double distance = 0;
			for (int i = 0; i < lhs.Length; i++) {
				var delta = lhs[i] - rhs[i];
				distance = Math.FusedMultiplyAdd(
					x: delta,
					y: delta,
					z: distance);
			}

			return distance;
		}
	}
}
