namespace Minotaur.Math {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;

	public static class Distance {

		public static double Euclidean(Array<float> lhs, Array<float> rhs) {
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

			return Math.Sqrt(distance);
		}

		public static Matrix<double> ComputeEuclideanDistanceMatrix(Matrix<float> datasetInstaces) {
			var instanceCount = datasetInstaces.RowCount;
			var distances = new MutableMatrix<double>(instanceCount, instanceCount);

			Parallel.For(0, instanceCount, i => {
				var lhs = datasetInstaces.GetRow(i);
				for (int j = 0; j < instanceCount; j++) {
					var rhs = datasetInstaces.GetRow(j);
					var distance = Euclidean(lhs, rhs);
					distances.Set(i, j, distance);
				}
			});

			return distances.ToMatrix();
		}
	}
}
