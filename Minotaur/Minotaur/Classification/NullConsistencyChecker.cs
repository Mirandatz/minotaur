namespace Minotaur.Classification {
	using Minotaur.Classification.Rules;

	public sealed class NullConsistencyChecker: IConsistencyChecker {

		public bool AreConsistent(RuleSet rules) => true;
	}
}
