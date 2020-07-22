using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CategoryVisualisation : MonoBehaviour
{
    public Material polygonMat;

    public Transform[] trianglePoints;                          // Stores a reference to empty game objects marking the tips of the graph's triangle

    Mesh polygonMesh;                                           // Will reneder our polygon on screen
    Vector3[] verts;                                            // Will store the vertices of our custom mesh

    enum CategoryIndex                                          // Just makes things a little more readable in code later. 0 = Mobility. 1 = Offense. 2 = Defense.
    {
        MobilityIndex,
        OffenseIndex,
        DefenseIndex,
        CentrePointIndex
    }

    // Start is called before the first frame update
    void Start()
    {
        polygonMesh = new Mesh();                               // initialise a customisable mesh
        verts = new Vector3[3];                                 // initialise the vertex array

        /*verts[0] = new Vector3(0, 0, 0);
        verts[1] = new Vector3(0, 5, 0);
        verts[2] = new Vector3(1, 4, 0);

        polygonMesh.vertices = verts;                           // Assign this new array to our custom mesh's vertices
        polygonMesh.triangles = new int[] { 0, 1, 2 };          // Tell Unity which indeces in the vertex array make up triangles (we only have 1 triangle).
                                                                // This makes the triangle of verteces filled (with the material we'll assign soon)
        */

        UpdateAlchemyPolygon();

        GetComponent<MeshRenderer>().material = polygonMat;     // Here we give it a material so we can customise the colour and texture
        GetComponent<MeshFilter>().mesh = polygonMesh;          // And here we finally assign it to the mesh filter component - telling Unity to draw it in the scene.
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAlchemyPolygon();
    }

    /// <summary>
    /// Takes the current Category values (Offense, Defense, Mobility) and updates the visualisation on our DA Scale
    /// </summary>
    public void UpdateAlchemyPolygon()
    {
        // temp variables to streamline referencing. Only live as long as the function.
        float mobilityLevel = MusicControllerDA.musicControllerInstance.GetMobilityLevel();
        float offenseLevel =  MusicControllerDA.musicControllerInstance.GetOffenseLevel();
        float defenseLevel =  MusicControllerDA.musicControllerInstance.GetDefenseLevel();
        float categoryMax =   MusicControllerDA.musicControllerInstance.categoryMax;

        
        // Here, we calculate the percentage that each category level represents out of its possible max value.
        // We then use a Lerp to place our vertex that percentage between the centre and the max point of our triangle graph.
        verts[0] = Vector3.Lerp(trianglePoints[(int)CategoryIndex.CentrePointIndex].localPosition, 
                                trianglePoints[(int)CategoryIndex.MobilityIndex].localPosition, (mobilityLevel / categoryMax));
        verts[1] = Vector3.Lerp(trianglePoints[(int)CategoryIndex.CentrePointIndex].localPosition,
                                trianglePoints[(int)CategoryIndex.OffenseIndex].localPosition, (offenseLevel / categoryMax));
        verts[2] = Vector3.Lerp(trianglePoints[(int)CategoryIndex.CentrePointIndex].localPosition,
                                trianglePoints[(int)CategoryIndex.DefenseIndex].localPosition, (defenseLevel / categoryMax));

        polygonMesh.vertices = verts;                           // Assign this new array to our custom mesh's vertices
        polygonMesh.triangles = new int[] { 0, 1, 2 };          // Tell Unity which indeces in the vertex array make up triangles (we only have 1 triangle).

        // Update polygon colour to a mix of primary colours based on the three category levels
        polygonMat.color = new Color((offenseLevel / categoryMax), (mobilityLevel / categoryMax), (defenseLevel / categoryMax), 0.65f);
    }
}
