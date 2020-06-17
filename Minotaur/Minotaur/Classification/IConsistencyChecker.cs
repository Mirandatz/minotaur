namespace Minotaur.Classification {
	using Minotaur.Classification.Rules;

	public interface IConsistencyChecker {

		bool AreConsistent(RuleSet rules);
	}
}
