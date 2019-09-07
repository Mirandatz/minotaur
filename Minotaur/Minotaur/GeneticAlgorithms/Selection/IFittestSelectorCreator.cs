namespace Minotaur.GeneticAlgorithms.Selection {
	using System;

	public static class IFittestSelectorCreator {

		public static IFittestSelector Create(
			string fittestSelectorName,
			int fittestCount,
			FitnessEvaluator fitnessEvaluator
			) {
			if (fittestSelectorName is null)
				throw new ArgumentNullException(nameof(fittestSelectorName));
			if (fitnessEvaluator is null)
				throw new ArgumentNullException(nameof(fitnessEvaluator));
			if (fittestCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(fittestCount));

			switch (fittestSelectorName) {
			case "nsga2":
			return new NSGA2(
				fitnessEvaluator: fitnessEvaluator,
				fittestCount: fittestCount);

			case "lexicographic":
			throw new NotImplementedException();

			default:
			throw new ArgumentException($"Unknown selection algorithm: {fittestSelectorName}");
			}
		}
	}
}