using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{

    //Texture 计数
    public class TextureCounter
    {
        protected Dictionary<Texture, int> textureCounter = new Dictionary<Texture, int>();
        public bool CountDown(Texture texture)
        {
            if (null == texture)
                return false;
            if (!textureCounter.ContainsKey(texture)) return false;
            if (--textureCounter[texture] != 0) return false;
            textureCounter.Remove(texture);
            return true;
        }
        public void CountUp(Texture texture)
        {
            if (null == texture)
                return;
            if (textureCounter.ContainsKey(texture))
            {
                textureCounter[texture]++;
            }
            else
            {
                textureCounter.Add(texture, 1);
            }
        }
    }
}