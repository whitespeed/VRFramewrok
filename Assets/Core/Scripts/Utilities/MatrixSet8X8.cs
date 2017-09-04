using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allow assse 8 x 8 matrix by single index
/// </summary>
public class MatrixSet8X8
{
    public enum SplitLineType
    {
        None, Verticle, Horziontal
    }

    public SplitLineType GridSpliter;
    int[,] container = new int[8, 8];


    public MatrixSet8X8()
    {
        for (int i = 0; i < 64; i++)
        {
            this[i] = int.MinValue;
        }
    }

    public void SetMatrix(Dictionary<int, int> ValueMap)
    {
        foreach (var key in ValueMap.Keys)
        {
            if (key >= 0 && key < 64)
            {
                this[key] = ValueMap[key];
            }
            else
            {
                Debug.LogErrorFormat("Invaild Key {0}, key is out of range", key);
                continue;
            }
        }
    }

    public int this[int index1, int index2]
    {
        get
        {
            return container[index1, index2];
        }
        set
        {
            container[index1, index2] = value;
        }
    }

    public int this[int index]
    {
        get
        {
            if (index < 0 || index >= 64)
            {
                Debug.LogError("Invaild index");
                return 0;
            }
            else
            {
                int x = index / 8;
                int y = index % 8;
                return container[x, y];
            }
        }
        set
        {
            if (index < 0 || index >= 64)
                Debug.LogError("Invaild index");
            else
            {
                int x = index / 8;
                int y = index % 8;
                container[x, y] = value;
            }
        }
    }

    public int GetIndexFromData(int data)
    {
        int id = -1;
        for (int i = 0; i < 64; i++)
        {
            if (this[i] == data)
            {
                id = i;
                break;
            }
        }
        return id;
    }

    public int GetRelativeIndex(int index)
    {
        if (GridSpliter == SplitLineType.Horziontal)
        {
            return (index - 32) > 0 ? index - 32 : index + 32;
        }
        else if (GridSpliter == SplitLineType.Verticle)
        {
            if (index % 8 > 4 || (index % 8 == 0 && index != 0))
            {
                return index - 4;
            }
            else
            {
                return index + 4;
            }
        }
        return index;
    }
}
