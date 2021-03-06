﻿using System;

namespace PS1Ubr
{
    public class CPortalSystemPlane : ILoadable
    {
        public CVec3f Normal;
        public float Distance;
        public float E; //TODO

        public bool Load(ref CDynMemoryReader r, ref CUberData data)
        {
            float temp = 0;
            if (!r.Get(ref temp)) return false;
            Normal.X = temp;

            if (!r.Get(ref temp)) return false;
            Normal.Y = temp;

            if (!r.Get(ref temp)) return false;
            Normal.Z = temp;

            if (!r.Get(ref Distance)) return false;
            if (!r.Get(ref E)) return false;

            return true;
        }
    }
}
