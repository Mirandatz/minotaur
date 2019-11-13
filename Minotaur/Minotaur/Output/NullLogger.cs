namespace Minotaur.Output {
	using Minotaur.Theseus.Evolution;

	public sealed class NullLogger: IEvolutionLogger {

		public void LogGeneration(GenerationResult generationResult) { }
	}
}
