namespace Minotaur.EvolutionaryAlgorithms.Selection {
	using System;

	public static class IFittestIdentifierParser {


		public static IFittestIdentifier Parse(string name, int fittestCount) {
			return name switch
			{
				"nsga2" => new NSGA2(fittestCount: fittestCount),
				"lexicographic" => new LexicographicFittestIdentifier(fittestCount: fittestCount),

				_ => throw new ArgumentException($"Unsupported fittest identifier {name}"),
			};
		}
	}
}
