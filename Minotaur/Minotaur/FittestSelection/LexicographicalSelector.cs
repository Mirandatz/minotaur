namespace Minotaur.FittestSelection {
	using System;
	using Minotaur.Collections;
	using Minotaur.Math;

	public sealed class LexicographicalSelector: IFittestSelector {

		private readonly int _fittestCount;
		private readonly LexicographicalFitnessComparer _comparer = new LexicographicalFitnessComparer();

		public LexicographicalSelector(int fittestCount) {
			if (fittestCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(fittestCount) + " must be >= 0.");

			_fittestCount = fittestCount;
		}

		public int[] FindIndicesOfFittestIndividuals(ReadOnlySpan<Fitness> fitnesses) {
			if (fitnesses.Length < _fittestCount)
				throw new ArgumentException(nameof(fitnesses) + $" must contain at least {_fittestCount} elements.");

			var indices = IndexingHelper.CreateIndices(count: fitnesses.Length);

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

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}