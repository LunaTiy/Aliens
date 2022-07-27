using System.Linq;

namespace Aliens
{
	public static class Program
	{
		private const string FileName = "AlienMarking.txt";

		public static void Main(string[] args)
		{
			if (!TryReadDataFromFile(FileName, out int[,] matrix))
				throw new Exception("Can't read file");

			int sum = GetSumOfElementsOfSubMatrix(matrix, out int countOfSubMatrix);
		}

		private static bool TryReadDataFromFile(string fileName, out int[,] matrix)
		{
			if (!File.Exists(fileName))
			{
				matrix = null;
				return false;
			}

			string[] lines = File.ReadAllLines(fileName);
			int lengthLines = lines.Length;

			matrix = new int[lengthLines, lengthLines];

			for (int i = 0; i < lengthLines; i++)
			{
				string[] elements = lines[i].Split(' ');

				for (int j = 0; j < elements.Length; j++)
				{
					if (!string.IsNullOrWhiteSpace(elements[j]))
						matrix[i, j] = int.Parse(elements[j]);
				}
			}

			return true;
		}

		private static int GetSumOfElementsOfSubMatrix(int[,] matrix, out int countOfSubMatrix)
		{
			int allSum = 0;
			int countBomb = 0;

			countOfSubMatrix = 0;

			int matrixWidth = matrix.GetLength(0);
			int matrixHeight = matrix.GetLength(1);

			for (int i = 0; i < matrixHeight; i++)
			{
				for (int j = 0; j < matrixWidth; j++)
				{
					Console.WriteLine($"Row: {i}, column: {j}");
					
					for (int countElementsInRow = 1; countElementsInRow < matrixHeight + 1 - i; countElementsInRow++)
					{
						for (int countElementsInColumn = 1; countElementsInColumn < matrixWidth + 1 - j; countElementsInColumn++)
						{
							int sumOfSubMatrix = 0;
							countOfSubMatrix++;

							for (int n = i; n < countElementsInRow + i && n < matrixHeight; n++)
							{
								for (int m = j; m < countElementsInColumn + j && m < matrixWidth; m++)
								{
									if (matrix[n, m] == -1)
										countBomb++;

									sumOfSubMatrix += matrix[n, m];
								}
							}

							sumOfSubMatrix *= (int)Math.Pow(-1, countBomb);
							countBomb = 0;

							if (sumOfSubMatrix % 13 == 0)
								allSum += sumOfSubMatrix;
						}
					}
				}
			}

			return allSum;
		}
	}
}