using Fusee.Base.Core;
using Fusee.Examples.MuVista.Core;
using Fusee.Math.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

public class PanoSphereFactory
{
    private static string pathToImageData = "C:\\Users\\jacko\\OneDrive\\Desktop\\Hochschule\\MuVista\\Fusee\\Examples\\Complete\\JSONReducer\\output\\data.json";

    private static string pathToMeta = PtRenderingParams.Instance.PathToOocFile + "\\meta.json";
    private static double3 offset;
    private static double3 scale;
    private static double3 center;
    private static double size;

    public static List<PanoSphere> createPanoSpheres()
    {
        readJSONMetaData();

        var panoImages = readJSONImageData();
        List<PanoSphere> panoSpheres = new List<PanoSphere>();
        foreach (PanoImage img in panoImages)
        {
            panoSpheres.Add(createSphereWithShift(img));
        }
        return panoSpheres;
    }

    private static List<PanoImage> readJSONImageData()
    {
        string json = File.ReadAllText(pathToImageData);
        var panos = JsonConvert.DeserializeObject<List<PanoImage>>(json);
        return panos;
    }

    private static void readJSONMetaData()
    {
        JObject jsonObj;

        using (StreamReader sr = new StreamReader(pathToMeta))
        {
            jsonObj = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
        }

        var jsonCenter = (JArray)jsonObj["octree"]["rootNode"]["center"];
        var jsonSize = (JValue)jsonObj["octree"]["rootNode"]["size"];
        var offsetX = (JValue)jsonObj["metaInfo"]["offsetX"];
        var offsetY = (JValue)jsonObj["metaInfo"]["offsetY"];
        var offsetZ = (JValue)jsonObj["metaInfo"]["offsetZ"];
        var scaleX = (JValue)jsonObj["metaInfo"]["scaleX"];
        var scaleY = (JValue)jsonObj["metaInfo"]["scaleY"];
        var scaleZ = (JValue)jsonObj["metaInfo"]["scaleZ"];
        center = new double3((double)jsonCenter[0], (double)jsonCenter[1], (double)jsonCenter[2]);
        size = (double)jsonSize;
        offset = new double3((double)offsetX, (double)offsetY, (double)offsetZ);
        scale = new double3((double)scaleX, (double)scaleY, (double)scaleZ);
    }


    private static PanoSphere createSphereWithShift(PanoImage img)
    {
        PanoSphere sphere = new PanoSphere(img.filename);
        sphere.sphereTransform.Translation = new float3(new double3(center.x + img.X - offset.x, center.y + img.Y - offset.y, center.z + img.Z - offset.z));
        sphere.sphereTransform.Rotation = new float3(new double3(degreeToRadian(img.roll), degreeToRadian(img.pitch), degreeToRadian(img.heading)));
        //Diagnostics.Debug("imgX:" + (img.X - offset.x) + " | imgY" + (img.Y - offset.y) + " | imgZ" + (img.Z - offset.z));

        Diagnostics.Debug("roll:" + img.roll + " | pitch" + img.pitch + " | heading" + img.heading);

        return sphere;
    }

    private static double degreeToRadian(double val)
    {
        return (Math.PI / 180) * val;
    }
}
