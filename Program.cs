using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;

namespace Aliens
{
	public static class Program
	{
		// Test file with matrix 4x4 data.txt, working matrix 500x500 in AlienMarking.txt
		private const string FileName = "AlienMarking.txt";

		public static void Main(string[] args)
		{
			if (!TryReadDataFromFile(FileName, out long[,] matrix))
				throw new Exception("Can't read file");

			long[,] bombsMatrix = GetBombMatrix(matrix);
			long[,] prefixMatrix = ConvertMatrixToPrefixMatrix(matrix);
			long result = GetSumOfSubMatrixInPrefixMatrix(prefixMatrix, bombsMatrix);

			Console.WriteLine($"Result sum: {result}");
		}

		/*
		 * Generate random matrix for test
		 */
		private static long[,] GenerateMatrix(int length)
		{
			long[,] matrix = new long[length, length];

			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length; j++)
				{
					matrix[i, j] = new Random().Next(20);
				}
			}

			return matrix;
		}

		private static bool TryReadDataFromFile(string fileName, out long[,] matrix)
		{
			if (!File.Exists(fileName))
			{
				matrix = null;
				return false;
			}

			string[] lines = File.ReadAllLines(fileName);
			int lengthLines = lines.Length;

			matrix = new long[lengthLines, lengthLines];

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

		/*
		 * My first solution, but this method is not optimization
		 * O(n ^ 6)
		 */
		private static long GetSumOfSubMatrix(long[,] matrix, out long countOfSubMatrix)
		{
			long allSum = 0;
			long countBomb = 0;

			countOfSubMatrix = 0;

			int matrixWidth = matrix.GetLength(0);
			int matrixHeight = matrix.GetLength(1);

			for (int i = 0; i < matrixHeight; i++)
			{
				for (int j = 0; j < matrixWidth; j++)
				{
					for (int countElementsInRow = 1; countElementsInRow < matrixHeight + 1 - i; countElementsInRow++)
					{
						for (int countElementsInColumn = 1; countElementsInColumn < matrixWidth + 1 - j; countElementsInColumn++)
						{
							long sumOfSubMatrix = 0;
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

		/*
		 * Convert matrix to prefix matrix
		 *
		 * Example:
		 *  __ __ __     __ __ ___
		 * |10 20 30    |10 30  60
		 * | 5 10 20 => |15 45  95
		 * | 2  4  6    |17 51 107
		 *
		 * How we get 60? 10 + 20 + 30 (input matrix) = 60 (prefix matrix)
		 * How we get 45? 10 + 20 + 5 + 10 (input matrix) = 45 (prefix matrix)
		 *
		 * 45 - it is matrix from 0:0 to 1:1 (subMatrix)
		 */
		private static long[,] ConvertMatrixToPrefixMatrix(long[,] matrix)
		{
			int length = matrix.GetLength(0);
			long[,] prefixMatrix = new long[length, length];

			prefixMatrix[0, 0] = matrix[0, 0];

			for (int i = 1; i < length; i++)
				prefixMatrix[0, i] = prefixMatrix[0, i - 1] + matrix[0, i];

			for (int i = 1; i < length; i++)
				prefixMatrix[i, 0] = prefixMatrix[i - 1, 0] + matrix[i, 0];

			for (int i = 1; i < length; i++)
				for (int j = 1; j < length; j++)
					prefixMatrix[i, j] = prefixMatrix[i - 1, j] + prefixMatrix[i, j - 1] - prefixMatrix[i - 1, j - 1] + matrix[i, j];

			return prefixMatrix;
		}

		/*
		 * Get Prefix matrix with bombs
		 */
		private static long[,] GetBombMatrix(long[,] matrix)
		{
			int length = matrix.GetLength(0);
			long[,] bombsMatrix = new long[length, length];

			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length; j++)
				{
					if (matrix[i, j] == -1)
						bombsMatrix[i, j] = 1;
					else
						bombsMatrix[i, j] = 0;
				}
			}

			return ConvertMatrixToPrefixMatrix(bombsMatrix);
		}

		/*
		 * Second solution with prefix matrix
		 * O(n ^ 4)
		 */
		private static long GetSumOfSubMatrixInPrefixMatrix(long[,] prefixMatrix, long[,] bombsMatrix)
		{
			long resultSum = 0;
			long length = prefixMatrix.GetLength(0);
			long count = 0;

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			for (int i = 0; i < length; i++)
			{
				Console.WriteLine($"Processing {i + 1} / {length}... Elapsed time: {stopwatch.Elapsed}");

				for (int j = 0; j < length; j++)
				{
					for (int countElementsInRow = 1; countElementsInRow < length + 1 - i; countElementsInRow++)
					{
						for (int countElementsInColumn = 1; countElementsInColumn < length + 1 - j; countElementsInColumn++)
						{
							long sumOfSubMatrix;
							long countBombs = 0;
							count++;

							if (i == 0 && j == 0)
							{
								sumOfSubMatrix = prefixMatrix[countElementsInRow - 1, countElementsInColumn - 1];

								countBombs = bombsMatrix[countElementsInRow - 1, countElementsInColumn - 1];
							}
							else if(i == 0)
							{
								sumOfSubMatrix = prefixMatrix[countElementsInRow - 1, countElementsInColumn + j - 1] - prefixMatrix[countElementsInRow - 1, j - 1];

								countBombs = bombsMatrix[countElementsInRow - 1, countElementsInColumn + j - 1] - bombsMatrix[countElementsInRow - 1, j - 1];
							}
							else if (j == 0)
							{
								sumOfSubMatrix = prefixMatrix[countElementsInRow + i - 1, countElementsInColumn - 1] - prefixMatrix[i - 1, countElementsInColumn - 1];

								countBombs = bombsMatrix[countElementsInRow + i - 1, countElementsInColumn - 1] - bombsMatrix[i - 1, countElementsInColumn - 1];
							}
							else
							{
								long currentMatrix = prefixMatrix[countElementsInRow + i - 1, countElementsInColumn + j - 1];
								long subMatrixTop = prefixMatrix[i - 1, countElementsInColumn + j - 1];
								long subMatrixLeft = prefixMatrix[countElementsInRow + i - 1, j - 1];
								long mergeMatrix = prefixMatrix[i - 1, j - 1];

								sumOfSubMatrix = currentMatrix - subMatrixTop - subMatrixLeft + mergeMatrix;

								countBombs = bombsMatrix[countElementsInRow + i - 1, countElementsInColumn + j - 1] -
								                 bombsMatrix[i - 1, countElementsInColumn + j - 1] -
								                 bombsMatrix[countElementsInRow + i - 1, j - 1] +
								                 bombsMatrix[i - 1, j - 1];
							}

							if (sumOfSubMatrix % 13 == 0)
							{
								sumOfSubMatrix *= (int)Math.Pow(-1, countBombs);
								resultSum += sumOfSubMatrix;

								//Console.WriteLine($"Count bombs = {countBombs}");
								//Console.WriteLine($"Sum % 13 in {count} matrix start {i}:{j} end {i + countElementsInRow - 1}:{j + countElementsInColumn - 1} = {sumOfSubMatrix}");
							}
						}
					}
				}
			}

			Console.WriteLine($"Result time {stopwatch.Elapsed}");
			stopwatch.Stop();

			return resultSum;
		}
	}
}