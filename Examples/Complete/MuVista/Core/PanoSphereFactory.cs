using Fusee.Base.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Examples.MuVista.Core;
using Fusee.Math.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

public class PanoSphereFactory
{
    private static string _pathToImageData = ".\\net6.0\\Assets\\Data\\output\\data.json";
    private static string _debugPathToImageData = "..\\net6.0\\Assets\\Data\\output\\data.json";
    //G:\\Projects\\Fusee\\Examples\\Complete\\JSONReducer\\output\\data.json
    private static string pathToMeta = PtRenderingParams.Instance.PathToOocFile + "\\meta.json";
    private static double3 offset;
    private static double3 center;

    private static string pathToImageData
    {
        get
        {
            if (Array.Exists(Environment.GetCommandLineArgs(), element => element == "useDebugPaths"))
                return _debugPathToImageData;
            else
                return _pathToImageData;
        }
    }
    public static List<PanoSphere> createPanoSpheres()
    {
        readJSONMetaData();

        var panoImages = readJSONImageData();
        List<PanoSphere> panoSpheres = new List<PanoSphere>();
        foreach (PanoImage img in panoImages)
        {
            panoSpheres.Add(createSphereWithShift(img));
        }
        for (int i = 0; i < panoSpheres.Count; i++)
        {
            if (i != 0)
            {
                panoSpheres[i].previous = panoSpheres[i - 1];
                panoSpheres[i].Children.Add(createArrow(panoSpheres[i].sphereTransform.Translation, panoSpheres[i].previous.sphereTransform.Translation, i, panoSpheres[i].radius,panoSpheres[i].sphereTransform.Rotation.y));
            }
            if (i != panoSpheres.Count - 1)
            {
                panoSpheres[i].next = panoSpheres[i + 1];
                panoSpheres[i].Children.Add(createArrow(panoSpheres[i].sphereTransform.Translation, panoSpheres[i].next.sphereTransform.Translation, i, panoSpheres[i].radius, panoSpheres[i].sphereTransform.Rotation.y));
            }
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
        var offsetX = (JValue)jsonObj["metaInfo"]["offsetX"];
        var offsetY = (JValue)jsonObj["metaInfo"]["offsetY"];
        var offsetZ = (JValue)jsonObj["metaInfo"]["offsetZ"];
        center = new double3((double)jsonCenter[0], (double)jsonCenter[1], (double)jsonCenter[2]);
        offset = new double3((double)offsetX, (double)offsetY, (double)offsetZ);
    }


    private static PanoSphere createSphereWithShift(PanoImage img)
    {
        PanoSphere sphere = new PanoSphere(img.filename);
        double shiftedX = shiftImgCoords(offset.x, img.X);
        double shiftedZ = shiftImgCoords(offset.y, img.Y);
        double shiftedY = shiftImgCoords(offset.z, img.Z);
        var fiber3dLadybugQuaternion_y = (float)img.qz;
        var fiber3dLadybugQuaternion_x = (float)img.qy * (-1);
        var fiber3dLadybugQuaternion_z = (float)img.qx * (-1);
        var fiber3dLadybugQuaternion_w = (float)img.qw * (-1);

    
        sphere.sphereTransform.Matrix = new Quaternion(fiber3dLadybugQuaternion_x, fiber3dLadybugQuaternion_y, fiber3dLadybugQuaternion_z, fiber3dLadybugQuaternion_w).ToRotMat();
        sphere.sphereTransform.Rotate(new float3(0, M.Pi, 0));
        sphere.sphereTransform.Translate(new float3(new double3(shiftedX, shiftedY, shiftedZ)));
        Diagnostics.Debug(sphere.sphereTransform.Rotation.y);
       
        return sphere;
    }

    private static double degreeToRadian(double val)
    {
        return (Math.PI / 180) * val;
    }

    private static double shiftImgCoords(double offset, double img)
    {
        return img - offset;
    }
    private static SceneNode createArrow(float3 pos, float3 nextPos, int i, float radius, float currentSphereRotationY)
    {
        SceneContainer blenderScene = AssetStorage.Get<SceneContainer>("arrow2.fus");
        SceneNode arrow = blenderScene.Children[0];

        arrow.Name = "connection" + i;
        float3 connectionVektor = new float3((float)(nextPos.x - pos.x), (float)(nextPos.y - pos.y), (float)(nextPos.z - pos.z));
        connectionVektor = float3.Rotate(new float3(0, -currentSphereRotationY, 0), connectionVektor);
        float angle = MathF.Atan2(1, 0) - MathF.Atan2(connectionVektor.z, connectionVektor.x);
        
        arrow.GetComponent<Transform>().Rotate(new float3((float)degreeToRadian(-10), angle , 0));
        arrow.GetComponent<Transform>(0).Translation = connectionVektor.Normalize() * (0.8f * radius);
        //arrow.GetComponent<Transform>(0).Translation.y -= 3;
        arrow.GetComponent<Transform>().Scale = new float3(0.8f, 0.8f, 0.8f);


        foreach (string s in Environment.GetCommandLineArgs())
        {
            Diagnostics.Debug(s);
        }

        return arrow;
    }
}
