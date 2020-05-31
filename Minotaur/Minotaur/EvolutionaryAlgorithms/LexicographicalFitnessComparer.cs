namespace Minotaur.EvolutionaryAlgorithms {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	public sealed class LexicographicalFitnessComparer: IComparer<Fitness> {
		public int Compare([AllowNull] Fitness lhs, [AllowNull] Fitness rhs) {
			if (lhs is null)
				throw new ArgumentNullException(nameof(lhs));
			if (rhs is null)
				throw new ArgumentNullException(nameof(rhs));
			if (lhs.Count != rhs.Count)
				throw new ArgumentException(nameof(lhs) + " and " + nameof(rhs) + " must have the same length.");

			for (int i = 0; i < lhs.Count; i++) {
				if (lhs[i] < rhs[i])
					return -1;
				else if (lhs[i] > rhs[i])
					return 1;
			}

			return 0;
		}
	}
}
