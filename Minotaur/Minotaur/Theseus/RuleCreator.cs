namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class RuleCreator {

		public bool TryCreateNewRule(Array<Rule> existingRules, out Rule newRule) {
			if (existingRules is null)
				throw new ArgumentNullException(nameof(existingRules));
			if (existingRules.ContainsNulls())
				throw new ArgumentException(nameof(existingRules) + " can't contain nulls.");

			throw new NotImplementedException();
		}
	}
}
