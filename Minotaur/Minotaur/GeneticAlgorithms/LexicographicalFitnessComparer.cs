namespace Minotaur.GeneticAlgorithms {
	using System;
	using System.Collections.Generic;

	public sealed class LexicographicalFitnessComparer: IComparer<Fitness> {

		public int Compare(Fitness lhs, Fitness rhs) {
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
