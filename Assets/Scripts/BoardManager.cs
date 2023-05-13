using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager _instance;

    public List<Material> materials = new();
    public int x, y, layers;
    public float gap;
    List<Tile> tileList = new List<Tile>();
    public Tile prevSelectedTile;
    public Grid grid;
    public GameObject piecesParent;
    
    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {

        //table = new Table(layers, columns, rows);
        //grid = new (x, y, layers);

        GenerateBoard();

    }

    public void GenerateBoard()
    {



       /* if (Mathf.Pow(columns * rows, layers) % 4 != 0)
        {
            return;
        }*/
        GameObject Cube = piecesParent.transform.GetChild(0).gameObject;


        Vector3 size = Cube.GetComponent<Renderer>().bounds.size;
        int id = 0;

        for (int i = 0; i < grid.depth.Length; i++)
        {

            for (int j = 0; j < grid.depth[i].columns.Length; j++)
            {

                for (int k = 0; k < grid.depth[i].columns[j].tiles.Length; k++)
                {
                    
                                       Vector3 firstTrans = new Vector3();
                                       firstTrans.x = (((grid.depth[i].columns.Length - 1) * (-size.x)) / 2) + ((size.x+gap) * j);
                    firstTrans.y = ((grid.depth[i].columns[j].tiles.Length - 1) * (size.y) / 2) + ((-size.y-gap) * k);
                    firstTrans.z = (((grid.depth.Length - 1) * (-size.z )/2)+ ((size.z+gap)*i));
                    int matchId = Random.Range(0, piecesParent.transform.childCount - 1);
                    GameObject tile = Instantiate(piecesParent.transform.GetChild(matchId).gameObject, firstTrans, Quaternion.identity,transform.GetChild(0));
                    tile.name = $"x = {k} , y = {j} , z = {i}";
                    Tile tileScript = tile.AddComponent<Tile>();
                    tileScript.tileInfo = new TileInfo(k, j, i,id,matchId);
                    id++;
                    tile.AddComponent<BoxCollider>();
                                   }
            }
        }
            }
   
    
       public void TileSelected(Tile tile)
    {
        Debug.Log("clicked");
        if (GetTileByCoord(tile.tileInfo.x,tile.tileInfo.y-1,tile.tileInfo.layer) != null && GetTileByCoord(tile.tileInfo.x, tile.tileInfo.y + 1, tile.tileInfo.layer) != null)
        {

        }
        if (prevSelectedTile == null)
        {
            
            prevSelectedTile = tile;
            return;
        }
        if (tile.tileInfo.matchId == prevSelectedTile.tileInfo.matchId && prevSelectedTile.tileInfo.id != tile.tileInfo.id)
        {
            Matched(prevSelectedTile,tile);
        }
        else
        {
            prevSelectedTile = tile;
        }
    }
    void Matched(Tile tile1,Tile tile2)
    {
        
               prevSelectedTile = null;
        Destroy(tile1.gameObject);
        Destroy(tile2.gameObject);
    }
    TileInfo GetTileByCoord(int x,int y,int depth)
    {
        try
        {
            return grid.depth[depth].columns[x].tiles[y];
        }
        catch (System.Exception)
        {

            return null;        }
        
    }
}


[System.Serializable]
public class X
{
    public X(){
    }
   [field: SerializeField] public GameObject tile;
}
[System.Serializable]

public class Y
{
    public Y(int rowAmount)
    {
        x = new X[rowAmount];
        for (int i = 0; i < rowAmount; i++)
        {
            x[i] = new X();
        }
    }
   [field: SerializeField] public X[] x;
}
[System.Serializable]

public class Layer
{

    public Layer(int yAmount,int xAmount)
    {
        y = new Y[yAmount];
        for (int i = 0; i < yAmount; i++)
        {
            y[i] = new Y(xAmount);
        }
    }

   [field: SerializeField]public Y[] y;
}
[System.Serializable]
public class Table
{
    [field:SerializeField]public Layer[] layers;
    public Table(int layerAmount,int yAmount,int xAmount)
    {
        layers = new Layer[layerAmount];
        for (int i = 0; i < layerAmount; i++)
        {
            layers[i] = new Layer(yAmount, xAmount);
        }
    }
}
[System.Serializable]
public class TileInfo
{
    [field:SerializeField]public int id;
    [field:SerializeField]public int matchId;
    [field:SerializeField]public int x;
    [field:SerializeField]public int y;
    [field:SerializeField]public int layer;
    public Piece pieceType;
    public void SetIds(int id,int matchId)
    {
        this.id = id;
        this.matchId = matchId;
    }
    public TileInfo( int x, int y,int layer,int id,int matchId)
    {
        this.x = x;
        this.y = y;
        this.layer = layer;
        this.matchId = matchId;
        this.id = id;
    }
}
[System.Serializable]
public class Grid
{
    public Depth[] depth;
    

}
[System.Serializable ]
public class Depth
{
    public Column[] columns;
}
[System.Serializable]
public class Column
{
    public TileInfo[] tiles;
}
public enum Piece
{
    none,A
}
