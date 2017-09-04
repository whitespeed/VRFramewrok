using System;
using System.Collections.Generic;
using UnityEngine;
namespace Framework
{
    public class TextureMemoryCache
    {
        protected Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public bool Exists(string url)
        {
            return textures.ContainsKey(url);
        }

        public void Add(string url, Texture texture)
        {
            if (!textures.ContainsKey(url))
                textures.Add(url, texture);
        }

        public Texture Get(string url)
        {
            return textures[url];
        }
    }
}