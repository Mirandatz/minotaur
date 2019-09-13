namespace Minotaur {
	using System;
	using System.Text;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;

	public static class FitnessReportMaker {

		public static string MakeReport(Array<Fitness> fitnesses) {
			if (fitnesses is null)
				throw new ArgumentNullException(nameof(fitnesses));
			if (fitnesses.Length < 3)
				throw new ArgumentException(nameof(fitnesses) + " must contain at least 3 elements.");

			var sortedFitnesses = fitnesses.ToArray();
			Array.Sort(
				array: sortedFitnesses,
				comparer: new LexicographicalFitnessComparer());

			var builder = new StringBuilder();

			builder.Append("Best individual: ");
			builder.AppendLine(sortedFitnesses[0].ToString());

			builder.Append("Best individual: ");
			builder.AppendLine(sortedFitnesses[sortedFitnesses.Length / 2].ToString());

			builder.Append("Worst individual: ");
			builder.AppendLine(sortedFitnesses[sortedFitnesses.Length - 1].ToString());

			return builder.ToString();
		}
	}
}
