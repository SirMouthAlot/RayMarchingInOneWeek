using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CameraMarchingMaster : MonoBehaviour {

    // First we need a compute shader 
    public ComputeShader raymarching;

    // this is where we will be storing the results of our ray marching
    RenderTexture target;

    // this is our unity camera 
    Camera cam;

    // we need to clean up each frame 
    List<ComputeBuffer> buffersToDispose;

    // we will need a light position
    Light lightSource;

    //////////////
    void Init () {
        cam = Camera.current;
        buffersToDispose = new List<ComputeBuffer> ();
        lightSource = FindObjectOfType<Light> ();

    }

    // this is called when we need to re-render
    void OnRenderImage (RenderTexture source, RenderTexture destination) {
        Init ();
        InitRenderTexture ();
        CreateScene ();
        SetParameters ();

        // setup our pointers to variables in the compute shader
        raymarching.SetTexture (0, "Source", source);
        raymarching.SetTexture (0, "Destination", target);

        // tell the compute shader to start
        int threadGroupsX = Mathf.CeilToInt (cam.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt (cam.pixelHeight / 8.0f);
        raymarching.Dispatch (0, threadGroupsX, threadGroupsY, 1);

        // draw the resulting texture to the screen
        Graphics.Blit (target, destination);

        // cleanup
        foreach (var buffer in buffersToDispose) {
            buffer.Dispose ();
        }
    }

    // Create the scene
    // this just packs the data in the unity scene graph into a contiguous array 
    // so that we can send it to the compute shader
    void CreateScene () {
        // get all shapes in the scene graph
        List<Shape> allShapes = new List<Shape> (FindObjectsOfType<Shape> ());

        // create an array of shape data (guaranteed to be a contiguous buffer)
        ShapeData[] shapeData = new ShapeData[allShapes.Count];

        // go through and copy the data into the buffer
        for (int i = 0; i < allShapes.Count; i++) {
            var s = allShapes[i];
            Vector3 col = new Vector3 (s.colour.r, s.colour.g, s.colour.b);
            shapeData[i] = new ShapeData () {
	                position 	= s.Position,
	                scale 		= s.Scale, 
	                colour 		= col,
	                shapeType 	= (int) s.shapeType,
	                operation 	= (int) s.operation,
	                blendStrength = s.blendStrength*3,
            };
        }

        // create a compute buffer (StructuredBuffer) to send to the compute shader
        ComputeBuffer shapeBuffer = new ComputeBuffer (shapeData.Length, ShapeData.GetSize ());

        // this tells the compute shader where to find the data
        shapeBuffer.SetData (shapeData);
        raymarching.SetBuffer (0, "shapes", shapeBuffer);
        raymarching.SetInt ("numShapes", shapeData.Length);

        // for cleanup
        buffersToDispose.Add (shapeBuffer);
    }

    // this just sets some global variables in the computeShader
    void SetParameters () {
        raymarching.SetMatrix ("_CameraToWorld", cam.cameraToWorldMatrix);
        raymarching.SetMatrix ("_CameraInverseProjection", cam.projectionMatrix.inverse);
        raymarching.SetVector ("_LightPosition", lightSource.transform.position);
        raymarching.SetFloat  ("_LightIntensity", lightSource.intensity);
    }

    // create a texture target to render to
    void InitRenderTexture () {
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight) {
            if (target != null) {
                target.Release ();
            }
            target = new RenderTexture (cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create ();
        }
    }

    // definition of the actual shape data, this needs to be in the exact same ordering as in the computeShader
    struct ShapeData {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 colour;
        public int shapeType;
        public int operation;
        public float blendStrength;

        // this function returns the number of bytes, in our case above we have 10 floats and 2 ints
        public static int GetSize () {
            return sizeof (float) * 10 + sizeof (int) * 2;
        }
    }
}
