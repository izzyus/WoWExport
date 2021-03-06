﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using WoWFormatLib.Structs.ADT;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Generators.ADT_Alpha
{
    class ADT_Alpha
    {
        //-----------------------------------------------------------------------------------------------------------------
        //PUBLIC STUFF:
        //-----------------------------------------------------------------------------------------------------------------
        public List<Bitmap> AlphaLayers = new List<Bitmap>();
        public List<String> AlphaLayersNames = new List<String>();
        public String SplatmapJSON;
        //-----------------------------------------------------------------------------------------------------------------

        public void GenerateAlphaMaps(ADT adtfile, int GenerationMode)
        {
            if (GenerationMode == 0 || GenerationMode == 1) //MODE 0 & 1 (256 RGB ALPHAS, ONE PER CHUNK)
            {
                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///ALPHA MAPS TEST
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------
                var valuesR = new MCAL().layer;
                var valuesG = new MCAL().layer;
                var valuesB = new MCAL().layer;
                for (uint c = 0; c < adtfile.chunks.Count(); c++)
                {
                    if (adtfile.texChunks[c].alphaLayer != null)
                    {
                        var bmp = new Bitmap(64, 64);
                        //Assign the channels...
                        switch (adtfile.texChunks[c].layers.Count())
                        {
                            case 2:
                                valuesR = adtfile.texChunks[c].alphaLayer[1].layer;
                                break;
                            case 3:
                                valuesR = adtfile.texChunks[c].alphaLayer[1].layer;
                                valuesG = adtfile.texChunks[c].alphaLayer[2].layer;
                                break;

                            case 4:
                                valuesR = adtfile.texChunks[c].alphaLayer[1].layer;
                                valuesG = adtfile.texChunks[c].alphaLayer[2].layer;
                                valuesB = adtfile.texChunks[c].alphaLayer[3].layer;
                                break;
                            default:
                                //Don't do anything
                                break;
                        }
                        if (GenerationMode == 0)
                        {
                            // 64x64 ALPHAS:
                            for (int x = 0; x < 64; x++)
                            {
                                for (int y = 0; y < 64; y++)
                                {
                                    Color color;
                                    switch (adtfile.texChunks[c].layers.Count())
                                    {
                                        case 2:
                                            color = Color.FromArgb(255, valuesR[x * 64 + y], 0, 0);
                                            break;
                                        case 3:
                                            color = Color.FromArgb(255, valuesR[x * 64 + y], valuesG[x * 64 + y], 0);
                                            break;
                                        case 4:
                                            color = Color.FromArgb(255, valuesR[x * 64 + y], valuesG[x * 64 + y], valuesB[x * 64 + y]);
                                            break;
                                        default:
                                            color = Color.FromArgb(255, 0, 0, 0);
                                            break;
                                    }
                                    bmp.SetPixel(x, y, color);
                                }
                            }
                        }
                        if (GenerationMode == 1)
                        {
                            // 63x63 ALPHAS: (Last column/row = previous one)
                            for (int x = 0; x < 63; x++)
                            {
                                for (int y = 0; y < 63; y++)
                                {
                                    Color color;
                                    switch (adtfile.texChunks[c].layers.Count())
                                    {
                                        case 2:
                                            color = Color.FromArgb(255, valuesR[x * 64 + y], 0, 0);
                                            break;
                                        case 3:
                                            color = Color.FromArgb(255, valuesR[x * 64 + y], valuesG[x * 64 + y], 0);
                                            break;
                                        case 4:
                                            color = Color.FromArgb(255, valuesR[x * 64 + y], valuesG[x * 64 + y], valuesB[x * 64 + y]);
                                            break;
                                        default:
                                            color = Color.FromArgb(255, 0, 0, 0);
                                            break;
                                    }
                                    bmp.SetPixel(x, y, color);
                                    if (y == 62) { bmp.SetPixel(x, y + 1, color); }
                                    if (x == 62) { bmp.SetPixel(x + 1, y, color); }
                                    if (x == 62 && y == 62) { bmp.SetPixel(x + 1, y + 1, color); }
                                }
                            }
                        }
                        //----------------------------------------------------------------------------------------------------------
                        //Fix bmp orientation:
                        //----------------------------------------------------------------------------------------------------------
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipY);
                        //----------------------------------------------------------------------------------------------------------

                        //----------------------------------------------------------------------------------------------------------
                        //Store the generated map in the array
                        //----------------------------------------------------------------------------------------------------------
                        AlphaLayers.Add(bmp);
                        //----------------------------------------------------------------------------------------------------------   
                    }
                    else //Create and add an empty BMP if Null
                    {
                        var bmp = new Bitmap(64, 64);
                        AlphaLayers.Add(bmp);
                    }
                }
                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///ALPHA MAPS TEST END
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------
            }
            if (GenerationMode == 2 || GenerationMode == 3) //MODE 2 & 3 (A BLACK&WHITE ALPHA FOR EACH LAYER (ROUGHLY ~768 ALPHAS))
            {
                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///ALPHA MAPS TEST
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------
                for (uint c = 0; c < adtfile.chunks.Count(); c++)
                {
                    var chunk = adtfile.chunks[c];
                    for (int li = 0; li < adtfile.texChunks[c].layers.Count(); li++)
                    {
                        if (adtfile.texChunks[c].alphaLayer != null)
                        {
                            var values = adtfile.texChunks[c].alphaLayer[li].layer;
                            var bmp = new System.Drawing.Bitmap(64, 64);
                            {
                                if (GenerationMode == 2)
                                {
                                    // 64x64 ALPHAS:
                                    for (int x = 0; x < 64; x++)
                                    {
                                        for (int y = 0; y < 64; y++)
                                        {
                                            Color color;
                                            if (Managers.ConfigurationManager.ADTAlphaUseA)
                                            {
                                                color = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
                                            }
                                            else
                                            {
                                                color = System.Drawing.Color.FromArgb(255, values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
                                            }
                                            bmp.SetPixel(x, y, color);
                                        }
                                    }
                                }
                                if (GenerationMode == 3)
                                {
                                    // 63x63 ALPHAS: (Last column/row = previous one)
                                    for (int x = 0; x < 63; x++)
                                    {
                                        for (int y = 0; y < 63; y++)
                                        {
                                            Color color;
                                            if (Managers.ConfigurationManager.ADTAlphaUseA)
                                            {
                                                color = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
                                            }
                                            else
                                            {
                                                color = System.Drawing.Color.FromArgb(255, values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
                                            }
                                            bmp.SetPixel(x, y, color);
                                            if (y == 62) { bmp.SetPixel(x, y + 1, color); }
                                            if (x == 62) { bmp.SetPixel(x + 1, y, color); }
                                            if (x == 62 && y == 62) { bmp.SetPixel(x + 1, y + 1, color); }
                                        }
                                    }
                                }
                            }
                            //----------------------------------------------------------------------------------------------------------
                            //Store the layer textures
                            //----------------------------------------------------------------------------------------------------------
                            var AlphaLayerName = adtfile.textures.filenames[adtfile.texChunks[c].layers[li].textureId].ToLower();
                            AlphaLayersNames.Add(c + ";" + li + ";" + Path.GetFileNameWithoutExtension(AlphaLayerName));
                            //----------------------------------------------------------------------------------------------------------

                            //----------------------------------------------------------------------------------------------------------
                            //Fix bmp orientation:
                            //----------------------------------------------------------------------------------------------------------
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipY);
                            //----------------------------------------------------------------------------------------------------------

                            //----------------------------------------------------------------------------------------------------------
                            //Store the generated map in the array
                            //----------------------------------------------------------------------------------------------------------
                            AlphaLayers.Add(bmp);
                            //----------------------------------------------------------------------------------------------------------   
                        }
                    }
                }
                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///ALPHA MAPS TEST END
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------
            }

            #region Splatmaps (1024x1024)
            if (GenerationMode == 4)
            {
                //Adapted from Selzier's code [https://github.com/Selzier/wow.export.unity/blob/master/src/js/3D/exporters/ADTExporter.js]

                //----------------------------------------------------------------------------------------------------------
                //Generate the splatmap json
                //----------------------------------------------------------------------------------------------------------
                #region Splatmap JSON
                string materialJSON = "{\"chunkData\":{"; // New JSON file to save material data
                for (uint c = 0; c < adtfile.chunks.Count(); c++)
                {
                    materialJSON += "\"" + c + "\":[";
                    for (int li = 0; li < adtfile.texChunks[c].layers.Count(); li++)
                    {
                        string AlphaLayerName = adtfile.textures.filenames[adtfile.texChunks[c].layers[li].textureId].ToLower();
                        if (Managers.ConfigurationManager.ADTPreserveTextureStruct)
                        {
                            AlphaLayerName = AlphaLayerName.Replace("\\", "\\\\").Replace(".blp", "");  //Remove extension and modify the path to use "\\" instead of "\"
                        }
                        else
                        {
                            AlphaLayerName = Path.GetFileNameWithoutExtension(AlphaLayerName); // Get only the filename
                        }

                        //Layer scale
                        float LayerScale = 4;
                        if (adtfile.texParams != null && adtfile.texParams.Count() >= adtfile.texChunks[c].layers[li].textureId)
                        {
                            LayerScale = (float)Math.Pow(2, (adtfile.texParams[adtfile.texChunks[c].layers[li].textureId].flags & 0xF0) >> 4);
                        }

                        materialJSON += "{\"id\":\"" + AlphaLayerName + "\",\"scale\":\"" + LayerScale + "\"},";
                    }
                    materialJSON = materialJSON.Substring(0, materialJSON.Length - 1); // Remove tailing comma
                    materialJSON += "],"; // Close the subchunk array
                }
                materialJSON = materialJSON.Substring(0, materialJSON.Length - 1); // Remove tailing comma
                string fullJSON = materialJSON + "},\"splatmapData\":{"; // Create JSON data to include splatmap data
                materialJSON += "}}"; // Close the JSON data

                JObject matJSON = JObject.Parse(materialJSON);

                if (adtfile.textures.filenames.Length == 0)
                {
                    fullJSON += "\"id0\":\"null\",";
                }
                else
                {
                    for (int q = 0; q < adtfile.textures.filenames.Length; q++)
                    {
                        string textureFile = adtfile.textures.filenames[q].ToLower();

                        if (Managers.ConfigurationManager.ADTPreserveTextureStruct)
                        {
                            textureFile = textureFile.Replace("\\", "\\\\").Replace(".blp", ""); //Remove extension and modify the path to use "\\" instead of "\"
                        }
                        else
                        {
                            textureFile = Path.GetFileNameWithoutExtension(textureFile);
                        }

                        fullJSON += "\"id" + q + "\":\"" + textureFile + "\",";
                    }
                }

                fullJSON = fullJSON.Substring(0, fullJSON.Length - 1); // remove tailing comma
                fullJSON += "}}"; // Close the JSON data

                SplatmapJSON = fullJSON;

                #endregion
                //----------------------------------------------------------------------------------------------------------

                //----------------------------------------------------------------------------------------------------------
                //Generate the actual splatmaps
                //----------------------------------------------------------------------------------------------------------
                #region Splatmap Bitmap

                string[] materialIDs = adtfile.textures.filenames;
                for (int i = 0; i < materialIDs.Length; i++)
                {
                    if (Managers.ConfigurationManager.ADTPreserveTextureStruct)
                    {
                        materialIDs[i] = materialIDs[i].ToLower().Replace(".blp", ""); //Remove extension for the files
                    }
                    else
                    {
                        materialIDs[i] = Path.GetFileNameWithoutExtension(materialIDs[i].ToLower());
                    }
                }
                int imageCount = IntCeil(materialIDs.Length, 4);

                //----------------------------------------------------------------------------------------------------------
                //Structure for this abomination:
                //----------------------------------------------------------------------------------------------------------
                //>A int array for every map we need that contains:
                //>A int array for each channel (4 in total A R G B) that contains:
                //>A 2D array for each pixel (1024x1024)
                int[][][,] pixelData = new int[imageCount][][,];
                for (int p = 0; p < pixelData.Length; p++)
                {
                    pixelData[p] = new int[4][,];
                    for (int i = 0; i < 4; i++)
                    {
                        pixelData[p][i] = new int[1024, 1024];
                    }
                }
                //----------------------------------------------------------------------------------------------------------

                // Now before we draw each sub-chunk to PNG, we need to check it's texture list in json.
                // Based on what order the textures are for that sub-chunk, we may need to draw RGBA in a different order than 0,1,2,3

                //Chunk offset
                int xOff = 0;
                int yOff = 0;

                //Loop for all the 256 texChunks
                for (int chunkIndex = 0; chunkIndex < 256; chunkIndex++)
                {
                    TexMCNK texChunk = adtfile.texChunks[chunkIndex];
                    MCAL[] alphaLayers = texChunk.alphaLayer;
                    MCLY[] textureLayers = texChunk.layers;

                    // If there is no texture data just skip it
                    if (textureLayers.Length > 0)
                    {
                        // X,Y Loop through the texChunk (data is stored as 64x64)
                        for (int x = 0; x < 64; x++)
                        {
                            for (int y = 0; y < 64; y++)
                            {
                                int alphaIndex = x * 64 + y;

                                int numberTextureLayers = matJSON["chunkData"][chunkIndex.ToString()].Count();

                                for (int k = 0; k < numberTextureLayers; k++)
                                {
                                    // k = 1, random materialID. This could be any RGBA, RGBA color!
                                    int currentIndex = 0;
                                    string currentID = (string)matJSON["chunkData"][chunkIndex.ToString()][k]["id"]; //Probably not a good idea to use a string though (check back on >7xx support)

                                    for (int l = 0; l < materialIDs.Length; l++)
                                    {
                                        if (materialIDs[l] == currentID)
                                        {
                                            currentIndex = l;
                                        }
                                    }
                                    int texIndex = currentIndex;

                                    // Calculate image index, 1 PNG image for each 4 textures. index 0 includes base texture on channel 0
                                    int imageIndex = IntFloor(texIndex, 4);

                                    // 0-3 RGBA. If imageIndex=0 this should not be 0 because that is basetexture
                                    int channelIndex = texIndex % 4;

                                    // 'vec3 blendTex' from the adt.fragment shader
                                    var blendTexs = new int[3];
                                    if (alphaLayers != null) //Those layers can be null
                                    {
                                        switch (alphaLayers.Length)
                                        {
                                            case 2:
                                                blendTexs[0] = alphaLayers[1].layer[alphaIndex];
                                                break;
                                            case 3:
                                                blendTexs[0] = alphaLayers[1].layer[alphaIndex];
                                                blendTexs[1] = alphaLayers[2].layer[alphaIndex];
                                                break;
                                            case 4:
                                                blendTexs[0] = alphaLayers[1].layer[alphaIndex];
                                                blendTexs[1] = alphaLayers[2].layer[alphaIndex];
                                                blendTexs[2] = alphaLayers[3].layer[alphaIndex];
                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    // 'vec4 layer_weights' from the adt.fragment shader
                                    var sumBlendTex = 0;
                                    for (int b = 0; b < blendTexs.Length; b++)
                                    {
                                        sumBlendTex += blendTexs[b];
                                    }
                                    sumBlendTex = Clamp(sumBlendTex, 0, 255);
                                    int[] layerWeights = new int[4];
                                    layerWeights[0] = 255 - sumBlendTex;
                                    layerWeights[1] = blendTexs[0];
                                    layerWeights[2] = blendTexs[1];
                                    layerWeights[3] = blendTexs[2];

                                    // Write the actual pixel data
                                    if (k == 0)
                                    {
                                        pixelData[imageIndex][channelIndex][x + xOff, y + yOff] = layerWeights[0];
                                    }
                                    else
                                    {
                                        pixelData[imageIndex][channelIndex][x + xOff, y + yOff] = layerWeights[k];
                                    }

                                }
                            }
                        }
                    }
                    //----------------------------------------------------------------------------------------------------------
                    //Change the offset
                    //----------------------------------------------------------------------------------------------------------
                    if (yOff + 64 > 960)
                    {
                        yOff = 0;
                        if (xOff + 64 <= 960)
                        {
                            xOff += 64;
                        }
                    }
                    else
                    {
                        yOff += 64;
                    }
                    //----------------------------------------------------------------------------------------------------------
                }

                //----------------------------------------------------------------------------------------------------------
                //Generate the bitmaps
                //----------------------------------------------------------------------------------------------------------
                for (int t = 0; t < imageCount; t++)
                {
                    Bitmap bmp = new Bitmap(1024, 1024);

                    for (int x = 0; x < 1024; x++)
                    {
                        for (int y = 0; y < 1024; y++)
                        {
                            Color currentColor = Color.FromArgb(
                                ZeroClamp(pixelData[t][3][x, y]),//A
                                ZeroClamp(pixelData[t][0][x, y]),//R
                                ZeroClamp(pixelData[t][1][x, y]),//G
                                ZeroClamp(pixelData[t][2][x, y]) //B 
                                );

                            bmp.SetPixel(x, y, currentColor);
                        }
                    }

                    //----------------------------------------------------------------------------------------------------------
                    //Fix bmp orientation:
                    //----------------------------------------------------------------------------------------------------------
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipY);
                    //----------------------------------------------------------------------------------------------------------

                    //----------------------------------------------------------------------------------------------------------
                    //Store the generated map in the list
                    //----------------------------------------------------------------------------------------------------------
                    AlphaLayers.Add(bmp);
                    //----------------------------------------------------------------------------------------------------------
                }
                //----------------------------------------------------------------------------------------------------------

                #endregion
                //----------------------------------------------------------------------------------------------------------
            }
            #endregion
        }

        private int IntCeil(int value, int divisor)
        {
            return ((value / divisor) + (value % divisor == 0 ? 0 : 1));
        }

        private int IntFloor(int value, int divisor)
        {
            return (value / divisor);
        }

        private int ZeroClamp(int x)
        {
            if (x < 0)
                return 0;

            return x;
        }

        private int Clamp(int val, int min, int max)
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }
}