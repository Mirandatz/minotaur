namespace Minotaur.RuleCreation {
	using System;
	using Minotaur.Classification.Rules;
	using Minotaur.RuleCreation.CFSBE;

	public sealed class RuleCreator {

		private readonly UncoveredHyperRectangleFinder _rectangleFinder;

		public RuleCreator(UncoveredHyperRectangleFinder rectangleFinder) {
			_rectangleFinder = rectangleFinder;
		}

		public Rule? TryCreateRule(ReadOnlySpan<Rule> existingRules) {
			if (existingRules.IsEmpty)
				return CreateRandomRule();

			throw new NotImplementedException();
		}

		private Rule CreateRandomRule() {
			throw new NotImplementedException();
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
