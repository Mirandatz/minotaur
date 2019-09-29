namespace Minotaur.GeneticAlgorithms {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class Fitness: IEquatable<Fitness>, IReadOnlyList<float> {

		private readonly float[] _objectives;

		private readonly int _precomputedHashCode;

		private Fitness(float[] objectives) {
			if (objectives.Length == 0)
				throw new ArgumentException(nameof(objectives) + " can't be empty");

			for (int i = 0; i < objectives.Length; i++) {
				if (float.IsNaN(objectives[i]))
					throw new ArgumentException(nameof(objectives) + " can't contain NaNs");
			}

			_objectives = objectives;
			Count = objectives.Length;

			var hash = new HashCode();
			for (int i = 0; i < _objectives.Length; i++)
				hash.Add(_objectives[i]);

			_precomputedHashCode = hash.ToHashCode();
		}

		public static Fitness Wrap(float[] objectives) {
			if (objectives.Length == 0)
				throw new ArgumentException(nameof(objectives) + " can't be empty");

			return new Fitness(objectives);
		}

		public float this[int index] => _objectives[index];

		public int Count { get; }

		public override string ToString() => "[" + string.Join(", ", _objectives) + "]";

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) {
			if (obj is Fitness other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(Fitness other) {
			if (ReferenceEquals(this, other))
				return true;

			// Again, fitnesses should all have the same length
			// finding one with different length indicates a critical error
			if (Count != other.Count)
				throw new InvalidOperationException("Fitness should ALWAYS have the same Count");

			var lhs = _objectives;
			var rhs = other._objectives;

			for (int i = 0; i < lhs.Length; i++) {
				if (lhs[i] != rhs[i])
					return false;
			}

			return true;
		}

		public IEnumerator<float> GetEnumerator() => _objectives.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _objectives.GetEnumerator();
	}
}
