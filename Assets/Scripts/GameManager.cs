using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    public GameObject token;
    public GameObject start_finalToken;
    public GameObject wayToken;
    private int[,] GameMatrix; //0 not chosen, 1 player, 2 enemy de momento no hago nada con esto
    private Node[,] NodeMatrix;
    private int startPosx, startPosy;
    private int endPosx, endPosy;
    void Awake()
    {
        Instance = this;
        GameMatrix = new int[Size, Size];
        Calculs.CalculateDistances(Panel, Size);
    }
    private void Start()
    {
        for(int i = 0; i<Size; i++)
        {
            for (int j = 0; j< Size; j++)
            {
                GameMatrix[i, j] = 0;
            }
        }
        
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while(endPosx== startPosx || endPosy== startPosy);

        GameMatrix[startPosx, startPosy] = 2;
        GameMatrix[startPosx, startPosy] = 1;
        NodeMatrix = new Node[Size, Size];
        CreateNodes();
        PathFindingAStar();
    }
    public void CreateNodes()
    {
        for(int i=0; i<Size; i++)
        {
            for(int j=0; j<Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i,j));
                NodeMatrix[i,j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i,j],endPosx,endPosy);
            }
        }
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }
      //  DebugMatrix();
    }
    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Instantiate(token, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                Debug.Log("Element (" + j + ", " + i + ")");
                Debug.Log("Position " + NodeMatrix[i, j].RealPosition);
                Debug.Log("Heuristic " + NodeMatrix[i, j].Heuristic);
                Debug.Log("Ways: ");
                foreach (var way in NodeMatrix[i, j].WayList)
                {
                    Debug.Log(" (" + way.NodeDestiny.PositionX + ", " + way.NodeDestiny.PositionY + ")");
                }
            }
        }
    }
    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(x<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(y>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x>0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x<Size-1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }
    public void PathFindingAStar()
    {
        Node startNode = NodeMatrix[startPosx, startPosy];
        Node finalNode = NodeMatrix[endPosx, endPosy];

        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        startNode.GCost = 0;
        openList.Add(startNode);
        createToken(startPosx, startPosy, start_finalToken);
        //token.GetComponent<SpriteRenderer>().color = Color.magenta;

        while (openList.Count>0) 
        {
            Node currentNode = openList.OrderBy(n => n.FCost).First();
            if (currentNode == finalNode)
            {
                createToken(currentNode.PositionX, currentNode.PositionY, start_finalToken);

                Node temp = currentNode.NodeParent;
                while (temp != startNode && temp != null)
                {
                    createToken(temp.PositionX, temp.PositionY, wayToken);
                    temp = temp.NodeParent;
                }
                Debug.Log("¡Acabado!");
                return;
            }
            closedList.Add(currentNode);
            openList.Remove(currentNode);

            foreach (var way in currentNode.WayList)
            {
                if (closedList.Contains(way.NodeDestiny)) continue;

                float _newCost = way.Cost + currentNode.GCost;
                if (_newCost < way.NodeDestiny.GCost)
                {
                    way.NodeDestiny.GCost = _newCost;
                    way.NodeDestiny.NodeParent = currentNode;

                    if (!openList.Contains(way.NodeDestiny))
                    {
                        openList.Add(way.NodeDestiny);   
                        createToken(way.NodeDestiny.PositionX, way.NodeDestiny.PositionY, token);
                    }
                }
            }
        }

    }
    public void createToken(int posx, int posy, GameObject token)
    {
        Vector3 worldPos = NodeMatrix[posx, posy].RealPosition;
        Instantiate(token, worldPos, Quaternion.identity);
    }
}
