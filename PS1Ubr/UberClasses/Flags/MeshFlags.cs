using System;

namespace PS1Ubr.UberClasses
{
    [Flags]
    public enum MeshFlags
    {
        MESHSYS_FLAG_PORTALSYSTEM = 0x1, //Seen
        MESHSYS_FLAG_VEC3ARRAY = 0x4, //Seen
        MESHSYS_FLAG_SKELETONS = 0x8, //Seen
        MESHSYS_FLAG_USERDATA = 0x20,
        //40
        MESHSYS_FLAG_COLLISION = 0x80, //Seen
        MESHSYS_FLAG_NO_AAB = 0x2000,
        MESHSYS_FLAG_MAP_GEOMETRY = 0x4000, //Seen
                                            /*
                                            lava09_0
                                            lava09_1
                                            lava09_10
                                            lava09_2
                                            lava09_3
                                            lava09_7
                                            lava09_8
                                            lava09_9
                                            */
        MESHSYS_FLAG_HAS_EF = 0x8000, //Set when skeletons contain something starting with "ef_"
        MESHSYS_FLAG_UNK2 = 0x10000, //LARGE_ASSET?
        MESHSYS_FLAG_HAS_ANIM = 0x20000,
        MESHSYS_FLAG_HAS_EFFECT = 0x40000,
    }
}
