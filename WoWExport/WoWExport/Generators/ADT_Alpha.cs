using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using WoWFormatLib.Structs.ADT;
namespace Generators.ADT_Alpha
{
    class ADT_Alpha
    {
        //-----------------------------------------------------------------------------------------------------------------
        //PUBLIC STUFF:
        //-----------------------------------------------------------------------------------------------------------------
        public List<Bitmap> AlphaLayers = new List<Bitmap>();
        public List<String> AlphaLayersNames = new List<String>();
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
                                    //var color = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
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
                                    //var color = System.Drawing.Color.FromArgb(values[x * 64 + y], 0, 0, 0); //for pure black generation
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
                                    //var color = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
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
                            AlphaLayerName = AlphaLayerName.Substring(AlphaLayerName.LastIndexOf("\\", AlphaLayerName.Length - 2) + 1);
                            AlphaLayerName = AlphaLayerName.Substring(0, AlphaLayerName.Length - 4);
                            //AlphaLayersNames.Add(AlphaLayerName + "_" + c + "_" + li);
                            //AlphaLayersNames.Add(c + "_" + li + "_" + AlphaLayerName);
                            AlphaLayersNames.Add(c + ";" + li + ";" + AlphaLayerName);
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
        }
    }
}
