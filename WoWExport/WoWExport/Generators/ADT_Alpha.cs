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
            if (GenerationMode == 1 || GenerationMode == 2) //MODE 1 & 2 (ALL TEXTURES GET ONLY ONE ALPHA FOR ALL THE 256 CHUNKS (ALPHA SIZE = 1024x1024))
            {
                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///ALPHA MAPS TEST
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------



                for (int alphamap = 0; alphamap < adtfile.textures.filenames.Count(); alphamap++)
                {
                    //----------------------------------------------------------------------------------------------------------
                    //Get the name of the texture used in this alphamap and store it
                    //----------------------------------------------------------------------------------------------------------
                    var AlphaLayerName = adtfile.textures.filenames[alphamap];
                    AlphaLayerName = AlphaLayerName.Substring(AlphaLayerName.LastIndexOf("\\", AlphaLayerName.Length - 2) + 1);
                    AlphaLayerName = AlphaLayerName.Substring(0, AlphaLayerName.Length - 4);
                    AlphaLayersNames.Add(AlphaLayerName);

                    //----------------------------------------------------------------------------------------------------------
                    //Get the full path and texture name for a comparation down the line
                    //----------------------------------------------------------------------------------------------------------
                    var layername = adtfile.textures.filenames[alphamap];
                    //----------------------------------------------------------------------------------------------------------

                    int xOff = 0;
                    int yOff = 0;

                    var bmp = new System.Drawing.Bitmap(1024, 1024);

                    for (uint c = 0; c < adtfile.chunks.Count(); c++)
                    {
                        var chunk = adtfile.chunks[c];
                        for (int li = 0; li < adtfile.texChunks[c].layers.Count(); li++)
                        {
                            if (adtfile.texChunks[c].alphaLayer != null)
                            {
                                var values = adtfile.texChunks[c].alphaLayer[li].layer;
                                if (adtfile.textures.filenames[adtfile.texChunks[c].layers[li].textureId].ToLower() == layername.ToLower())
                                {
                                    for (int x = 0; x < 64; x++)
                                    {
                                        for (int y = 0; y < 64; y++)
                                        {
                                            var CurrentColor = new System.Drawing.Color();

                                            if (GenerationMode == 2) //ARGB MODE
                                            {

                                                switch (li)
                                                {
                                                    case 0:
                                                        CurrentColor = System.Drawing.Color.FromArgb(255, values[x * 64 + y], 0, 0);
                                                        break;
                                                    case 1:
                                                        CurrentColor = System.Drawing.Color.FromArgb(255, 0, values[x * 64 + y], 0);
                                                        break;
                                                    case 2:
                                                        CurrentColor = System.Drawing.Color.FromArgb(255, 0, 0, values[x * 64 + y]);
                                                        break;
                                                    case 3:
                                                        CurrentColor = System.Drawing.Color.FromArgb(values[x * 64 + y], 0, 0, 0);
                                                        break;
                                                }
                                                
                                            }
                                            else //FLAT MODE
                                            {
                                                CurrentColor = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
                                                //var color = System.Drawing.Color.FromArgb(values[x * 64 + y], 0, 0, 0); //for pure black generation
                                            }

                                            bmp.SetPixel(x + xOff, y + yOff, CurrentColor);
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
                                xOff = xOff + 64;
                            }
                        }
                        else
                        {
                            yOff = yOff + 64;
                        }
                        //----------------------------------------------------------------------------------------------------------
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



                    //----------------------------------------------------------------------------------------------------------
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ///ALPHA MAPS TEST END
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //----------------------------------------------------------------------------------------------------------

                }
            }
            else //METHOD 3 (ALL THE CHUNKS (256) GET AN ALPHA FOR EVERY USED TEXTURE (ALPHA SIZE = 64x64 ))
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
                                for (int x = 0; x < 64; x++)
                                {
                                    for (int y = 0; y < 64; y++)
                                    {
                                        var color = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
                                        //var color = System.Drawing.Color.FromArgb(values[x * 64 + y], 0, 0, 0); //for pure black generation
                                        bmp.SetPixel(x, y, color);
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
