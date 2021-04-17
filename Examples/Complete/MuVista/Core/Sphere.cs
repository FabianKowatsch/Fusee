using Fusee.Engine.Core.Scene;
using Fusee.Math.Core;
using System.Linq;

namespace Fusee.Examples.MuVista.Core
{
    public class Sphere : Mesh
    {
        public Sphere(float radius, int longSegments, int latSegments)
        {
            float3[] verts = new float3[(longSegments + 1) * latSegments];
            float3[] norms = new float3[(longSegments + 1) * latSegments];
            float2[] uvs = new float2[(longSegments + 1) * latSegments];
            ushort[] tris = new ushort[longSegments * (latSegments - 1) * 6];

            //Initialisierung der verts, norms und uvs
            for (int i = 0; i < latSegments; i++)
            {
                float theta = (float)M.Pi / (latSegments - 1) * i;

                for (int j = 0; j <= longSegments; j++)
                {
                    float phi = 2 * (float)M.Pi / (longSegments) * j;

                    if (i == 0)  //Nordpol
                    {
                        verts[j] = new float3(0, radius, 0);
                        norms[j] = -float3.UnitY;
                        uvs[j] = new float2((float)(j) / (float)(longSegments), 1);
                    }
                    else if (i == latSegments - 1)   //S�dpol
                    {
                        verts[(latSegments - 1) * (longSegments + 1) + j] = new float3(0, -radius, 0);
                        norms[(latSegments - 1) * (longSegments + 1) + j] = float3.UnitY;
                        uvs[(latSegments - 1) * (longSegments + 1) + j] = new float2((float)(j) / (float)(longSegments), 0);
                    }
                    else
                    {
                        if (j == longSegments)
                        {
                            phi = 2 * (float)M.Pi / (longSegments - 1) * 0;
                        }
                        verts[i * longSegments + j + i * 1] = new float3(
                            (float)(radius * M.Sin(phi) * M.Sin(theta)),
                            (float)(radius * M.Cos(theta)),
                            (float)(radius * M.Cos(phi) * M.Sin(theta)));

                        norms[i * longSegments + j + i * 1] = new float3(
                            -(float)(radius * M.Sin(phi) * M.Sin(theta)),
                            -(float)(radius * M.Cos(theta)),
                            -(float)(radius * M.Cos(phi) * M.Sin(theta)));

                        uvs[i * longSegments + j + i * 1] = new float2(
                            (float)(j) / (float)(longSegments),
                            1 - ((float)(i) / (float)(latSegments - 1)));
                    }
                }
            }

            for (int j = 0, k = 0; j < (latSegments - 1) * longSegments; j++, k++)   //j f�r die Array-Indices und k ist ein Punkt in den aktuellen Dreiecken
            {
                if ((k + 1) % (longSegments + 1) == 0)
                {
                    k++;
                }

                tris[j * 6] = (ushort)(k);
                tris[j * 6 + 1] = (ushort)(k + (longSegments + 1) + 1);
                tris[j * 6 + 2] = (ushort)(k + 1);

                tris[j * 6 + 3] = (ushort)(k);
                tris[j * 6 + 4] = (ushort)(k + (longSegments + 1));
                tris[j * 6 + 5] = (ushort)(k + (longSegments + 1) + 1);

            }

            Vertices = verts.ToArray();
            Normals = norms.ToArray();
            Triangles = tris.ToArray();
            UVs = uvs.ToArray();
        }
    }
}
