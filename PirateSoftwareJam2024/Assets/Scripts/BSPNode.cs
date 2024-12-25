using UnityEngine;

public class BSPNode
{
    public RectInt rect;
    public BSPNode left, right;
    public RectInt room;

    public BSPNode(RectInt rect)
    {
        this.rect = rect;
    }

    public bool IsLeaf() => left == null && right == null;
}
