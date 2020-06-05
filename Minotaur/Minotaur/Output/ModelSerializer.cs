namespace Minotaur.Output {
	using System;
	using System.Globalization;
	using System.IO;
	using CsvHelper;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class ModelSerializer {

		public void Serialize(TextWriter textWriter, Individual model) {
			// @Assumption: all rules within a model have the same number of 
			// feature tests in their antecedents.
			// @Assumption: all rules within a model have the same type of 
			// consequent.
			// @Assumption: all consequents of the same type have the same length

			throw new NotImplementedException();

			//using var csvWriter = new CsvWriter(writer: textWriter, cultureInfo: CultureInfo.InvariantCulture);

			//WriteHeader(csvWriter, model);

			//foreach (var rule in model.Rules) {
			//	WriteAntecedent(csvWriter, rule.Antecedent);
			//	WriteConsequent(csvWriter, rule.Consequent);
			//	csvWriter.NextRecord();
			//}
		}

		private void WriteHeader(CsvWriter csvWriter, Individual model) {
			throw new NotImplementedException();
			//var featureCount = model.Rules[0].Antecedent.Length;
			//for (int i = 0; i < featureCount; i++)
			//	csvWriter.WriteField($"Feature Test {i}");

			//var classCount = ((MultiLabel) model.Rules[0].Consequent).Length;
			//for (int i = 0; i < classCount; i++)
			//	csvWriter.WriteField($"Class {i}");

			//csvWriter.NextRecord();
		}

		private void WriteAntecedent(CsvWriter csvWriter, Array<IFeatureTest> antecedent) {
			foreach (var featureTest in antecedent) {
				var continuousFeatureTest = (ContinuousFeatureTest) featureTest;
				CsvSerializationHelper.Write(csvWriter, continuousFeatureTest);
			}
		}

		private void WriteConsequent(CsvWriter csvWriter, ILabel consequent) {
			var multiLabelPredictions = (MultiLabel) consequent;
			CsvSerializationHelper.Write(csvWriter, multiLabelPredictions);
		}
	}
}
