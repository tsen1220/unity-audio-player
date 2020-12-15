using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectrumManager : MonoBehaviour
{
    public float[] spectrum = new float[8192];
    public LineRenderer upperLine;
    public LineRenderer lowerLine;
    public MeshFilter[] shells;

    private List<int> triangles = new List<int>();
    private AudioSource audioSource;
    private int lineInterval = 20;

    private const int ShellDepth = 200;
    private const int ShellWidth = 70;
    private List<float[]> shellTrack = new List<float[]>();
    private float shellRadius = 20;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        upperLine.positionCount = spectrum.Length / lineInterval;
        lowerLine.positionCount = spectrum.Length / lineInterval;

        for (int i = 0; i < ShellDepth; i++)
        {
            shellTrack.Add(new float[ShellWidth]);
        }

        SetMeshTriangles();
    }

    private void Update()
    {
        audioSource.GetSpectrumData(spectrum, 1, FFTWindow.BlackmanHarris);
        RendererLine();
        CaptureNewSamplesForTrack();
        if (shellTrack.Count > ShellDepth)
        {
            shellTrack.RemoveAt(0);
        }
        RenderMesh();
    }

    private void RendererLine()
    {
        float avgAngle = 180f / spectrum.Length * lineInterval * Mathf.Deg2Rad;
        List<Vector3> linePos = new List<Vector3>();

        for (int i = 0; i < upperLine.positionCount; i++)
        {
            Vector3 basePos = new Vector3(Mathf.Cos(avgAngle * i), Mathf.Sin(avgAngle * i), 0) * 50;
            basePos *= spectrum[i] * 100;
            linePos.Add(basePos);
        }
        upperLine.SetPositions(linePos.ToArray());
        lowerLine.SetPositions(linePos.ToArray());
    }

    private void SetMeshTriangles()
    {
        // Square Set
        for (int i = 0; i < ShellDepth - 1; i++)
        {
            for (int j = 0; j < ShellWidth - 1; j++)
            {
                int point1 = i * ShellWidth + j;
                int point2 = (i + 1) * ShellWidth + j;
                int point3 = (i + 1) * ShellWidth + (j + 1);
                int point4 = i * ShellWidth + (j + 1);

                triangles.Add(point1);
                triangles.Add(point2);
                triangles.Add(point4);
                triangles.Add(point2);
                triangles.Add(point3);
                triangles.Add(point4);
            }
        }
    }

    private void RenderMesh()
    {
        CaptureNewSamplesForTrack();
        Vector3[] vertices = new Vector3[ShellWidth * ShellDepth];
        Vector2[] uvs = new Vector2[ShellWidth * ShellDepth];
        float angle = 180f / (ShellWidth - 1) * Mathf.Deg2Rad;

        for (int i = ShellDepth - 1, k = 0; i >= 0; i--, k++)
        {
            for (int j = 0; j < ShellWidth; j++)
            {
                Vector3 baseVerticePos = new Vector3(Mathf.Cos(angle * j), Mathf.Sin(angle * j), 0);
                baseVerticePos *= shellRadius - (shellTrack[i][j] * 60);
                vertices[i * ShellWidth + j] = new Vector3(baseVerticePos.x, baseVerticePos.y, -k);

                if (i == ShellDepth - 1)
                {
                    Vector2 baseUvsPos = new Vector2((float)j / ShellWidth, 0);
                    uvs[i * ShellWidth + j] = baseUvsPos;
                }
                else
                {
                    float raw = (float)Mathf.Pow(i, 3) / Mathf.Pow(ShellDepth, 3) * shellTrack[i][j] * 20;
                    float strenth = Mathf.Clamp(raw, 0.01f, 0.99f);
                    Vector2 baseUvsPos = new Vector2((float)j / ShellWidth, strenth);
                    uvs[i * ShellWidth + j] = baseUvsPos;
                }
            }
        }

        for (int i = 0; i < shells.Length; i++)
        {
            shells[i].mesh.Clear();
            shells[i].mesh.SetVertices(new List<Vector3>(vertices)) ;
            shells[i].mesh.SetUVs(0 ,new List<Vector2>(uvs));
            shells[i].mesh.SetTriangles(triangles, 0);
            shells[i].mesh.RecalculateNormals();
        }
    }

    private void CaptureNewSamplesForTrack()
    {
        float[] foundation = Utils.Slice(spectrum, 10, 80);
        shellTrack.Add(foundation);
    }
}
