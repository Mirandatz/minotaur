namespace Minotaur.GeneticAlgorithms.Selection {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections;
	using Minotaur.Math;

	public sealed class LexicographicFittestIdentifier: IFittestIdentifier {
		private readonly int _fittestCount;
		private readonly LexicographicalFitnessComparer _comparer = new LexicographicalFitnessComparer();

		public LexicographicFittestIdentifier(int fittestCount) {
			_fittestCount = fittestCount;
		}

		public int[] FindIndicesOfFittestIndividuals(Array<Fitness> fitnesses) {
			var indices = NaturalRange
				.CreateSorted(inclusiveStart: 0, exclusiveEnd: _fittestCount)
				.ToArray();

			var fitnessArray = fitnesses.ToArray();

			Array.Sort(
				keys: fitnessArray,
				items: indices,
				comparer: _comparer);

			var fittest = indices
				.AsSpan()
				.Slice(start: 0, length: _fittestCount)
				.ToArray();

			return fittest;
		}

		private sealed class LexicographicalFitnessComparer: IComparer<Fitness> {

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
}