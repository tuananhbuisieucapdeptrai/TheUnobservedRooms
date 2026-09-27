using System.Collections.Generic;
using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public static class ProceduralAssetFactory
    {
        public static GameObject Crystal(string name, Transform parent, float radius, float height, int sides = 6)
        {
            var vertices = new List<Vector3> { new(0,height * .5f,0), new(0,-height * .5f,0) };
            for (var i = 0; i < sides; i++)
            {
                var a = Mathf.PI * 2f * i / sides; vertices.Add(new Vector3(Mathf.Cos(a) * radius,0,Mathf.Sin(a) * radius));
            }
            var triangles = new List<int>();
            for (var i = 0; i < sides; i++)
            {
                var a = 2 + i; var b = 2 + (i + 1) % sides;
                triangles.AddRange(new[] { 0,a,b, 1,b,a });
            }
            return MeshObject(name,parent,vertices,triangles);
        }

        public static GameObject Prism(string name, Transform parent, float radius, float height, int sides = 8)
        {
            var vertices = new List<Vector3>();
            for (var y = 0; y < 2; y++) for (var i = 0; i < sides; i++)
            {
                var a = Mathf.PI * 2f * i / sides; vertices.Add(new Vector3(Mathf.Cos(a) * radius,y == 0 ? -height*.5f : height*.5f,Mathf.Sin(a) * radius));
            }
            var triangles = new List<int>();
            for (var i = 0; i < sides; i++)
            {
                var n = (i + 1) % sides; triangles.AddRange(new[] { i,n,sides+n, i,sides+n,sides+i });
            }
            for (var i = 1; i < sides - 1; i++) triangles.AddRange(new[] { 0,i+1,i, sides,sides+i,sides+i+1 });
            return MeshObject(name,parent,vertices,triangles);
        }

        public static GameObject Veil(string name, Transform parent, float width, float height, int columns = 8, int rows = 10)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (var y = 0; y <= rows; y++) for (var x = 0; x <= columns; x++)
            {
                var px = ((float)x / columns - .5f) * width; var py = ((float)y / rows - .5f) * height;
                var depth = Mathf.Sin(x * 1.73f + y * 2.11f) * .045f; vertices.Add(new Vector3(px,py,depth));
            }
            for (var y = 0; y < rows; y++) for (var x = 0; x < columns; x++)
            {
                var i = y * (columns + 1) + x; var n = i + columns + 1;
                triangles.AddRange(new[] { i,n,i+1, i+1,n,n+1 });
            }
            return MeshObject(name,parent,vertices,triangles);
        }

        public static GameObject SurveyorBody(string name, Transform parent)
        {
            const int sides = 7; var vertices = new List<Vector3>(); var triangles = new List<int>();
            var heights = new[] { -1.35f,-.55f,.35f,1.25f }; var radii = new[] { .38f,.7f,.55f,.22f };
            for (var r = 0; r < heights.Length; r++) for (var i = 0; i < sides; i++)
            {
                var a = Mathf.PI * 2f * i / sides + r * .22f;
                vertices.Add(new Vector3(Mathf.Cos(a)*radii[r],heights[r],Mathf.Sin(a)*radii[r]));
            }
            for (var r = 0; r < heights.Length - 1; r++) for (var i = 0; i < sides; i++)
            {
                var a = r*sides+i; var b = r*sides+(i+1)%sides; var c = (r+1)*sides+i; var d = (r+1)*sides+(i+1)%sides;
                triangles.AddRange(new[] { a,c,b, b,c,d });
            }
            return MeshObject(name,parent,vertices,triangles);
        }

        private static GameObject MeshObject(string name, Transform parent, List<Vector3> vertices, List<int> triangles)
        {
            var item = new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); item.transform.SetParent(parent,false);
            var mesh = new Mesh { name = name + "Mesh" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            var uv = new List<Vector2>(vertices.Count); foreach (var vertex in vertices) uv.Add(new Vector2(vertex.x + .5f,vertex.y + .5f)); mesh.SetUVs(0,uv);
            item.GetComponent<MeshFilter>().sharedMesh = mesh; return item;
        }
    }
}
