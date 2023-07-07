using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public List<TileInfo> allTileInfo = new();
    public List<TileInfo> clickable = new();
    public List<TileInfo> matchable = new();
    public string selectedTileIndex;

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
        allTileInfo = new();
        GenerateBoard();

    }
    [ContextMenu("Solve")]
    void Solve()
    {
        StartCoroutine(SolveC());
    }
    IEnumerator SolveC()
    {
        while (ContainGo(allTileInfo))
        {
            yield return new WaitForSeconds(0f);
            var two = GetTwoMatchable();

            if (two.Count==2)
            {
                TileSelected(two[0].go.GetComponent<Tile>());
                yield return new WaitForSeconds(0f);
                TileSelected(two[1].go.GetComponent<Tile>());

            }
        }
    }
    public void GenerateBoard()
    {

        allTileInfo.Clear();


              int sum = 0;
        foreach (var item in grid.depth)
        {
            foreach (var item2 in item.columns)
            {
                sum += item2.tiles.Count;
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

                for (int k = 0; k < grid.depth[i].columns[j].tiles.Count; k++)
                {

                    Vector3 firstTrans = GetWorldPostion(k, j, i);
                    int matchId = GetRandomMatchId();
                    GameObject tile = Instantiate(piecesParent.transform.GetChild(matchId).gameObject, firstTrans, Quaternion.identity, transform.GetChild(0));
                    tile.name = $"x = {grid.depth[i].columns[j].tiles.Count / 2 - k} , y = {j} , z = {i}";
                    Tile tileScript = tile.AddComponent<Tile>();

                    
                    tileScript.tileInfo = new TileInfo(grid.depth[i].columns[j].tiles.Count / 2 - k, j, i, id, matchId,tile,firstTrans);
                    grid.depth[i].columns[j].tiles[k] = tileScript.tileInfo;
                   id++;
                    tile.AddComponent<BoxCollider>();
                    allTileInfo.Add(tileScript.tileInfo);
                }
            }
        }
               MakeSolvable();
        PostMove();

    }
    int GetRandomMatchId()
    {
        int a;
        do
        {
             a = UnityEngine.Random.Range(0, piecesParent.transform.childCount - 1);
           /* if (a != 1)
            {
*/
            /*} */
        } while (a==1);
                return a;
        
    }
    bool ContainGo(List<TileInfo> tiles)
    {
        return tiles.Exists(x => x.go );
    }
    public void GetAllTileInfos()
    {
        allTileInfo.Clear();
        foreach (var d in grid.depth)
        {
            foreach (var y in d.columns)
            {
                foreach (var x in y.tiles)
                {
                    if (x.go != null)
                    {
                        allTileInfo.Add(x);

                    }
                }
            }
        }
    }
    public List<TileInfo> GetAllClickable()
    {
        List<TileInfo> tiles = new();
        foreach (var item in allTileInfo)
        {
            if (item.go ==null)
            {
                continue;
            }
            if (CanSelect(item))
            {
                tiles.Add(item);
                /*foreach (var a in item.go.GetComponent<MeshRenderer>().materials)
                {
                    a.color = Color.red;
                }*/
            }
        }
        return tiles;
           }
    public void ColorList(List<TileInfo> tileInfos,Color color)
    {
        foreach (var item in allTileInfo)
        {
            if (item.go != null)
            {
                
                if (tileInfos.Contains(item))
                {
                    foreach (var a in item.go.GetComponent<MeshRenderer>().materials)
                    {

                        a.color = color;
                    }
                }
                else
                {
                    foreach (var a in item.go.GetComponent<MeshRenderer>().materials)
                    {

                        a.color = Color.white;
                    }
                }
           
                
            }
        }
        
    }

    Tuple<int,int> GetLeastAndHighestAmountOfMatchid()
    {
       /* GetAllTileInfos();*/
        int highest = -1, lowest = 999;
        if (allTileInfo.Count>0)
        {
            Dictionary<int, int> dic = new();
            foreach (var item in allTileInfo)
            {
                if (dic.ContainsKey(item.matchId))
                {
                    dic[item.matchId]++;
                    Debug.LogError(dic[item.matchId]);
                }
                else
                {
                    dic.Add(item.matchId, 1);
                }
                if (dic[item.matchId] > highest)
                {
                    highest = dic[item.matchId];
                }
                if (dic[item.matchId] < lowest)
                {
                    lowest = dic[item.matchId];
                }
            }
        }
        
        return new Tuple<int, int>(lowest, highest);


    }
    
    void MakeMatchable(int amount)
    {
        List<TileInfo> matchable = GetAllMatchable();
        if (matchable.Count >= amount)
        {
            return;
        }
        clickable = GetAllClickable();
        if (amount<=clickable.Count)
        {
            for (int i = 0; i < amount/2; i++)
            {
                clickable = GetAllClickable();

                if (matchable.Count >= amount)
                {
                    break;
                }
                else if(clickable.Count>=2)
                {
                   var a= clickable.Find(x => matchable.Find(y => y.matchId != x.matchId).go!=null);
                    matchable.Add(a);
                    var b = clickable.Find(x => matchable.Find(y => y.matchId != x.matchId).go!=null);
                    b.matchId = a.matchId;
                    matchable.Add(b);
                }

            }
        }
        else if (amount>clickable.Count)
        {
            for (int i = 0; i < clickable.Count/2; i++)
            {
                clickable = GetAllClickable();

                if (matchable.Count >= amount)
                {
                    break;
                }
                else if(clickable.Count>=2)
                {
                    var a = clickable.Find(x => matchable.Find(y => y.matchId != x.matchId).go != null);
                    matchable.Add(a);
                    var b = clickable.Find(x => matchable.Find(y => y.matchId != x.matchId).go != null);
                    b.matchId = a.matchId;
                    matchable.Add(b);
                }
            }
        }


        Debug.Log(matchable.Count);
        //if (clickable.Count>=2)
        //{

        //    TileInfo tile1 = clickable[UnityEngine.Random.Range(0, clickable.Count)];
        //    clickable.Remove(tile1);

        //    TileInfo tile2 = clickable[UnityEngine.Random.Range(0, clickable.Count)];

        //    clickable.Remove(tile2);

        //    tile2.matchId = tile1.matchId;
        //    Destroy(tile1.go);
        //    Destroy(tile2.go);
        //    tile1.go = null;
        //    tile2.go = null;
        //}
    }
    public void DestroyMatchable()
    {

        foreach (var item in GetAllMatchable())
        {
            Destroy(item.go);
            item.go = null;
        }
    }
    
        bool IsMatchingPossible()
    {
        List<int> matchIds = new();
        foreach (var item in clickable)
        {
            if (matchIds.Exists(x=>x == item.matchId))
            {
                //Debug.LogError("match possible");
                return true;
            }
            else
            {

                matchIds.Add(item.matchId);
            }
        }
        return false;
    }
    [ContextMenu("Get all matachable")]
    public List<TileInfo> GetAllMatchable()
    {

        List<TileInfo> matches = new();
       var clicks= GetAllClickable();

        matchable.Clear();
        Hashtable table = new();
        foreach (var item in clicks)
        {
            if (table.Contains(item.matchId))
            {
                //Destroy()

                matches.Add(item);
                matches.Add((TileInfo)table[item.matchId]);
                table.Remove(item.matchId);

            }
            else
            {
                table.Add(item.matchId, item);
            }
        }
        return matches;
    }

    List<TileInfo> GetTwoMatchable()
    {
        List<TileInfo> matches = new();
        var clicks = GetAllClickable();

        matchable.Clear();
        Hashtable table = new();
        foreach (var item in clicks)
        {
            if (table.Contains(item.matchId))
            {
                //Destroy()

                matches.Add(item);
                matches.Add((TileInfo)table[item.matchId]);
                table.Remove(item.matchId);
                return matches;

            }
            else
            {
                table.Add(item.matchId, item);
            }
        }
        return matches;

    }
    public void MakeSolvable()
    {

        StartCoroutine(MakeSolvableD());
       
    }
    IEnumerator MakeSolvableD()
    {
        while (ContainGo(allTileInfo))
        {
            clickable.Clear();
            clickable.AddRange( GetAllClickable());
            if (clickable.Count == 0)
            {

                Debug.LogError("clickable not possible");
                Debug.LogError(allTileInfo.Count);
                break;
            }
            MakeMatchable(6);
            matchable = GetAllMatchable();
            ColorList(matchable, Color.cyan);
            yield return new WaitForSeconds(3);
            DestroyMatchable();


        }
        MakeGOForAllAccordingToMatchid();
    }
    public void MakeGOForAllAccordingToMatchid()
    {
        
                foreach (var r in allTileInfo)
                {
                    if (r.go!=null)
                    {
                        Destroy(r.go);
                        r.go = null;
                       
                    }
                    
                    r.go = Instantiate(piecesParent.transform.GetChild(r.matchId).gameObject, r.worldPostion, Quaternion.identity, transform.GetChild(0));
                    Tile tileScript = r.go.AddComponent<Tile>();
                    tileScript.tileInfo = r;

                    r.go.AddComponent<BoxCollider>();

                }
          } public void MakeGOAccordingToMatchid(TileInfo tile)
    {
                               Destroy(tile.go);
                        tile.go = null;
                        tile.go = Instantiate(piecesParent.transform.GetChild(tile.matchId).gameObject, tile.worldPostion, Quaternion.identity, transform.GetChild(0));
                          
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
        firstTrans.y = ((grid.depth[i].columns[j].tiles.Count - 1) * (size.y) / 2) + ((-size.y - gap) * k);
        firstTrans.z = (((grid.depth.Length - 1) * (-size.z) / 2) + ((size.z + gap) * -i));
        return firstTrans;
    }
    int GetTileXCount(int y, int depth)
    {
        return grid.depth[depth].columns[y].tiles.Count;
    }
    int GetTileYCount( int depth)
    {
        return grid.depth[depth].columns.Length;
    }
    bool CanSelectZ(TileInfo tileInfo)
    {
        //return true;
        //if (tileInfo.layer==grid.depth.Length-1)
        //{
        //    Debug.Log("can");
        //    return true;
        //}
        //if (tileInfo.layer+1 >= grid.depth.Length)
        //{
        //    return false;
        //}
        if (tileInfo.layer == grid.depth.Length - 1)
        {
            Debug.Log(" abs can");
            return true;
        }
        if (tileInfo.layer+1 >= grid.depth.Length)
        {
            return false;
            

        }
        if (GetTileXCount(tileInfo.y,tileInfo.layer) == GetTileXCount(tileInfo.y, tileInfo.layer+1))
        {
            Debug.Log("can");

            if (GetGoByCoord(tileInfo.x,tileInfo.y,tileInfo.layer+1) == null)
            {
                Debug.Log("can");

                return true;
            }
        }
        else
        {
            Debug.Log("can");

            if (GetTileXCount(tileInfo.y, tileInfo.layer) %2 == 0)
            {
                Debug.Log("can");

                if (GetGoByCoord(tileInfo.x,tileInfo.y,tileInfo.layer+1) == null && GetGoByCoord(tileInfo.x-1, tileInfo.y, tileInfo.layer + 1) == null)
                {
                    Debug.Log("can");

                    return true;
                }
            }
            else
            {
                Debug.Log("can");

                if (GetGoByCoord(tileInfo.x, tileInfo.y, tileInfo.layer + 1) == null && GetGoByCoord(tileInfo.x + 1, tileInfo.y, tileInfo.layer + 1) == null)
                {
                    Debug.Log("can");

                    return true;
                }
            }
            
        }
        Debug.Log("can");

        return false;
    }
    bool CanSelect(TileInfo tileInfo)
    {
        //Debug.Log(CanSelectZ(tileInfo));
        return CanSelectZ(tileInfo) && CanSelectXY(tileInfo);
        
    }
    bool CanSelectXY(TileInfo tileInfo)
    {
        if (tileInfo.y - 1 >= 0)
        {
            if (GetTileXCount(tileInfo.y, tileInfo.layer)%2 == GetTileXCount(tileInfo.y - 1, tileInfo.layer) % 2
                       )
            {
                print("here");

                if (GetGoByCoord(tileInfo.x, tileInfo.y - 1, tileInfo.layer) == null)
                {

                    print("here");
                    return true;
                }

            }
            else
            {
                print("here");
                if (GetTileXCount(tileInfo.y, tileInfo.layer)%2==0)
                {
                    if ((GetGoByCoord(tileInfo.x - 1, tileInfo.y - 1, tileInfo.layer) == null)
                                        && (GetGoByCoord(tileInfo.x, tileInfo.y - 1, tileInfo.layer) == null))
                    {
                        print("here");

                        return true;
                    }
                }
                else
                {
                    if ((GetGoByCoord(tileInfo.x + 1, tileInfo.y - 1, tileInfo.layer) == null)
                                                           && (GetGoByCoord(tileInfo.x, tileInfo.y - 1, tileInfo.layer) == null))
                    {
                        print("here");

                        return true;
                    }
                }
                               
                
            }
        }
        else
        {
            return true;
        }
        if (tileInfo.y+1<GetTileYCount(tileInfo.layer))
        {
            if (GetTileXCount(tileInfo.y, tileInfo.layer)%2 == GetTileXCount(tileInfo.y + 1, tileInfo.layer) % 2)
            {
                print("here");
                               
                if (GetGoByCoord(tileInfo.x, tileInfo.y + 1, tileInfo.layer) == null)
                {
                    print("here");

                    return true;
                }
            }
            else
            {
                if (GetTileXCount(tileInfo.y, tileInfo.layer) %2==0)
                {
                    if ((GetGoByCoord(tileInfo.x - 1, tileInfo.y + 1, tileInfo.layer) == null)
                                        && (GetGoByCoord(tileInfo.x, tileInfo.y + 1, tileInfo.layer) == null))
                    {
                        print("here");

                        return true;
                    }
                }
                else
                {
                    if ((GetGoByCoord(tileInfo.x + 1, tileInfo.y + 1, tileInfo.layer) == null)
                                                            && (GetGoByCoord(tileInfo.x, tileInfo.y + 1, tileInfo.layer) == null))
                    {
                        print("here");

                        return true;
                    }
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
        Debug.Log(CanSelect(tile.tileInfo));
       
        if (!clickable.Contains(tile.tileInfo))
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
        foreach (var depth in grid.depth)
        {
            foreach (var col in depth.columns)
            {

                foreach (var row in col.tiles)
                {

                    if (row == tile1.tileInfo || row == tile2.tileInfo)
                    {
                        row.go = null;
                    }
                }
            }
        }

        prevSelectedTile = null;
        Destroy(tile1.gameObject);
        Destroy(tile2.gameObject);

              
        PostMove();
    }

    void PostMove()
    {
        //GetAllTileInfos();
        clickable.Clear();
        matchable.Clear();
        
        clickable.AddRange( GetAllClickable());
        if (!IsMatchingPossible())
        {
            Debug.LogError("Match not possible");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }
        else
        {
           matchable= GetAllMatchable();
        }

        ColorList(clickable,Color.red);
    }
       GameObject GetGoByCoord(int x, int y, int depth)
    {
        try
        {
            return allTileInfo.Find(xx => xx.x == x && xx.y == y && xx.layer == depth).go;

        }
        catch (Exception)
        {

            return null;        }
       
    } GameObject GetGoByCoord(Tile tile)
    {
        try
        {
        return allTileInfo.Find(xx => xx.x == tile.tileInfo.x && xx.y == tile.tileInfo.y && xx.layer == tile.tileInfo.layer).go;

        }
        catch (Exception)
        {

            return null;
        }

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
    [field: SerializeField] public Vector3 worldPostion;
    public Piece pieceType;
    public void SetIds(int id, int matchId)
    {
        this.id = id;
        this.matchId = matchId;
    }
    public TileInfo(int x, int y, int layer, int id, int matchId,GameObject go,Vector3 worldPostion)
    {
        this.x = x;
        this.y = y;
        this.layer = layer;
        this.matchId = matchId;
        this.id = id;
        this.go = go;
        this.worldPostion = worldPostion;
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
    public List<TileInfo> tiles;
}
public enum Piece
{
    none, A
}
