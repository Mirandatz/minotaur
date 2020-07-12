namespace Minotaur.FittestSelection {
	using System;

	public static class IFittestIdentifierParser {


		public static IFittestIdentifier Parse(string name, int fittestCount) {
			return name switch
			{
				"lexicographic" => new LexicographicFittestIdentifier(fittestCount: fittestCount),

				_ => throw new ArgumentException($"Unsupported fittest identifier {name}"),
			};
		}
	}
}
