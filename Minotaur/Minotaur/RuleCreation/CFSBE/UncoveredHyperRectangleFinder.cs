namespace Minotaur.RuleCreation.CFSBE {
	using System;
	using Minotaur.Classification.Rules;
	using Minotaur.Math.Geometry;

	public sealed class UncoveredHyperRectangleFinder {

		public HyperRectangle? FindUncoveredHyperRectangle(ReadOnlySpan<Rule> rules) {
			if (rules.IsEmpty)
				throw new ArgumentException(nameof(rules) + " can't be empty.");

			throw new NotImplementedException();
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
