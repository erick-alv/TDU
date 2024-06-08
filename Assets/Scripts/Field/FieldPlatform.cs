using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldPlatform : MonoBehaviour
{
    public Vector2Int coords;
    public GameObject turretAtPlatform = null;
    public List<FieldPlatform> platformsWithTurret = null;
    public bool isPrimaryPlatform = false;

    private Renderer rend;
    private Material originalMaterial;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
    }

    public void SetTurret(GameObject turret, List<FieldPlatform> platformsWithTurret, bool isPrimary)
    {
        turretAtPlatform = turret;
        this.platformsWithTurret = platformsWithTurret;
        isPrimaryPlatform = isPrimary;
    }

    public void RemoveTurret()
    {
        turretAtPlatform = null;
        platformsWithTurret = null;
        isPrimaryPlatform = false;
    }

    public void SetMaterial(Material newMaterial)
    {
        rend.material = newMaterial;
    }

    public void SetBackOriginalMaterial()
    {
        rend.material = originalMaterial;
    }
}
