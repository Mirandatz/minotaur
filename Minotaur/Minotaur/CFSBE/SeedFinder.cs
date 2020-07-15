namespace Minotaur.CFSBE {
	using System;
	using Minotaur.Classification.Rules;
	using Minotaur.Collections;
	using Minotaur.Datasets;

	public sealed class SeedFinder {

		private readonly DatasetCoverageComputer _coverageComputer;

		public SeedFinder(DatasetCoverageComputer coverageComputer) {
			_coverageComputer = coverageComputer;
		}

		public Array<int> FindSeedsIndices(ReadOnlySpan<Rule> rules) => throw new NotImplementedException();

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
