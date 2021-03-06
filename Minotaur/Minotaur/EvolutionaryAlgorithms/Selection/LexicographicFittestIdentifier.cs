namespace Minotaur.EvolutionaryAlgorithms.Selection {
	using System;
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
				.CreateSorted(inclusiveStart: 0, exclusiveEnd: fitnesses.Length)
				.ToArray();

			var fitnessArray = fitnesses.ToArray();

			Array.Sort(
				keys: fitnessArray,
				items: indices,
				comparer: _comparer);

			Array.Reverse(indices);

			var fittest = indices
				.AsSpan()
				.Slice(start: 0, length: _fittestCount)
				.ToArray();

			return fittest;
		}
	}
}