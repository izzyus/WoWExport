//Adapted from Selzier's code [https://github.com/Selzier/wow.export.unity/blob/master/src/js/3D/exporters/ADTExporter.js]
using System.Linq;
using System.Drawing;
using WoWFormatLib.Structs.ADT;
using System;

namespace Generators.ADT_Height
{
	class ADT_Height
	{
		//-----------------------------------------------------------------------------------------------------------------
		//PUBLIC STUFF:
		//-----------------------------------------------------------------------------------------------------------------
		public Bitmap heightMap = new Bitmap(257, 257);
		public float[,] heightArray2d = new float[257, 257];
		//-----------------------------------------------------------------------------------------------------------------

		public void GenerateHeightmap(ADT adtfile)
		{
			float[] vertexHeightList = new float[66049];

			int hCount = 0;
			uint ci = 0;
			for (var x = 0; x < 16; x++)
			{
				for (var y = 0; y < 16; y++)
				{
					var chunk = adtfile.chunks[ci];
					for (int row = 0, idx = 0; row < 17; row++)
					{
						bool isShort = (row % 2) != 0;
						int rowLength = isShort ? 8 : 9;

						for (var col = 0; col < rowLength; col++)
						{
							var vY = chunk.vertices.vertices[idx] + chunk.header.position.Z;

							#region vertexHeightList_Logic
							if (x < 15)
							{
								if (!isShort && y < 15 && row < 16)
								{ // Long Column; Don't draw bottom row
									if (col == 0)
									{
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else if (col < 8)
									{
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 1] + chunk.header.position.Z) + vY) / 2; // New Vertex in middle (531 = white)
										hCount++;
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else if (col == 8)
									{ // Bottom Left Corner
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 1] + chunk.header.position.Z) + vY) / 2; // New Vertex in middle
										hCount++;
									}
								}
								else if (!isShort && y == 15 && row < 16)
								{ // Long Column; Include bottom row
									if (col == 0)
									{
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else
									{
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 1] + chunk.header.position.Z) + vY) / 2; // New Vertex in middle
										hCount++;
										vertexHeightList[hCount] = vY; // Include bottom row of pixels
										hCount++;
									}
								}
								else if (isShort && y < 15 && row < 16)
								{ // Short Column; Don't draw bottom row
									vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 9] + chunk.header.position.Z) + (chunk.vertices.vertices[idx + 8] + chunk.header.position.Z)) / 2; // New Vertex in middle
									hCount++;
									vertexHeightList[hCount] = vY;
									hCount++;
								}
								else if (isShort && y == 15 && row < 16)
								{ // Short Column; Include bottom row
									if (col < 7)
									{
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 9] + chunk.header.position.Z) + (chunk.vertices.vertices[idx + 8] + chunk.header.position.Z)) / 2; // New Vertex in middle 
										hCount++;
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else if (col == 7)
									{
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 9] + chunk.header.position.Z) + (chunk.vertices.vertices[idx + 8] + chunk.header.position.Z)) / 2; // New Vertex in middle 
										hCount++;
										vertexHeightList[hCount] = vY;
										hCount++;
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 8] + chunk.header.position.Z) + (chunk.vertices.vertices[idx + 9] + chunk.header.position.Z)) / 2;
										hCount++;
									}
								}
							}
							else if (x == 15)
							{ // Include Right Edge							
								if (!isShort && y < 15)
								{ // Long Column; Don't draw bottom row
									if (col == 0)
									{
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else if (col < 8)
									{
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 1] + chunk.header.position.Z) + vY) / 2; // New Vertex in middle
										hCount++;
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else if (col == 8)
									{ // Bottom Left Corner
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 1] + chunk.header.position.Z) + vY) / 2; // New Vertex in middle
										hCount++;
									}
								}
								else if (!isShort && y == 15)
								{ // Long Column; Include bottom row
									if (col == 0)
									{
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else if (col < 8)
									{
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 1] + chunk.header.position.Z) + vY) / 2; // New Vertex in middle
										hCount++;
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else if (col == 8)
									{ // Bottom Pixel
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 1] + chunk.header.position.Z) + vY) / 2; // New Vertex in middle
										hCount++;
										vertexHeightList[hCount] = vY; // Include bottom row of pixels
										hCount++;
									}
								}
								else if (isShort && y < 15)
								{ // Short Column; Don't draw bottom row
									vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 9] + chunk.header.position.Z) + (chunk.vertices.vertices[idx + 8] + chunk.header.position.Z)) / 2; // New Vertex in middle
									hCount++;
									vertexHeightList[hCount] = vY;
									hCount++;
								}
								else if (isShort && y == 15)
								{ // Short Column; Include bottom row
									if (col < 7)
									{
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 9] + chunk.header.position.Z) + (chunk.vertices.vertices[idx + 8] + chunk.header.position.Z)) / 2; // New Vertex in middle
										hCount++;
										vertexHeightList[hCount] = vY;
										hCount++;
									}
									else if (col == 7)
									{
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 9] + chunk.header.position.Z) + (chunk.vertices.vertices[idx + 8] + chunk.header.position.Z)) / 2; // New Vertex in middle
										hCount++;
										vertexHeightList[hCount] = vY;
										hCount++;
										vertexHeightList[hCount] = ((chunk.vertices.vertices[idx - 8] + chunk.header.position.Z) + (chunk.vertices.vertices[idx + 9] + chunk.header.position.Z)) / 2; // Include bottom vertex
										hCount++;
									}
								}
							}
							#endregion
							idx++;
						}
					}
					ci++;
				}
			}

			// Determine min/max terrain height values
			var maxHeight = vertexHeightList.Max();
			var minHeight = vertexHeightList.Min();

			// Check for values which are not numbers
			for (var i = 0; i < 66049; i++)
			{
				if (float.IsNaN(vertexHeightList[i]))
				{
					vertexHeightList[i] = maxHeight;
				}
			}
			#region Heightmam_Bitmap
			// Draw heightmap
			int yOff = 0;
			int xOff = 0;
			hCount = 0;
			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					var xSize = 16;
					var ySize = 16;

					if (y == 15 && x < 15)
					{
						ySize = 17;
					}
					else if (x == 15 && y < 15)
					{
						xSize = 17;
					}
					else if (x == 15 && y == 15)
					{
						xSize = 17;
						ySize = 17;
					}
					else
					{
						xSize = 16;
						ySize = 16;
					}

					for (int col = 0; col < xSize; col++)
					{
						for (int row = 0; row < ySize; row++)
						{
							var normalized = FloatToIntNormalized255(vertexHeightList[hCount], maxHeight, minHeight);
							var color = System.Drawing.Color.FromArgb(255, normalized, normalized, normalized);
							heightMap.SetPixel(col + xOff, row + yOff, color);

							//For JSON generation
							heightArray2d[col + xOff, row + yOff] = vertexHeightList[hCount];

							hCount++;
						}
					}

					if (yOff + ySize > 256)
					{
						yOff = 0;
						if (xOff + xSize <= 256)
						{
							xOff += xSize;
						}
					}
					else
					{
						yOff += ySize;
					}
				}
			}
			//----------------------------------------------------------------------------------------------------------
			//Fix bmp orientation:
			//----------------------------------------------------------------------------------------------------------
			heightMap.RotateFlip(RotateFlipType.Rotate270FlipY);
			//----------------------------------------------------------------------------------------------------------
			#endregion

			//----------------------------------------------------------------------------------------------------------
			//Fix array orientation:
			//----------------------------------------------------------------------------------------------------------
			heightArray2d = RotateMatrixCounterClockwise(heightArray2d);
			heightArray2d = FlipMatrix(heightArray2d);
			//----------------------------------------------------------------------------------------------------------
		}

		private static int FloatToIntNormalized255(float value, float min, float max)
		{
			return (int)(((value - max) / (min - max)) * 255);
		}


		static float[,] RotateMatrixCounterClockwise(float[,] oldMatrix)
		{
			float[,] newMatrix = new float[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
			int newColumn, newRow = 0;
			for (int oldColumn = oldMatrix.GetLength(1) - 1; oldColumn >= 0; oldColumn--)
			{
				newColumn = 0;
				for (int oldRow = 0; oldRow < oldMatrix.GetLength(0); oldRow++)
				{
					newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
					newColumn++;
				}
				newRow++;
			}
			return newMatrix;
		}

		static float[,] FlipMatrix(float[,] oldMatrix)
		{
			float[,] newMatrix = new float[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];

			for (int i = 0; i < oldMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < oldMatrix.GetLength(1); j++)
				{
					newMatrix[j, i] = oldMatrix[i, j];
				}
			}
			return newMatrix;
		}
	}
}