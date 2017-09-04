using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("GameObject/UI/Polygon Image")]
    public class PolygonImage : Image
    {

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            if (type != Type.Simple)
            {
                return;
            }
            if (overrideSprite == null || overrideSprite.triangles.Length == 6)
            {
                return;
            }
            if (vh.currentVertCount != 4)
            {
                return;
            }
            var vertice = new UIVertex();
            vh.PopulateUIVertex(ref vertice, 0);
            Vector2 lb = vertice.position;
            vh.PopulateUIVertex(ref vertice, 2);
            Vector2 rt = vertice.position;


            var len = sprite.vertices.Length;
            var vertices = new List<UIVertex>(len);
            Vector2 Center = sprite.bounds.center;
            var invExtend = new Vector2(1/sprite.bounds.size.x, 1/sprite.bounds.size.y);
            for (var i = 0; i < len; i++)
            {
                vertice = new UIVertex();
                // normalize
                var x = (sprite.vertices[i].x - Center.x)*invExtend.x + 0.5f;
                var y = (sprite.vertices[i].y - Center.y)*invExtend.y + 0.5f;
                // lerp to position
                vertice.position = new Vector2(Mathf.Lerp(lb.x, rt.x, x), Mathf.Lerp(lb.y, rt.y, y));
                vertice.color = color;
                vertice.uv0 = sprite.uv[i];
                vertices.Add(vertice);
            }


            len = sprite.triangles.Length;
            var triangles = new List<int>(len);
            for (var i = 0; i < len; i++)
            {
                triangles.Add(sprite.triangles[i]);
            }
            vh.Clear();
            vh.AddUIVertexStream(vertices, triangles);
        }
    }
}