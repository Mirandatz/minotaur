namespace Minotaur.Theseus {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class RuleCreator {

		public readonly Dataset Dataset;
		private readonly SeedSelector _seedSelector;
		private readonly HyperRectangleCreator _hyperRectangleCreator;
		private readonly HyperRectangleEnlarger _hyperRectangleExpander;

		public bool TryCreateRule(Array<Rule> existingRules, out Rule rule) {
			if (existingRules is null)
				throw new ArgumentNullException(nameof(existingRules));

			var seedFound = _seedSelector.TryFindSeed(
				existingRules: existingRules,
				out var seed);

			if (!seedFound) {
				rule = null;
				return false;
			}

			var hyperRectangles = new HyperRectangle[existingRules.Length];
			Parallel.For(0, hyperRectangles.Length, i => {
				var currentRule = existingRules[i];
				var hyperRectangle = _hyperRectangleCreator.FromRule(currentRule);
				hyperRectangles[i] = hyperRectangle;
			});

			var enlargedSpace = _hyperRectangleExpander.Enlarge(
				target: seed,
				others: hyperRectangles,
				dimensionExpansionOrder: null);

			throw new NotImplementedException();
		}
	}
}
