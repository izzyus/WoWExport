﻿/*
        DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
                    Version 2, December 2004

 Copyright (C) 2004 Sam Hocevar <sam@hocevar.net>

 Everyone is permitted to copy and distribute verbatim or modified
 copies of this license document, and changing it is allowed as long
 as the name is changed.

            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION

  0. You just DO WHAT THE FUCK YOU WANT TO.
*/
using System;
using WoWFormatLib.Utils;

namespace WoWFormatLib.Structs.M2
{
    public struct M2Model
    {
        public uint version;

        public int physFileID;
        public int skelFileID;

        public int[] boneFileDataIDs;
        public int[] skinFileDataIDs;
        public int[] lod_skinFileDataIDs;
        public AFID[] animFileData;

        public string filename;
        public string name;
        public GlobalModelFlags flags;
        public Sequence[] sequences;
        public Animation[] animations;
        public AnimationLookup[] animationlookup;
        public Bone[] bones;
        public KeyBoneLookup[] keybonelookup;
        public Vertice[] vertices;
        public uint nViews;
        public SKIN.SKIN[] skins;
        public Color[] colors;
        public Texture[] textures;
        public Transparency[] transparency;
        public UVAnimation[] uvanimations;
        public TexReplace[] texreplace;
        public RenderFlag[] renderflags;
        public BoneLookupTable[] bonelookuptable;
        public TexLookup[] texlookup;

        //unk1
        public TransLookup[] translookup;

        public UVAnimLookup[] uvanimlookup;
        public Vector3[] vertexbox;
        public float vertexradius;
        public Vector3[] boundingbox;
        public float boundingradius;
        public BoundingTriangle[] boundingtriangles;
        public BoundingVertex[] boundingvertices;
        public BoundingNormal[] boundingnormals;
        public Attachment[] attachments;
        public AttachLookup[] attachlookup;
        public Event[] events;
        public Light[] lights;
        public Camera[] cameras;
        public CameraLookup[] cameralookup;
        public RibbonEmitter[] ribbonemitters;
        public ParticleEmitter[] particleemitters;
    }

    /* Retrieved from https://wowdev.wiki/M2 */
    [Flags]
    public enum GlobalModelFlags : uint
    {
        Flag_TiltX                      = 0x1,
        Flag_TiltY                      = 0x2,
        Flag_0x4                        = 0x4,
        Flag_UseTextureCombinerCombos   = 0x8,
        Flag_0x10                       = 0x10,
        Flag_LoadPhysData               = 0x20,
        Flag_0x40                       = 0x40,
        Flag_0x80                       = 0x80,     // set on all models since cata alpha
        Flag_CameraRelated              = 0x100,
        Flag_NewParticleRecord          = 0x200,
        Flag_0x400                      = 0x400,
        Flag_TextureTransUseBoneSeq     = 0x800,    // When set, texture transforms are animated using the sequence being played on the bone found by index in tex_unit_lookup_table[textureTransformIndex], instead of using the sequence being played on the model's first bone. Example model: 6DU_HellfireRaid_FelSiege03_Creature
        Flag_0x1000                     = 0x1000,
        Flag_0x2000                     = 0x2000,   // seen in various legion models
        Flag_0x4000                     = 0x4000,
        Flag_0x8000                     = 0x8000,   // seen in UI_MainMenu_Legion
        Flag_0x10000                    = 0x10000,
        Flag_0x20000                    = 0x20000,
        Flag_0x40000                    = 0x40000,
        Flag_0x80000                    = 0x80000,
        Flag_0x100000                   = 0x100000,
        Flag_0x200000                   = 0x200000  // apparently: use 24500 upgraded model format: chunked .anim files, change in the exporter reordering sequence+bone blocks before name
    }

    [Flags]
    public enum TextureFlags
    {
        Flag_0x1_WrapX = 0x1,
        Flag_0x2_WrapY = 0x2,
    }
    public struct Sequence
    {
        public uint timestamp;
    }

    public struct ParticleEmitter
    {
        //needs filling in
    }

    public struct AFID
    {
        public short animID;
        public short subAnimID;
        public uint fileDataID;
    }


    [Flags]
    public enum AnimFlags : uint
    {
        Flag_0x1                        = 0x1,
        Flag_0x2                        = 0x2,
        Flag_0x4                        = 0x4,
        Flag_0x8                        = 0x8,
        Flag_0x10                       = 0x10,
        Flag_0x20                       = 0x20,
        Flag_0x40                       = 0x40,
        Flag_0x80                       = 0x80,
        Flag_0x100                      = 0x100,
        Flag_0x200                      = 0x200,
        Flag_0x400                      = 0x400,
        Flag_0x800                      = 0x800,
    }

