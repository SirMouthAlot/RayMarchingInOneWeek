using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is just a simple shape that contains a bit of information on blending operation and type for our compute shader
public class Shape : MonoBehaviour
{
    public enum ShapeType {Sphere,Cube,Torus,Ellipsoid};
    public enum Operation {None, Blend, Cut,Mask}

    public ShapeType shapeType;
    public Operation operation;
    public Color colour = Color.white;

    [Range(0,1)]
    public float blendStrength = 0.5f;
    public bool isAnimated = false;

    [Range(0,0.5f)]
    public float animX=0.0f;
    [Range(0,0.5f)]
    public float animY=0.01f;
    [Range(0,0.5f)]
    public float animZ=0;

    public Vector3 Position {
        get {
            return transform.position;
        }
    }

    public Vector3 Scale {
        get {
            return transform.localScale;
        }
    }
    void Update(){
        if(isAnimated){
            transform.position += new Vector3(animX*Mathf.Cos(Time.time),0.01f*Mathf.Sin(Time.time),animZ*Mathf.Sin(Time.time*2.0f));
        }   
    }
}
