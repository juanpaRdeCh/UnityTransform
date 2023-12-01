using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformWheels : MonoBehaviour
{
    [SerializeField] Vector3 displacement;   // Translation displacement vector
    [SerializeField] float displacementAngle; // Angle for rotation
    [SerializeField] AXIS rotationAXIS;      // Axis of rotation

    // Wheel models, these ones are imported from the scene with their own coordinates as gameObjects
    [SerializeField] GameObject wheel1;
    [SerializeField] GameObject wheel2;
    [SerializeField] GameObject wheel3;
    [SerializeField] GameObject wheel4;

    Mesh mesh;
    Vector3[] baseVertices;
    Vector3[] newVertices;

    [SerializeField] Wheel[] wheels;

    void Start()
    {
        mesh = GetComponentInChildren<MeshFilter>().mesh;
        baseVertices = mesh.vertices;

        newVertices = new Vector3[baseVertices.Length];
        for (int i = 0; i < baseVertices.Length; i++)
        {
            newVertices[i] = baseVertices[i];
        }

        wheels[0].gameObject = wheel1;
        wheels[1].gameObject = wheel2;
        wheels[2].gameObject = wheel3;
        wheels[3].gameObject = wheel4;

        // Initialize the objects and store their base properties such as mesh, position, and vertices
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].mesh = wheels[i].gameObject.GetComponentInChildren<MeshFilter>().mesh;
            wheels[i].baseVertices = wheels[i].mesh.vertices;
            wheels[i].newVertices = new Vector3[wheels[i].baseVertices.Length];
            // I commented this fragment of code because the wheels were applying the object coordinates on the original coordinates, moving them from
            // its original position 
            //wheels[i].position = wheels[i].gameObject.transform.position;
        }
    }

    float GetAngle(Vector3 displacement)
    {
        // Calculate angle based on the displacement vector 
        float angle = Mathf.Atan2(displacement.x, displacement.z) * Mathf.Rad2Deg;
        return angle;
    }

    void Update()
    {
        DoTransform();
    }

    void DoTransform()
    {
        displacementAngle = GetAngle(displacement);

        // Apply the transformation to the car body
        Matrix4x4 move = HW_Transforms.TranslationMat(displacement.x * Time.time,
                                                      displacement.y * Time.time,
                                                      displacement.z * Time.time);
        Matrix4x4 rotate = HW_Transforms.RotateMat(displacementAngle, rotationAXIS);
        Matrix4x4 composite = move * rotate;

        // Apply the transformation to the main object's vertices
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector4 temp = new Vector4(baseVertices[i].x,
                                       baseVertices[i].y,
                                       baseVertices[i].z, 1);
            newVertices[i] = composite * temp;
        }

        // Apply the transformation to each wheel, same translation matrices as we learnt in class
        for (int i = 0; i < wheels.Length; i++)
        {
            Matrix4x4 initialPosMatrix = HW_Transforms.TranslationMat(wheels[i].position.x,
                                                                      wheels[i].position.y,
                                                                      wheels[i].position.z);
            Matrix4x4 rotateWheel = HW_Transforms.RotateMat(wheels[i].rotation * Time.time, AXIS.X);
            Matrix4x4 wheelComposite = composite * initialPosMatrix * rotateWheel;

            for (int j = 0; j < wheels[i].baseVertices.Length; j++)
            {
                Vector4 temp = new Vector4(wheels[i].baseVertices[j].x,
                                           wheels[i].baseVertices[j].y,
                                           wheels[i].baseVertices[j].z, 1);
                wheels[i].newVertices[j] = wheelComposite * temp;
            }

            // Update the mesh and recalculate normals for each wheel
            wheels[i].mesh.vertices = wheels[i].newVertices;
            wheels[i].mesh.RecalculateNormals();
            wheels[i].mesh.RecalculateBounds();
        }

        // Update the mesh and recalculate normals for the car body
        mesh.vertices = newVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}

[System.Serializable]
public class Wheel
{
    // Initialize the wheel object with the basic properties
    public GameObject gameObject;
    public Mesh mesh;
    public Vector3[] baseVertices;
    public Vector3[] newVertices;
    [SerializeField] public Vector3 position;
    [SerializeField] public float rotation;

    public Wheel(Vector3 pos)
    {
        position = pos;
    }
}
