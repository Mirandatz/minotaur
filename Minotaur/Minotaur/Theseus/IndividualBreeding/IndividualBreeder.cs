namespace Minotaur.Theseus.IndividualBreeding {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class IndividualBreeder {

		private readonly Dataset _dataset;
		private readonly RuleConsistencyChecker _consistencyChecker;

		public IndividualBreeder(Dataset dataset, RuleConsistencyChecker ruleConsistencyChecker) {
			_dataset = dataset;
			_consistencyChecker = ruleConsistencyChecker;
			throw new NotImplementedException();
		}

		public Individual Breed(Individual lhs, Individual rhs) {
			throw new NotImplementedException();
			//if (lhs.Equals(rhs))
			//	return lhs;

			//// @Performance
			//var combinedRules = lhs.Rules
			//	.Concat(rhs.Rules)
			//	.OrderBy(r => -1 * VolumeComputer.ComputeRuleVolume(_dataset, r))
			//	.ToArray();

			//var consistentRules = new List<Rule>(combinedRules.Length);
			//consistentRules.Add(combinedRules[0]);

			//for (int i = 1; i < combinedRules.Length; i++) {
			//	var currentRule = combinedRules[i];
			//	if (_consistencyChecker.AreConsistent(consistentRules, currentRule)) {
			//		consistentRules.Add(currentRule);
			//	} else {
			//		break;
			//	}
			//}

			//return new Individual(
			//	rules: consistentRules.ToArray(),
			//	defaultPrediction: _dataset.DefaultLabel);
		}
	}
}
