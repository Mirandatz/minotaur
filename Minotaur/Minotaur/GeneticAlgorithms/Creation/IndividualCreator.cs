namespace Minotaur.GeneticAlgorithms.Creation {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;
	using Random = Minotaur.Random.ThreadStaticRandom;

	/// <summary>
	/// This is a "dumb individual creator"; it doesn't provide any guarantees on intra-individual
	/// rule consistency, i.e. it's possible (and likely) that the individuals created by this 
	/// class will contain rules that are inconsistent.
	/// </summary>
	public sealed class NaiveIndividualCreator {
		private readonly int _maxRules;
		private readonly RuleCreator _ruleCreator;
		private readonly Array<bool> _defaultLabels;

		public NaiveIndividualCreator(int maxRules, RuleCreator ruleCreator, Array<bool> defaultLabels) {
			if (maxRules < 1)
				throw new ArgumentOutOfRangeException(nameof(maxRules) + " must be >= 1");

			_maxRules = maxRules;
			_ruleCreator = ruleCreator ?? throw new ArgumentNullException(nameof(ruleCreator));
			_defaultLabels = defaultLabels ?? throw new ArgumentNullException(nameof(defaultLabels));
		}

		public Individual CreateIndividual() {
			var ruleCount = Random.Int(
				inclusiveMin: Individual.MinimumRuleCount,
				exclusiveMax: _maxRules);

			var rules = new Rule[ruleCount];
			for (int i = 0; i < rules.Length; i++)
				rules[i] = _ruleCreator.CreateRule();

			return new Individual(
				rules: rules,
				defaultLabels: _defaultLabels);
		}
	}
}
