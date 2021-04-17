using Fusee.Engine.Core.Scene;
using Fusee.Math.Core;

namespace Fusee.Examples.MuVista.Core
{
    public class GridPlane : Mesh
    {
        public GridPlane(int longSegments, int latSegments, float height, float width, float distancePlane)
        {
            float3[] verts1 = new float3[(longSegments + 1) * latSegments];
            float3[] norms1 = new float3[(longSegments + 1) * latSegments];

            //Plane
            float rowHeight = height / (latSegments - 1);
            float columnWidth = width / longSegments;
            float startHeight = height / 2f;
            float startWidth = -width / 2f;

            //Initialisierung der verts und norms
            for (int i = 0; i < latSegments; i++)
            {
                for (int j = 0; j <= longSegments; j++)
                {
                    //Vektor wird um 45� um den Koordinatenursprung gedreht
                    float x = M.Cos(M.Pi) * (startWidth + (j * columnWidth)) + M.Sin(M.Pi) * distancePlane;
                    float y = startHeight - (i * rowHeight);
                    float z = -M.Sin(M.Pi) * (startWidth + (j * columnWidth)) + M.Cos(M.Pi) * distancePlane;
                    verts1[i * (longSegments + 1) + j] = new float3(x, y, z); //Add - to x and z value to mirror the image
                    norms1[i * (longSegments + 1) + j] = float3.UnitZ;
                }
            }

            Vertices1 = verts1;
            Normals1 = norms1;
        }
    }
}
