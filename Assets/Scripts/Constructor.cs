using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Constructor : MonoBehaviour
{
    
    public FieldCreator fieldCreator;
    public Material platSelectionMaterial;
    public Material validMaterial;
    public Material invalidMaterial;

    public Turret[] turretPrefabs = new Turret[3];

    [Header("UI Elements references")]
    public TextMeshProUGUI logText;

    enum Mode { None, Constructing, Destroying, MovingSelection, MovingNewPlace };
    

    private int fieldWidth;
    private int fieldHeight;

    //vars to set/put null/empty on begin mode, cancel and confirm
    private Mode currentMode = Mode.None;
    private FieldPlatform currentPlatform = null;
    private List<FieldPlatform> secondaryPlatforms = new List<FieldPlatform>();
    private Vector2Int currentCoords = Vector2Int.zero;
    private List<Vector2Int> secondaryCoords = new List<Vector2Int>();
    //vars to set/put null on changinf prefab
    private int currentPrefabIndex = 1; //TODO make 0 default
    private Turret currentTurret = null;
    private FieldPlatform prevMovePlatform = null;//just for move case
    private Turret prevMoveTurret = null;//just for move case
    private List<FieldPlatform> prevMoveSecondaryPlatforms = new List<FieldPlatform>();



    //TODO check " currentMode == ..." to see which ones I  include/remove
    //TODO disable other actions whn doing one (on Editor)!!!!
    //TODO after cancelling or (confirming and finishing action) empty and set respective variables



    // __________________________________________________________Begin Modes

    public void BeginConstructing()
    {
        bool b = BeginInit();
        if(!b)
        {
            return;
        }
        currentMode = Mode.Constructing;

        //  Set primary coordinates and get primary platform
        SetInitialCoordAndPlatform();

        //  Turret on this platform
        Vector3 turretPos = fieldCreator.CoordToPos(currentCoords.x, currentCoords.y);
        turretPos.y = turretPrefabs[currentPrefabIndex].transform.position.y;
        currentTurret = Instantiate(turretPrefabs[currentPrefabIndex], turretPos, turretPrefabs[currentPrefabIndex].transform.rotation);
        Vector3 displacement = currentTurret.transform.position - currentTurret.placementPoint.position;//displacement to locate turret correctly
        displacement.y = 0;//No displacement in y axis
        currentTurret.transform.position += displacement;

        currentTurret.rangeVisualization.SetActive(true);

        //  Get secondary coordinates and platforms
        foreach (var oc in currentTurret.otherFieldCoordinates)
        {
            secondaryCoords.Add(currentCoords + oc);
        }
        secondaryPlatforms = fieldCreator.GetPlatformsAtCoords(secondaryCoords);

        //  Visualize current platforms and turrets
        VisualizeCurrentPlatforms();
        VisualizeTurret();
    }

    public void BeginDestroying()
    {
        bool b = BeginInit();
        if (!b)
        {
            return;
        }
        currentMode = Mode.Destroying;

        //  Set primary coordinates and get primary platform
        SetInitialCoordAndPlatform();
        VisualizeCurrentPlatforms();

    }

    public void BeginMovingSelection()
    {
        bool b = BeginInit();
        if (!b)
        {
            return;
        }
        currentMode = Mode.MovingSelection;

        SetInitialCoordAndPlatform();
        VisualizeCurrentPlatforms();

    }

    private void SetInitialCoordAndPlatform()
    {
        currentCoords = Vector2Int.zero;
        List<FieldPlatform> plats = fieldCreator.GetPlatformsAtCoords(new List<Vector2Int> { currentCoords });
        currentPlatform = plats[0];

    }

    private bool BeginInit()
    {
        Vector2Int dimenstions = fieldCreator.GetFieldDimension();
        fieldWidth = dimenstions.x;
        fieldHeight = dimenstions.y;
        if (fieldWidth == 0 || fieldHeight == 0)
        {
            logText.SetText("Cannot Act on Field with dimension 0");
            return false;
        }
        return true;
    }

    // __________________________________________________________ Cancel, confirm and Validate action
    
    public void CancelAction()
    {
        logText.SetText("");
        SetCurrentPlatformsToNormal();

        if(currentMode == Mode.Constructing)
        {
            Destroy(currentTurret.gameObject);
        } else if(currentMode == Mode.MovingNewPlace) {
            prevMoveTurret.gameObject.SetActive(true);//TODO instead of setting to true set color to normal
            //delete current Turret
            Destroy(currentTurret.gameObject);
            //set again the turret to the fields
            List<FieldPlatform> allPlatforms = new List<FieldPlatform> { prevMovePlatform };
            allPlatforms.AddRange(prevMoveSecondaryPlatforms);
            prevMovePlatform.SetTurret(prevMoveTurret.gameObject, allPlatforms, true);
            foreach(var plat in prevMoveSecondaryPlatforms)
            {
                plat.SetTurret(prevMoveTurret.gameObject, allPlatforms, false);
            }
        }


        //Put variables again to default values
        CleanUp();
    }

    public void Confirm()
    {
        if (currentMode == Mode.Constructing)
        {
            bool validPlacement = Validate();//TODO make validation of Gold
            if (validPlacement)
            {
                //Assign turret to platform
                List<FieldPlatform> allPlatforms = new List<FieldPlatform> {currentPlatform};
                allPlatforms.AddRange(secondaryPlatforms);
                currentPlatform.SetTurret(currentTurret.gameObject, allPlatforms, true);
                foreach(var plat in secondaryPlatforms)
                {
                    plat.SetTurret(currentTurret.gameObject, allPlatforms, false);
                }

                currentTurret.rangeVisualization.SetActive(false);

                //put normal materials
                currentTurret.SetBackOriginalMaterial();
                SetCurrentPlatformsToNormal();


                //Put variables again to default values
                CleanUp();

                //TODO take Gold
            } else
            {
                logText.SetText("It is not possible to construct the turret in the current location");
            }
        } else if (currentMode == Mode.Destroying) { 
            bool validDeletion = Validate();
            if(validDeletion)
            {
                //Destroy turret
                Destroy(currentPlatform.turretAtPlatform);
                //TODO recover gold??????
                List<FieldPlatform> allPlatforms = currentPlatform.platformsWithTurret;
                //Remove reference in platforms
                foreach(var plat in allPlatforms)
                {
                    plat.RemoveTurret();
                }

                SetCurrentPlatformsToNormal();
                //Put variables again to default values
                CleanUp();
                

            } else
            {
                logText.SetText("There is no turret to delete in the current location");
            }
        } else if(currentMode == Mode.MovingSelection)
        {
            bool validMoving = Validate();
            if (validMoving)
            {
                //we get the primary platform and secondary fields
                List<FieldPlatform> allPlatforms = currentPlatform.platformsWithTurret;
                foreach(var plat in allPlatforms)
                {
                    if (plat.isPrimaryPlatform)
                    {
                        prevMovePlatform = plat;
                    } else
                    {
                        prevMoveSecondaryPlatforms.Add(plat);
                    }
                }
                //make a copy of current object so we move it afterwards
                GameObject copyObject = Instantiate(prevMovePlatform.turretAtPlatform);
                //Set the previous turret to inactive so that we can reset it
                prevMoveTurret = prevMovePlatform.turretAtPlatform.GetComponent<Turret>();
                prevMoveTurret.gameObject.SetActive(false);//TODO instead of inactive set some invisible color
                //Remove reference in platforms
                foreach (var plat in allPlatforms)
                {
                    plat.RemoveTurret();
                }

                //begin mode MovingNewPlace
                currentMode = Mode.MovingNewPlace;
                SetCurrentPlatformsToNormal();

                //  Set primary coordinates and get primary platform
                currentPlatform = prevMovePlatform;
                currentCoords = new Vector2Int(currentPlatform.coords.x, currentPlatform.coords.y);

                //  Turret on this platform
                currentTurret = copyObject.GetComponent<Turret>();
                currentTurret.rangeVisualization.SetActive(true);

                //  Get secondary coordinates and platforms
                secondaryCoords.Clear();
                foreach (var oc in currentTurret.otherFieldCoordinates)
                {
                    secondaryCoords.Add(currentCoords + oc);
                }
                secondaryPlatforms.Clear();
                secondaryPlatforms = fieldCreator.GetPlatformsAtCoords(secondaryCoords);

                //  Visualize current platforms and turrets
                VisualizeCurrentPlatforms();
                VisualizeTurret();

            }
            else
            {
                logText.SetText("There is no turret to move in the current location");
            }

        } else if (currentMode == Mode.MovingNewPlace)
        {
            bool validPlacement = Validate();
            if (validPlacement)
            {
                //Assign turret to platform
                List<FieldPlatform> allPlatforms = new List<FieldPlatform> { currentPlatform };
                allPlatforms.AddRange(secondaryPlatforms);
                currentPlatform.SetTurret(currentTurret.gameObject, allPlatforms, true);
                foreach (var plat in secondaryPlatforms)
                {
                    plat.SetTurret(currentTurret.gameObject, allPlatforms, false);
                }

                //put normal materials
                currentTurret.SetBackOriginalMaterial();
                SetCurrentPlatformsToNormal();

                currentTurret.rangeVisualization.SetActive(false);

                //Destroy previous turret (saved for recover)
                Destroy(prevMoveTurret.gameObject);

                //Put variables again to default values
                CleanUp();

            }
            else
            {
                logText.SetText("It is not possible to move the turret to the current location");
            }

        }
    }

    private void CleanUp()
    {
        currentMode = Mode.None;
        currentPlatform = null;
        secondaryPlatforms.Clear();
        currentCoords = Vector2Int.zero;
        secondaryCoords.Clear();
        currentPrefabIndex = 0;
        currentTurret = null;
        prevMovePlatform = null;
        prevMoveTurret = null;
        prevMoveSecondaryPlatforms.Clear();
}

    private bool Validate()
    {
        if (currentMode == Mode.Constructing || currentMode == Mode.MovingNewPlace)
        {
            if(currentPlatform.turretAtPlatform != null)
            {
                return false;
            }
            foreach(var plat in secondaryPlatforms)
            {
                if (plat == null || plat.turretAtPlatform != null)
                {
                    return false;
                }
            }
            return true;
        } else if (currentMode == Mode.Destroying || currentMode == Mode.MovingSelection)
        {
            if(currentPlatform.turretAtPlatform == null)
            {
                return false;
            } else
            {
                return true;
            }
        } else
        {
            throw new Exception($"Validate() should not be called in the mode {currentMode}");
        }
        
    }

    // __________________________________________________________Visualization Code

    private void SetCurrentPlatformsToNormal()
    {
        if (currentPlatform != null)
        {
            currentPlatform.SetBackOriginalMaterial();
        }

        foreach (var platform in secondaryPlatforms)
        {
            if (platform != null)
            {
                platform.SetBackOriginalMaterial();
            }
            
        }
    }

    private void VisualizeCurrentPlatforms()
    {

        //TODO perhaps quit visualization of secondary platforms???? (once done) 
        currentPlatform.SetMaterial(platSelectionMaterial);
        foreach (var platform in secondaryPlatforms)
        {
            if (platform != null)
            {
                platform.SetMaterial(platSelectionMaterial);
            }
        }
    }

    private void VisualizeTurret()
    {
        if(currentMode == Mode.Constructing || currentMode == Mode.MovingNewPlace)
        {
            bool validPlacement = Validate();
            if (validPlacement)
            {
                currentTurret.SetMaterial(validMaterial);
            } else
            {
                currentTurret.SetMaterial(invalidMaterial);
            }
        }
    }




    // __________________________________________________________Move around field Functions

    public void MoveUp()
    {
        MoveOnField((coords) =>
        {
            coords.y = coords.y + 1 % fieldHeight;
            return coords;
        });

    }

    public void MoveDown()
    {
        MoveOnField((coords) =>
        {
            coords.y -= 1;
            if (coords.y < 0)
            {
                coords.y = fieldHeight - 1;
            }
            return coords;
        });

    }

    public void MoveRight()
    {
        MoveOnField((coords) =>
        {
            coords.x = (coords.x + 1) % fieldWidth;
            return coords;
        });
    }

    public void MoveLeft()
    {

        MoveOnField((coords) =>
        {
            coords.x -= 1;
            if (coords.x < 0)
            {
                coords.x = fieldWidth - 1;
            }
            return coords;
        });

    }

    private void MoveOnField(Func<Vector2Int, Vector2Int> CoordChangeFunction)
    {
        logText.SetText("");
        if (currentMode == Mode.None)
        {
            return;
        }
        SetCurrentPlatformsToNormal();

        //  Set primary coordinates and get primary platform
        FieldPlatform refPlat = null;
        while (refPlat == null)
        {
            currentCoords = CoordChangeFunction(currentCoords);
            List<FieldPlatform> plats = fieldCreator.GetPlatformsAtCoords(new List<Vector2Int> { currentCoords });
            refPlat = plats[0];
        }
        currentPlatform = refPlat;
        currentPlatform.SetMaterial(platSelectionMaterial);

        
        if (currentMode == Mode.Constructing || currentMode == Mode.MovingNewPlace)
        {
            //  Turret on this platform
            Vector3 turretPos = fieldCreator.CoordToPos(currentCoords.x, currentCoords.y);
            turretPos.y = turretPrefabs[currentPrefabIndex].transform.position.y;
            currentTurret.transform.position = turretPos;

            Vector3 displacement = currentTurret.transform.position - currentTurret.placementPoint.position;
            displacement.y = 0;//No displacement in y axis
            currentTurret.transform.position += displacement;

            //  Get secondary coordinates and platforms
            secondaryCoords.Clear();
            foreach (var oc in currentTurret.otherFieldCoordinates)
            {
                secondaryCoords.Add(currentCoords + oc);
            }
            secondaryPlatforms.Clear();
            secondaryPlatforms = fieldCreator.GetPlatformsAtCoords(secondaryCoords);
        }
        //  Visualize current platforms and turrets    
        VisualizeCurrentPlatforms();
        VisualizeTurret();

    }

    public void Rotate()
    {
        logText.SetText("");
        if (currentMode == Mode.Constructing || currentMode == Mode.MovingNewPlace)
        {
            SetCurrentPlatformsToNormal();

            //  Turret on this platform
            currentTurret.Rotate90AntiHour();

            //  Get secondary coordinates and platforms
            secondaryCoords.Clear();
            foreach (var oc in currentTurret.otherFieldCoordinates)
            {
                secondaryCoords.Add(currentCoords + oc);
            }
            secondaryPlatforms.Clear();
            secondaryPlatforms = fieldCreator.GetPlatformsAtCoords(secondaryCoords);

            //  Visualize current platforms and turrets
            VisualizeCurrentPlatforms();
            VisualizeTurret();
        }
    }

    public void SwitchTurretPrefab(string prefabIndex)
    {
        if (currentMode == Mode.Constructing) {
            int index;
            bool parsed = int.TryParse(prefabIndex, out index);
            if (!parsed || index < 0 || index >= turretPrefabs.Length || index == currentPrefabIndex)
            {
                return;
            }

            currentPrefabIndex = index;
            SetCurrentPlatformsToNormal();

            //Destroy current turret
            Destroy(currentTurret.gameObject);

            //new Turret on this platForm
            Vector3 turretPos = fieldCreator.CoordToPos(currentCoords.x, currentCoords.y);
            turretPos.y = turretPrefabs[currentPrefabIndex].transform.position.y;
            currentTurret = Instantiate(turretPrefabs[currentPrefabIndex], turretPos, turretPrefabs[currentPrefabIndex].transform.rotation);
            Vector3 displacement = currentTurret.transform.position - currentTurret.placementPoint.position;//displacement to locate turret correctly
            displacement.y = 0;//No displacement in y axis
            currentTurret.transform.position += displacement;

            currentTurret.rangeVisualization.SetActive(true);


            //  Get secondary coordinates and platforms
            secondaryCoords.Clear();
            foreach (var oc in currentTurret.otherFieldCoordinates)
            {
                secondaryCoords.Add(currentCoords + oc);
            }
            secondaryPlatforms.Clear();
            secondaryPlatforms = fieldCreator.GetPlatformsAtCoords(secondaryCoords);

            //  Visualize current platforms and turrets    
            VisualizeCurrentPlatforms();
            VisualizeTurret();
        }
        

    }


}
