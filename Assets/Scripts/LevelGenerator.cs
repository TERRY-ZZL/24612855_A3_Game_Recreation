using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    // 八地图块
    public GameObject outsideCorner;
    public GameObject outsideWall;
    public GameObject insideCorner;
    public GameObject insideWall;
    public GameObject pellet;
    public GameObject powerPellet;
    public GameObject tJunction;
    public GameObject ghostGate;

    public GameObject sceneMap;
    public Camera mapCamera;







    // 左上象限
    public int[,] levelMap =
    {
        { 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
        { 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4 },
        { 2, 5, 3, 4, 4, 3, 5, 3, 4, 4, 4, 3, 5, 4 },
        { 2, 6, 4, 0, 0, 4, 5, 4, 0, 0, 0, 4, 5, 4 },
        { 2, 5, 3, 4, 4, 3, 5, 3, 4, 4, 4, 3, 5, 3 },
        { 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 },
        { 2, 5, 3, 4, 4, 3, 5, 3, 3, 5, 3, 4, 4, 4 },
        { 2, 5, 3, 4, 4, 3, 5, 4, 4, 5, 3, 4, 4, 3 },
        { 2, 5, 5, 5, 5, 5, 5, 4, 4, 5, 5, 5, 5, 4 },
        { 1, 2, 2, 2, 2, 1, 5, 4, 3, 4, 4, 3, 0, 4 },
        { 0, 0, 0, 0, 0, 2, 5, 4, 3, 4, 4, 3, 0, 3 },
        { 0, 0, 0, 0, 0, 2, 5, 4, 4, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 5, 4, 4, 0, 3, 4, 4, 8 },
        { 2, 2, 2, 2, 2, 1, 5, 3, 3, 0, 4, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0 }
    };



        // 按编号选预制体
        GameObject GetPrefab(int number)
    {
        if (number == 1)
        {
            return outsideCorner;
        }
        else if (number == 2)
        {
            return outsideWall;
        }
        else if (number == 3)
        {
            return insideCorner;
        }
        else if (number == 4)
        {
            return insideWall;
        }
        else if (number == 5)
        {
            return pellet;
        }
        else if (number == 6)
        {
            return powerPellet;
        }
        else if (number == 7)
        {
            return tJunction;
        }
        else if (number == 8)
        {
            return ghostGate;
        }
        else
        {
            return null;
        }
    }

    // 读取完整地图，镜像位置折回左上象限
    int GetMapValue(int row, int column)
    {
        int rowCount = levelMap.GetLength(0);
        int columnCount = levelMap.GetLength(1);

        int fullHeight = rowCount * 2 - 1;
        int fullWidth = columnCount * 2;

        if (row < 0 || row >= fullHeight)
        {
            return -1;
        }

        if (column < 0 || column >= fullWidth)
        {
            return -1;
        }

        if (row >= rowCount)
        {
            row = fullHeight - 1 - row;
        }

        if (column >= columnCount)
        {
            column = fullWidth - 1 - column;
        }

        return levelMap[row, column];
    }



        // 顺序：上、右、下、左
        private int[] rowStep = { -1, 0, 1, 0 };
    private int[] columnStep = { 0, 1, 0, -1 };

    // 看接什么：0不接，1外墙，2内墙
    int GetConnection(int number, int turn, int direction)
    {
        // turn每加1，图转90度
        int side = (direction + turn) % 4;

        if (number == 1)
        {
            if (side == 1 || side == 2)
            {
                return 1;
            }
        }
        else if (number == 2)
        {
            if (side == 1 || side == 3)
            {
                return 1;
            }
        }
        else if (number == 3)
        {
            if (side == 1 || side == 2)
            {
                return 2;
            }
        }
        else if (number == 4 || number == 8)
        {
            if (side == 1 || side == 3)
            {
                return 2;
            }
        }
        else if (number == 7)
        {
            if (side == 1 || side == 3)
            {
                return 1;
            }
            else if (side == 2)
            {
                return 2;
            }
        }

        return 0;
    }

    

    // 对两边的连接类型
    bool CanMatchNeighbour(
        int row,
        int column,
        int turn,
        int direction,
        bool[,,] possible)
    {
        int number = GetMapValue(row, column);
        int connection = GetConnection(number, turn, direction);

        int nextRow = row + rowStep[direction];
        int nextColumn = column + columnStep[direction];

        int nextNumber = GetMapValue(nextRow, nextColumn);

        if (nextNumber == -1)
        {
            return connection == 0 || number == 2;
        }

        int nextDirection = (direction + 2) % 4;

        for (int nextTurn = 0; nextTurn < 4; nextTurn++)
        {
            if (possible[nextRow, nextColumn, nextTurn])
            {
                int nextConnection = GetConnection(
                    nextNumber,
                    nextTurn,
                    nextDirection);

                if (connection == nextConnection)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // 先留可能的，再逐轮排除接不上的方向
    int[,] FindWallDirections()
    {
        int fullHeight = levelMap.GetLength(0) * 2 - 1;
        int fullWidth = levelMap.GetLength(1) * 2;

        // 行、列和四种转向
        bool[,,] possible = new bool[fullHeight, fullWidth, 4];

        for (int row = 0; row < fullHeight; row++)
        {
            for (int column = 0; column < fullWidth; column++)
            {
                int number = GetMapValue(row, column);

                possible[row, column, 0] = true;

                if (number == 1 || number == 3 || number == 7)
                {
                    possible[row, column, 1] = true;
                    possible[row, column, 2] = true;
                    possible[row, column, 3] = true;
                }
                else if (number == 2 || number == 4 || number == 8)
                {
                    possible[row, column, 1] = true;
                }
            }
        }

        bool changed = true;

        while (changed)
        {
            changed = false;

            for (int row = 0; row < fullHeight; row++)
            {
                for (int column = 0; column < fullWidth; column++)
                {
                    for (int turn = 0; turn < 4; turn++)
                    {
                        if (!possible[row, column, turn])
                        {
                            continue;
                        }

                        for (int direction = 0; direction < 4; direction++)
                        {
                            if (!CanMatchNeighbour(
                                row,
                                column,
                                turn,
                                direction,
                                possible))
                            {
                                possible[row, column, turn] = false;
                                changed = true;
                                break;
                            }
                        }
                    }
                }
            }
        }



        // 每格只留一个确定的角度
        int[,] wallDirections = new int[fullHeight, fullWidth];

        for (int row = 0; row < fullHeight; row++)
        {
            for (int column = 0; column < fullWidth; column++)
            {
                int directionCount = 0;
                int angle = 0;

                for (int turn = 0; turn < 4; turn++)
                {
                    if (possible[row, column, turn])
                    {
                        directionCount = directionCount + 1;
                        angle = turn * 90;
                    }
                }

                if (directionCount == 0)
                {
                    throw new System.InvalidOperationException(
                        "Invalid wall connections at row " + row +
                        ", column " + column);
                }
                else if (directionCount > 1)
                {
                    throw new System.InvalidOperationException(
                        "Ambiguous wall direction at row " + row +
                        ", column " + column);
                }

                wallDirections[row, column] = angle;
            }
        }

        return wallDirections;
    }


        // 沿竖墙找拐角或T接点，判断纹路朝哪边，不该画不对称的（挠头）
        int GetOutsideWallAngle(int row, int column, int[,] wallDirections)
    {
        if (wallDirections[row, column] % 180 == 0)
        {
            return 0;
        }

        int fullHeight = wallDirections.GetLength(0);
        int fullWidth = wallDirections.GetLength(1);

        for (int rowDirection = -1; rowDirection <= 1; rowDirection += 2)
        {
            for (int checkRow = row + rowDirection;
                checkRow >= 0 && checkRow < fullHeight;
                checkRow += rowDirection)
            {
                int number = GetMapValue(checkRow, column);
                int turn = wallDirections[checkRow, column] / 90;

                if (number == 1 || number == 7)
                {
                    int sideType = 1;

                    if (number == 7)
                    {
                        sideType = 2;
                    }

                    if (GetConnection(number, turn, 1) == sideType)
                    {
                        return 270;
                    }
                    else if (GetConnection(number, turn, 3) == sideType)
                    {
                        return 90;
                    }

                    break;
                }

                if (number != 2 ||
                    wallDirections[checkRow, column] % 180 == 0)
                {
                    break;
                }
            }
        }

        if (column == 0)
        {
            return 270;
        }
        else if (column == fullWidth - 1)
        {
            return 90;
        }

        throw new System.InvalidOperationException(
            "Ambiguous outside wall side at row " + row +
            ", column " + column);
    }

    // 建一组地图块，结束行不包含在内
    void CreateMapPart(
        Transform mapRoot,
        string partName,
        Vector3 position,
        Vector3 scale,
        int firstRow,
        int endRow,
        int[,] wallDirections)
    {
        GameObject part = new GameObject(partName);

        part.transform.SetParent(mapRoot, false);
        part.transform.localPosition = position;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = scale;

        int columnCount = levelMap.GetLength(1);

        for (int row = firstRow; row < endRow; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                int number = levelMap[row, column];

                if (number == 0)
                {
                    continue;
                }

                GameObject prefab = GetPrefab(number);
                GameObject tile = Instantiate(prefab, part.transform, false);

                tile.name = "R" + row.ToString("D2") +
                    "_C" + column.ToString("D2") +
                    "_" + prefab.name;

                // 列向右，行向下
                tile.transform.localPosition =
                    new Vector3(column, -row, 0f);

                tile.transform.localRotation =
                    Quaternion.Euler(0f, 0f, wallDirections[row, column]);

                tile.transform.localScale = Vector3.one;
            }
        }
    }

    void Start()
    {
        int rowCount = levelMap.GetLength(0);
        int columnCount = levelMap.GetLength(1);

        int fullHeight = rowCount * 2 - 1;
        int fullWidth = columnCount * 2;

        // 先算连接方向，再确定外墙纹路
        int[,] wallDirections = FindWallDirections();

        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                if (levelMap[row, column] == 2)
                {
                    wallDirections[row, column] =
                        GetOutsideWallAngle(row, column, wallDirections);
                }
            }
        }

        // 新地图先不显示
        GameObject generatedMap = new GameObject("Level01_Generated");
        generatedMap.SetActive(false);

        float rightX = fullWidth - 1;
        float bottomY = -(fullHeight - 1);

        // 四个象限用父对象翻转
        CreateMapPart(
            generatedMap.transform,
            "Quadrant_TopLeft",
            Vector3.zero,
            Vector3.one,
            0,
            rowCount - 1,
            wallDirections);

        CreateMapPart(
            generatedMap.transform,
            "Quadrant_TopRight",
            new Vector3(rightX, 0f, 0f),
            new Vector3(-1f, 1f, 1f),
            0,
            rowCount - 1,
            wallDirections);

        CreateMapPart(
            generatedMap.transform,
            "Quadrant_BottomLeft",
            new Vector3(0f, bottomY, 0f),
            new Vector3(1f, -1f, 1f),
            0,
            rowCount - 1,
            wallDirections);

        CreateMapPart(
            generatedMap.transform,
            "Quadrant_BottomRight",
            new Vector3(rightX, bottomY, 0f),
            new Vector3(-1f, -1f, 1f),
            0,
            rowCount - 1,
            wallDirections);

        // 中心行只做左右两份
        CreateMapPart(
            generatedMap.transform,
            "CenterRow_Left",
            Vector3.zero,
            Vector3.one,
            rowCount - 1,
            rowCount,
            wallDirections);

        CreateMapPart(
            generatedMap.transform,
            "CenterRow_Right",
            new Vector3(rightX, 0f, 0f),
            new Vector3(-1f, 1f, 1f),
            rowCount - 1,
            rowCount,
            wallDirections);

        // 按地图尺寸调整摄像机
        mapCamera.orthographic = true;

        mapCamera.transform.position = new Vector3(
            (fullWidth - 1) / 2f,
            -(fullHeight - 1) / 2f,
            -10f);

        float heightSize = fullHeight / 2f + 1.5f;
        float widthSize = (fullWidth / 2f + 1.5f) / mapCamera.aspect;

        mapCamera.orthographicSize = Mathf.Max(heightSize, widthSize);

        // 先停用旧地图，再切到新地图
        sceneMap.SetActive(false);
        Destroy(sceneMap);
        generatedMap.SetActive(true);
    }
}