    public struct Animation
    {
        public ushort animationID;
        public ushort subAnimationID;
        public uint length;
        public float movingSpeed;
        public AnimFlags flags;
        public short probability;
        public ushort unused;
        public uint unk1;
        public uint unk2;
        public uint playbackSpeed;
        public Vector3 minimumExtent;
        public Vector3 maximumExtent;
        public float boundsRadius;
        public short nextAnimation;
        public ushort index;
    }

    public struct AnimationLookup
    {
        public ushort animationID;
    }

    public struct Bone
    {
        public int boneId;
        public uint flags;
        public short parentBone;
        private ushort unk_0;
        private ushort unk_1;
        private ushort unk_2;
        public ABlock<Vector3> translation;
        public ABlock<Quaternion> rotation;
        public ABlock<Vector3> scale;
        public Vector3 pivot;
    }

    public struct UVAnimation
    {
        public ABlock<Vector3> translation;
        public ABlock<Quaternion> rotation;
        public ABlock<Vector3> scaling;
    }

    public struct Transparency
    {
        public ABlock<short> alpha;
    }

    public struct Color
    {
        public ABlock<RGBColor> color;
        public ABlock<short> alpha;
    }

    public struct Vertice
    {
        public Vector3 position;
        public byte boneWeight_0;
        public byte boneWeight_1;
        public byte boneWeight_2;
        public byte boneWeight_3;
        public byte boneIndices_0;
        public byte boneIndices_1;
        public byte boneIndices_2;
        public byte boneIndices_3;
        public Vector3 normal;
        public float textureCoordX;
        public float textureCoordY;
        public float textureCoordX2;
        public float textureCoordY2;
    }

    public struct AttachLookup
    {
        public ushort attachment;
    }

    public struct Attachment
    {
        public uint id;
        public uint bone;
        public Vector3 position;
        public ABlock<int> data;
    }

    public struct BoundingNormal
    {
        public Vector3 normal;
    }

    public struct BoundingVertex
    {
        public Vector3 vertex;
    }

    public struct BoundingTriangle
    {
        public ushort index_0;
        public ushort index_1;
        public ushort index_2;
    }

    public struct Camera
    {
        public uint type;
        public float farClipping;
        public float nearClipping;
        public ABlock<CameraPosition> translationPos;
        public Vector3 position;
        public ABlock<CameraPosition> translationTar;
        public Vector3 target;
        public ABlock<Vector3> scaling;
        public ABlock<float> unkABlock;
    }

    public struct CameraLookup
    {
        public ushort cameraID;
    }

    public struct CameraPosition
    {
        public float CameraPos_0;
        public float CameraPos_1;
        public float CameraPos_2;
        public float CameraPos_3;
        public float CameraPos_4;
        public float CameraPos_5;
        public float CameraPos_6;
        public float CameraPos_7;
        public float CameraPos_8;
    }

    public struct Event
    {
        public char identifier_0;
        public char identifier_1;
        public char identifier_2;
        public char identifier_3;
        public uint data;
        public uint bone;
        public Vector3 position;
        public ushort interpolationType;
        public ushort GlobalSequence;
        public uint nTimestampEntries;
        public uint ofsTimestampList;
    }

    public struct Light
    {
        public short type;
        public short bone;
        public Vector3 position;
        public ABlock<RGBColor> ambientColor;
        public ABlock<float> ambientIntensity;
        public ABlock<RGBColor> diffuseColor;
        public ABlock<float> diffuseIntensity;
        public ABlock<int> attenuationStart;
        public ABlock<int> attenuationEnd;
        public ABlock<int> unk;
    }

    public struct TexReplace
    {
        public short textureID;
    }

    public struct BoneLookupTable
    {
        public ushort bone;
    }

    public struct KeyBoneLookup
    {
        public ushort bone;
    }

    public struct TexLookup
    {
        public ushort textureID;
    }

    public struct TransLookup
    {
        public ushort transparencyID;
    }

    public struct UVAnimLookup
    {
        public ushort animatedTextureID;
    }

    public struct RenderFlag
    {
        public ushort flags;
        public ushort blendingMode;
    }

    public struct RibbonEmitter
    {
        public uint unk;
        public uint boneID;
        public Vector3 position;
        public int nTextures;
        public int ofsTextures;
        public int nBlendRef;
        public int ofsBlendRef;
        public ABlock<RGBColor> color;
        public ABlock<short> opacity;
        public ABlock<int> above;
        public ABlock<int> below;
        public float resolution;
        public float length;
        public float emissionAngle;
        public short renderFlags;
        public ABlock<short> unkABlock;
        public ABlock<bool> unkABlock2;
        public int unk2;
    }

    public struct Texture
    {
        public uint type;
        public TextureFlags flags;
        public string filename;
    }
}
