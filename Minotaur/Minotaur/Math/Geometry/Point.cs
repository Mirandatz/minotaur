namespace Minotaur.Math.Geometry {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class Point: IReadOnlyList<float> {

		private readonly float[] _coordinates;

		public Point(ReadOnlySpan<float> coordinates) {

			var storage = new float[coordinates.Length];

			for (int i = 0; i < coordinates.Length; i++) {
				var c = coordinates[i];

				if (float.IsNaN(c) || float.IsInfinity(c))
					throw new ArgumentException(nameof(coordinates) + " must contain only finite values.");

				storage[i] = c;
			}

			_coordinates = storage;
		}

		public ReadOnlySpan<float> AsSpan() => _coordinates.AsSpan();

		// Silly overrides

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();

		// IReadOnlyList

		public float this[int index] => _coordinates[index];

		public int Count => _coordinates.Length;

		public IEnumerator<float> GetEnumerator() => _coordinates.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _coordinates.GetEnumerator();
	}
}
