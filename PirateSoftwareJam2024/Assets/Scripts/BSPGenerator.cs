using UnityEngine;
using System.Collections.Generic;

public class BSPGenerator : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public int minRoomSize = 10;
    public int maxRoomSize = 20;
    public int gridSize = 1; // Grid size for snapping
    public GameObject tilePrefab; // Prefab for the tiles used to draw the rooms and corridors
    public Transform tileParent; // Parent transform to keep the hierarchy organized

    private BSPNode root;
    private List<BSPNode> rooms = new List<BSPNode>(); // List to store all rooms

    private void Start()
    {
        root = new BSPNode(new RectInt(0, 0, width, height));
        SplitSpace(root);
        CreateRooms(root);
        ConnectRoomsSequentially();
    }

    private void SplitSpace(BSPNode node)
    {
        if (node.rect.width <= minRoomSize * 2 && node.rect.height <= minRoomSize * 2) return;

        bool splitHorizontally = node.rect.width > node.rect.height;
        if (node.rect.width == node.rect.height) splitHorizontally = Random.value > 0.5f;

        int max = (splitHorizontally ? node.rect.width : node.rect.height) - minRoomSize;
        if (max <= minRoomSize) return;

        int split = Random.Range(minRoomSize, max);
        if (splitHorizontally)
        {
            node.left = new BSPNode(new RectInt(node.rect.x, node.rect.y, split, node.rect.height));
            node.right = new BSPNode(new RectInt(node.rect.x + split, node.rect.y, node.rect.width - split, node.rect.height));
        }
        else
        {
            node.left = new BSPNode(new RectInt(node.rect.x, node.rect.y, node.rect.width, split));
            node.right = new BSPNode(new RectInt(node.rect.x, node.rect.y + split, node.rect.width, node.rect.height - split));
        }

        SplitSpace(node.left);
        SplitSpace(node.right);
    }

    private void CreateRooms(BSPNode node)
    {
        if (node.IsLeaf())
        {
            int roomWidth = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, node.rect.width - 2));
            int roomHeight = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, node.rect.height - 2));
            int roomX = Random.Range(1, node.rect.width - roomWidth - 1);
            int roomY = Random.Range(1, node.rect.height - roomHeight - 1);
            node.room = new RectInt(
                Mathf.RoundToInt((node.rect.x + roomX) / gridSize) * gridSize,
                Mathf.RoundToInt((node.rect.y + roomY) / gridSize) * gridSize,
                roomWidth,
                roomHeight
            );

            InstantiateRoom((Vector2Int)node.room.position, node.room.size);
            rooms.Add(node); // Add the room to the list
        }
        else
        {
            CreateRooms(node.left);
            CreateRooms(node.right);
        }
    }

    private void ConnectRoomsSequentially()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int currentRoomCenter = Vector2Int.RoundToInt(rooms[i].room.center);
            Vector2Int nextRoomCenter = Vector2Int.RoundToInt(rooms[i + 1].room.center);

            if (Random.value > 0.5f)
            {
                // Horizontal first
                CreateHorizontalCorridor(currentRoomCenter.x, nextRoomCenter.x, currentRoomCenter.y);
                CreateVerticalCorridor(currentRoomCenter.y, nextRoomCenter.y, nextRoomCenter.x);
            }
            else
            {
                // Vertical first
                CreateVerticalCorridor(currentRoomCenter.y, nextRoomCenter.y, currentRoomCenter.x);
                CreateHorizontalCorridor(currentRoomCenter.x, nextRoomCenter.x, nextRoomCenter.y);
            }
        }
    }

    private void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        int start = Mathf.Min(x1, x2);
        int end = Mathf.Max(x1, x2);
        for (int x = start; x <= end; x += gridSize)
        {
            Vector3 position = new Vector3(x, y, 0);
            Instantiate(tilePrefab, position, Quaternion.identity, tileParent);
        }
    }

    private void CreateVerticalCorridor(int y1, int y2, int x)
    {
        int start = Mathf.Min(y1, y2);
        int end = Mathf.Max(y1, y2);
        for (int y = start; y <= end; y += gridSize)
        {
            Vector3 position = new Vector3(x, y, 0);
            Instantiate(tilePrefab, position, Quaternion.identity, tileParent);
        }
    }

    private void InstantiateRoom(Vector2Int position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 tilePosition = new Vector3(position.x + x, position.y + y, 0);
                Instantiate(tilePrefab, tilePosition, Quaternion.identity, tileParent);
            }
        }
    }
}
