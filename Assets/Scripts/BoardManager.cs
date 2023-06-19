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
    public Material meshMaterial;
    public Material selectedMeshMaterial;
    Vector3 size;
    GameObject Cube;
    public List<TileInfo> clickable = new();

    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        Cube = piecesParent.transform.GetChild(0).gameObject;

        size = Cube.GetComponent<Renderer>().bounds.size;

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
        int sum = 0;
        foreach (var item in grid.depth)
        {
            foreach (var item2 in item.columns)
            {
                sum += item2.tiles.Length;
            }
        }
        if (sum % 2 != 0)
        {
            Debug.LogError("generation not possible");
            return;
        }
        int id = 0;

        for (int i = 0; i < grid.depth.Length; i++)
        {

            for (int j = 0; j < grid.depth[i].columns.Length; j++)
            {

                for (int k = 0; k < grid.depth[i].columns[j].tiles.Length; k++)
                {

                    Vector3 firstTrans = GetWorldPostion(k, j, i);
                    int matchId = GetRandomMatchId();
                    GameObject tile = Instantiate(piecesParent.transform.GetChild(matchId).gameObject, firstTrans, Quaternion.identity, transform.GetChild(0));
                    tile.name = $"x = {k} , y = {j} , z = {i}";
                    Tile tileScript = tile.AddComponent<Tile>();

                    
                    tileScript.tileInfo = new TileInfo(k, j, i, id, matchId,tile);
                    grid.depth[i].columns[j].tiles[k] = tileScript.tileInfo;
                   id++;
                    tile.AddComponent<BoxCollider>();
                }
            }
        }
        
    }
    int GetRandomMatchId()
    {
        return Random.Range(0, piecesParent.transform.childCount - 1);
    }
    [ContextMenu("GetAllClickable")]
    public void GetAllClickable()
    {
        clickable.Clear();
        foreach (var d in grid.depth)
        {
            foreach (var y in d.columns)
            {
                foreach (var x in y.tiles)
                {
                    if (CanSelect(x))
                    {
                        clickable.Add(x);
                    }
                    else
                    {
                        Debug.Log("cant");
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="k">x</param>
    /// <param name="j">y</param>
    /// <param name="i">layer/depth</param>
    /// <returns></returns>
    Vector3 GetWorldPostion(int k, int j, int i)
    {
        Vector3 firstTrans = new Vector3();
        firstTrans.x = (((grid.depth[i].columns.Length - 1) * (-size.x)) / 2) + ((size.x + gap) * j);
        firstTrans.y = ((grid.depth[i].columns[j].tiles.Length - 1) * (size.y) / 2) + ((-size.y - gap) * k);
        firstTrans.z = (((grid.depth.Length - 1) * (-size.z) / 2) + ((size.z + gap) * i));
        return firstTrans;
    }
    int GetTileXCount(int y, int depth)
    {
        return grid.depth[depth].columns[y].tiles.Length;
    }
    int GetTileYCount( int depth)
    {
        return grid.depth[depth].columns.Length;
    }
    bool CanSelect(TileInfo tileInfo)
    {
        if (tileInfo.y-1>=0)
        {
            if (GetTileXCount(tileInfo.y, tileInfo.layer) % 2 == GetTileXCount(tileInfo.y - 1, tileInfo.layer)
                        % 2)
            {
                print("here");

                if (GetTileInfoByCoord(tileInfo.x, tileInfo.y - 1, tileInfo.layer) == null)
                {

                    print("here");
                    return true;
                }

            }
            else
            {
                print("here");

                              
                if ((tileInfo.x + 1>=GetTileXCount(tileInfo.y - 1, tileInfo.layer)|| GetTileInfoByCoord(tileInfo.x + 1, tileInfo.y - 1, tileInfo.layer) == null )
                    && (GetTileInfoByCoord(tileInfo.x - 1, tileInfo.y - 1, tileInfo.layer) == null || tileInfo.y <0))
                {
                    print("here");

                    return true;
                }
            }
        }
        else
        {
            return true;
        }
        if (tileInfo.y+1<GetTileYCount(tileInfo.layer))
        {
            if (GetTileXCount(tileInfo.y, tileInfo.layer) % 2 == GetTileXCount(tileInfo.y + 1, tileInfo.layer)
                        % 2)
            {
                print("here");

                if (GetTileInfoByCoord(tileInfo.x, tileInfo.y + 1, tileInfo.layer) == null)
                {
                    print("here");

                    return true;
                }
            }
            else
            {
                print("here");

                if ((tileInfo.x + 1 >= GetTileXCount(tileInfo.y + 1, tileInfo.layer)||GetTileInfoByCoord(tileInfo.x + 1, tileInfo.y + 1, tileInfo.layer) == null )
                    && (tileInfo.x < 0||GetTileInfoByCoord(tileInfo.x - 1, tileInfo.y + 1, tileInfo.layer) == null ))
                {
                    print("here");

                    return true;
                }
            }
        }
        else
        {
            return true;
        }
        
        
        return false;

    }
    public void TileSelected(Tile tile)
    {
        if (!CanSelect(tile.tileInfo))
        {
            return;
        }

        if (prevSelectedTile == null)
        {

            prevSelectedTile = tile;
            //mats[2] = selectedMeshMaterial;
            foreach (var item in tile.GetComponent<MeshRenderer>().materials)
            {
                item.color = Color.yellow;
            }

            return;
        }
        if (tile.tileInfo.matchId == prevSelectedTile.tileInfo.matchId && prevSelectedTile.tileInfo.id != tile.tileInfo.id)
        {
            Matched(prevSelectedTile, tile);
        }
        else
        {

            foreach (var item in tile.GetComponent<MeshRenderer>().materials)
            {
                item.color = Color.yellow;
            }
            foreach (var item in prevSelectedTile.GetComponent<MeshRenderer>().materials)
            {
                item.color = Color.white;
            }
            prevSelectedTile = tile;
        }
    }
    void Matched(Tile tile1, Tile tile2)
    {

        prevSelectedTile = null;
        Destroy(tile1.gameObject);
        Destroy(tile2.gameObject);
        for (int i = 0; i < grid.depth.Length; i++)
        {
            for (int j  = 0; j< grid.depth[i].columns.Length; j++)
            {
                for (int k = 0; k < grid.depth[i].columns[j].tiles.Length; k++)
                {
                    if (grid.depth[i].columns[j].tiles[j] == tile1.tileInfo|| grid.depth[i].columns[j].tiles[j] == tile2.tileInfo)
                    {
                        grid.depth[i].columns[j].tiles[j].go = null;
                    }
                    
                }
            }
        }
           }
    GameObject GetTileInfoByCoord(int x, int y, int depth)
    {
        Debug.Log($"{x},{y},{depth}");
        Debug.Log(grid.depth[depth].columns[y].tiles[x].go);
        return grid.depth[depth].columns[y].tiles[x].go;

    } }


[System.Serializable]
public class X
{
    public X()
    {
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

    public Layer(int yAmount, int xAmount)
    {
        y = new Y[yAmount];
        for (int i = 0; i < yAmount; i++)
        {
            y[i] = new Y(xAmount);
        }
    }

    [field: SerializeField] public Y[] y;
}
[System.Serializable]
public class Table
{
    [field: SerializeField] public Layer[] layers;
    public Table(int layerAmount, int yAmount, int xAmount)
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
    [field: SerializeField] public int id;
    [field: SerializeField] public int matchId;
    [field: SerializeField] public int x;
    [field: SerializeField] public int y;
    [field: SerializeField] public int layer;
    [field: SerializeField] public GameObject go;
    public Piece pieceType;
    public void SetIds(int id, int matchId)
    {
        this.id = id;
        this.matchId = matchId;
    }
    public TileInfo(int x, int y, int layer, int id, int matchId,GameObject go)
    {
        this.x = x;
        this.y = y;
        this.layer = layer;
        this.matchId = matchId;
        this.id = id;
        this.go = go;
    }
}
[System.Serializable]
public class Grid
{
    public Depth[] depth;


}
[System.Serializable]
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
    none, A
}
