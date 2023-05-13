using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



public class Tile : MonoBehaviour
{
    BoardManager bm;
    public TileInfo tileInfo;
    public MeshRenderer mesh;
    private void Awake()
    {
        //mesh = GetComponent<MeshRenderer>();
    }
    private void Start()
    {
       /* bm = BoardManager._instance;*/
    }
    private void OnMouseOver()
    {
    }
    private void OnMouseDown()
    {
        
        
        BoardManager._instance.TileSelected(this);
/*        print(matchId);
*/
    }
}
