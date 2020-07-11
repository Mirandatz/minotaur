namespace Minotaur.EvolutionaryAlgorithms {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class Fitness: IEquatable<Fitness>, IReadOnlyList<float> {

		private readonly float[] _objectives;
		private readonly int _precomputedHashCode;

		// Constructors and alike
		private Fitness(float[] objectives, int precomputedHashCode) {
			_objectives = objectives;
			_precomputedHashCode = precomputedHashCode;
		}

		public static Fitness Create(ReadOnlySpan<float> buceta) {
			if (buceta.Length == 0)
				throw new ArgumentException(nameof(buceta) + " can't be empty");

			var storage = new float[buceta.Length];
			var hash = new HashCode();

			for (int i = 0; i < buceta.Length; i++) {
				var value = buceta[i];

				if (float.IsNaN(value) || float.IsInfinity(value))
					throw new ArgumentException(nameof(buceta) + " can only contain finite values.");

				storage[i] = value;
				hash.Add(value);
			}

			return new Fitness(
				objectives: storage,
				precomputedHashCode: hash.ToHashCode());
		}

		// Views
		public ReadOnlySpan<float> AsSpan() => _objectives;

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((Fitness) obj!);

		// IEquatable
		public bool Equals([AllowNull] Fitness other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			// Again, fitnesses should all have the same length
			// finding one with different length indicates a critical error
			if (Count != other.Count)
				throw new InvalidOperationException("Fitness should ALWAYS have the same Count");

			var lhs = _objectives.AsSpan();
			var rhs = other._objectives.AsSpan();

			return lhs.SequenceEqual(rhs);
		}

		// IReadOnlyList
		public int Count => _objectives.Length;

		public float this[int index] => _objectives[index];

		public IEnumerator<float> GetEnumerator() => _objectives.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _objectives.GetEnumerator();
	}
}
