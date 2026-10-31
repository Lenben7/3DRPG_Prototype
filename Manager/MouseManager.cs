using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MouseManager : Singleton<MouseManager>
{
    public Texture2D point, doorway, attack, target, arrow;

    RaycastHit hitInfo;

    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> OnEnemyClicked;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        SetCursorTexture();
        MouseControll();
    }

    void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hitInfo))
        {
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(0,0), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(0,0), CursorMode.Auto);
                    break;
                case "Attackable":
                    Cursor.SetCursor(arrow, new Vector2(16,16), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(0,0), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(target, new Vector2(0,0), CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControll()
    {
        if(Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground")) 
            {
                OnMouseClicked?.Invoke(hitInfo.point);
            }
            if (hitInfo.collider.gameObject.CompareTag("Enemy")) 
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Attackable")) 
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Portal")) 
            {
                OnMouseClicked?.Invoke(hitInfo.point);
            }
        }
    }
}
