using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class FieldCreator : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("UI References")]
    public TMP_InputField widthInputField;
    public TMP_InputField heightInputField;

    [Header("FieldElements")]
    public FieldPlatform platformPrefab;
    public GameObject startPoint;
    public GameObject endPoint;

    private int width = 0;
    private int height = 0;
    private float spacing = 0.25f;
    private float platformDim = 4.0f;

    //variables of created Field
    private List<Vector2Int> pathSpaces = new List<Vector2Int>();
    private List<Vector3> pathPointsPositions = new List<Vector3>();
    private FieldPlatform[][] platforms;
    public bool fieldInitialized;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void CreateFieldOfDimensions()
    {
        width = ParseInputText(widthInputField.text);
        height = ParseInputText(heightInputField.text);
        
        EliminatePreviousField();

        platforms = new FieldPlatform[width][];
        for (int i = 0; i < width; i++)
        {
            platforms[i] = new FieldPlatform[height];
        }
        fieldInitialized = true;


        DeterminePath();


        //create platforms
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++) {
                Vector2Int coord = new Vector2Int(i, j);
                if (!pathSpaces.Contains(coord))
                {
                    Vector3 pPos = CoordToPos(coord.x, coord.y);
                    platforms[i][j] = Instantiate<FieldPlatform>(platformPrefab, pPos, platformPrefab.transform.rotation);
                    platforms[i][j].coords = new Vector2Int(i, j);
                } else
                {
                    platforms[i][j] = null;
                }
            }
        }
    }

    public Vector3 CoordToPos(int i, int j)
    {
        Vector3 ans = Vector3.zero;
        ans.x = i * platformDim + spacing * i;
        ans.z = j * platformDim + spacing * j;
        return ans;
    }

    private int ParseInputText(string text)
    {
        int num;
        bool validParse = int.TryParse(text, out num );
        if(!validParse)
        {
            return 5;
        }
        if(num<5)
        {
            return 5;
        }
        return num;
    }

    public void EliminatePreviousField()
    {
        if(pathSpaces != null)
        {
            pathSpaces.Clear();
            pathPointsPositions.Clear();
        }

        if(platforms != null)
        {
            for(int i = 0; i < platforms.Length; i++)
            {
                for (int j = 0; j < platforms[i].Length; j++)
                {
                    if (platforms[i][j] != null)
                    {
                        Destroy(platforms[i][j].gameObject);
                    }
                }
            }
        }
        fieldInitialized = false;
    }

    private void DeterminePath()
    {
        bool left = true;

        //pathPoint to End (at left)
        Vector3 endPos = CoordToPos(1, 0);
        endPoint.transform.position = new Vector3(endPos.x, endPoint.transform.position.y, endPos.z);

        pathPointsPositions.Add(endPos);
        //Firt the rows
        for( int j = 1; j<height-1; j+=2)//Row per row (game view down to up)
        {
            for(int i = 1; i<width-1; i++)
            {
                pathSpaces.Add(new Vector2Int(i, j));
            }

            Vector3 leftPos = CoordToPos(1, j);
            Vector3 rightPos = CoordToPos(width-2, j);

            if (left)
            {
                //space in between path rows
                pathSpaces.Add(new Vector2Int(1, j-1));
                //add pathPoint left to right
                pathPointsPositions.Add(leftPos);
                pathPointsPositions.Add(rightPos);
            } else
            {
                //space in between path rows
                pathSpaces.Add(new Vector2Int(width-2,j - 1));
                //add pathPoint right to left
                pathPointsPositions.Add(rightPos);
                pathPointsPositions.Add(leftPos);   
            }

            left = !left;
        }

        //Adding spaces of the beginning
        Vector3 startPos;
        if(left)
        {
            pathSpaces.Add(new Vector2Int(1, height-1));
            startPos = CoordToPos(1, height - 1);
            if(height % 2 == 0)
            {
                pathSpaces.Add(new Vector2Int(1, height - 2));
            }
        } else
        {
            pathSpaces.Add(new Vector2Int(width - 2, height - 1));
            startPos = CoordToPos(width - 2, height - 1);
            if (height % 2 == 0)
            {
                pathSpaces.Add(new Vector2Int(width - 2, height - 2));
            }
        }
        startPoint.transform.position = new Vector3(startPos.x, startPoint.transform.position.y, startPos.z);

    }

    public List<Vector3> GetPathPointsPositions()
    {
        List<Vector3> postions = new List<Vector3>(pathPointsPositions);
        //reverse it because we add it in other order
        postions.Reverse();
        return postions;
    }

    public Vector2Int GetFieldDimension()
    {
        return new Vector2Int(width, height);
    }

    public Vector2 GetFieldCoordinatesDimension()
    {
        return new Vector2((width - 1)*(platformDim+spacing), (height-1) * (platformDim + spacing));
    }

    public List<FieldPlatform> GetPlatformsAtCoords(List<Vector2Int> coords)
    {
        List<FieldPlatform> plats = new List<FieldPlatform>();
        foreach(Vector2Int coord in coords) {
            if (coord.x >= width || coord.x <0 || coord.y >= height || coord.y < 0) {
                plats.Add(null);//if it is out of range put a null
            } else
            {
                plats.Add(platforms[coord.x][coord.y]);
            }
        }
        return plats;
    }

    
}
