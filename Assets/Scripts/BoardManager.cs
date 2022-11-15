using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class BoardManager : MonoBehaviour
{
    public static BoardManager _instance;

    public List<Material> materials = new();
    public int rows, columns, layers;
    [field: SerializeField]public Table table;
    List<Tile> tileList = new List<Tile>();
    Tile prevSelectedTile;
    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {

        table = new Table(layers, columns, rows);

        GenerateBoard();

    }

    public void GenerateBoard()
    {
        if (Mathf.Pow( columns*rows,layers)%4 != 0)
        {
            return;
        }
        GameObject Cube = Resources.Load<GameObject>("Cube");
        Transform parrent = transform.GetChild(0);
        Vector3 firstTrans = parrent.GetChild(0).position;
        //  Setting Tiles

        for (int k = 0; k < table.layers.Length; k++)
        {
            

            for (int i = 0; i < table.layers[k].columns.Length; i++)
            {

                for (int j = 0; j < table.layers[k].columns[i].rows.Length; j++)
                {

                    GameObject tile = Instantiate(Cube, transform.position, Quaternion.identity);
                    table.layers[k].columns[i].rows[j].tile = tile;
                    tile.transform.parent = parrent;
                    tile.transform.position = firstTrans;
                    firstTrans.x += Cube.transform.localScale.x;


                    Tile tileS = tile.GetComponent<Tile>();
                    tileS.tileInfo = new TileInfo(j, i, k);
                    
                    tile.name = $"layer = {k} , col = {i} , row {j}";
                }
                firstTrans.x = parrent.transform.GetChild(0).position.x;
                firstTrans.y -= Cube.transform.localScale.y;
            }
            firstTrans.z -= Cube.transform.localScale.z;
            var tempZPos = firstTrans;
            firstTrans = parrent.GetChild(0).position;
            firstTrans.z = tempZPos.z;
            
        }
        GiveIds();
    }
    void GiveIds()
    {
        int id = 0;
        int index = 0;
        int mathcId = GetRandomMaterialNumber(); ;
        List<Tile> tiles = new List<Tile>();
        foreach (var item in table.layers)
        {
            foreach (var c in item.columns)
            {
                foreach (var r in c.rows)
                {
                    tiles.Add(r.tile.GetComponent<Tile>());


                    
                }
            }
            while (tiles.Count > 0)
            {

                int temp = Random.Range(0, tiles.Count);
                
                SetMaterial(tiles[temp].mesh, materials[mathcId]);
                tiles[temp].tileInfo.SetIds(id, mathcId);
                
                tiles[temp].tileInfo.id = id;
                tiles[temp].gameObject.name += $" ,id = {id}";
                tiles.RemoveAt(temp);
                index++;
                id++;
                if (index == 2)
                {
                    mathcId = GetRandomMaterialNumber();
                    index = 0;
                }
            }

        }

    }
    void SetMaterial(MeshRenderer mesh,Material mat)
    {
        mesh.material = mat;
    }

    int GetRandomMaterialNumber()
    {
        return Random.Range(0, materials.Count);
    }
    public void TileSelected(Tile tile)
    {
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
        //print(tile1.tileInfo.matchId);
        //print(tile2.tileInfo.matchId);
        print(tile1.tileInfo.layer);
        print(tile2.tileInfo.layer);
        prevSelectedTile = null;
        Destroy(tile1.gameObject);
        Destroy(tile2.gameObject);
    }
}


[System.Serializable]
public class Row
{
    public Row(){
    }
   [field: SerializeField] public GameObject tile;
}
[System.Serializable]

public class Column
{
    public Column(int rowAmount)
    {
        rows = new Row[rowAmount];
        for (int i = 0; i < rowAmount; i++)
        {
            rows[i] = new Row();
        }
    }
   [field: SerializeField] public Row[] rows;
}
[System.Serializable]

public class Layer
{

    public Layer(int columnAmount,int rowAmount)
    {
        columns = new Column[columnAmount];
        for (int i = 0; i < columnAmount; i++)
        {
            columns[i] = new Column(rowAmount);
        }
    }

   [field: SerializeField]public Column[] columns;
}
[System.Serializable]
public class Table
{
    [field:SerializeField]public Layer[] layers;
    public Table(int layerAmount,int columnAmount,int rowAmount)
    {
        layers = new Layer[layerAmount];
        for (int i = 0; i < layerAmount; i++)
        {
            layers[i] = new Layer(columnAmount,rowAmount);
        }
    }
}[System.Serializable]
public class TileInfo
{
    [field:SerializeField]public int id;
    [field:SerializeField]public int matchId;
    [field:SerializeField]public int row;
    [field:SerializeField]public int column;
    [field:SerializeField]public int layer;
    public void SetIds(int id,int matchId)
    {
        this.id = id;
        this.matchId = matchId;
    }
    public TileInfo( int row, int column,int layer)
    {
        this.row = row;
        this.column = column;
        this.layer = layer;
    }
}
