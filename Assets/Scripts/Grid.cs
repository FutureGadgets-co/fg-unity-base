﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void unityToFlutter(string handlerName, string message);
    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };

    [System.Serializable]
    public struct PiecePosition
    {
        public PieceType type;
        public int x;
        public int y;
    };

    public int xDim;
    public int yDim;
    public float fillTime;

    // TODO Get automatically or at least make an assertion
    public Level level;

    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;

    public PiecePosition[] initialPieces;

    private Dictionary<PieceType, GameObject> _piecePrefabDict;

    private GamePiece[,] _pieces;

    private bool _inverse = false;

    private GamePiece _pressedPiece;
    private GamePiece _enteredPiece;

    private bool _gameOver;

    private bool _isFilling;

    public bool IsFilling => _isFilling;

    private int steps = 0;

    private void Awake()
    {
        // populating dictionary with piece prefabs types
        _piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!_piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                _piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        // instantiate backgrounds
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
            }
        }

        // instantiating pieces
        _pieces = new GamePiece[xDim, yDim];

        for (int i = 0; i < initialPieces.Length; i++)
        {
            if (initialPieces[i].x >= 0 && initialPieces[i].y < xDim
                && initialPieces[i].y >=0 && initialPieces[i].y <yDim)
            {
                SpawnNewPiece(initialPieces[i].x, initialPieces[i].y, initialPieces[i].type);
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (_pieces[x, y] == null)
                {
                    SpawnNewPiece(x, y, PieceType.EMPTY);
                }                
            }
        }

        StartCoroutine(Fill());
    }

    private IEnumerator Fill()
    {        
        bool needsRefil = true;
        _isFilling = true;

        while (needsRefil)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {
                _inverse = !_inverse;
                yield return new WaitForSeconds(fillTime);
            }

            needsRefil = ClearAllValidMatches();
        }

        _isFilling = false;
    }

    /// <summary>
    /// One pass through all grid cells, moving them down one grid, if possible.
    /// </summary>
    /// <returns> returns true if at least one piece is moved down</returns>
    private bool FillStep()
    {
        bool movedPiece = false;
        // y = 0 is at the top, we ignore the last row, since it can't be moved down.
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;
                if (_inverse) { x = xDim - 1 - loopX; }
                GamePiece piece = _pieces[x, y];

                if (!piece.IsMovable()) continue;
                
                GamePiece pieceBelow = _pieces[x, y + 1];

                if (pieceBelow.Type == PieceType.EMPTY)
                {
                    Destroy(pieceBelow.gameObject);
                    piece.MovableComponent.Move(x, y + 1, fillTime);
                    _pieces[x, y + 1] = piece;
                    SpawnNewPiece(x, y, PieceType.EMPTY);
                    movedPiece = true;
                }
                else
                {
                    for (int diag = -1; diag <= 1; diag++)
                    {
                        if (diag == 0) continue;
                        
                        int diagX = x + diag;

                        if (_inverse)
                        {
                            diagX = x - diag;
                        }

                        if (diagX < 0 || diagX >= xDim) continue;
                        
                        GamePiece diagonalPiece = _pieces[diagX, y + 1];

                        if (diagonalPiece.Type != PieceType.EMPTY) continue;
                        
                        bool hasPieceAbove = true;

                        for (int aboveY = y; aboveY >= 0; aboveY--)
                        {
                            GamePiece pieceAbove = _pieces[diagX, aboveY];

                            if (pieceAbove.IsMovable())
                            {
                                break;
                            }
                            else if (/*!pieceAbove.IsMovable() && */pieceAbove.Type != PieceType.EMPTY)
                            {
                                hasPieceAbove = false;
                                break;
                            }
                        }

                        if (hasPieceAbove) continue;
                        
                        Destroy(diagonalPiece.gameObject);
                        piece.MovableComponent.Move(diagX, y + 1, fillTime);
                        _pieces[diagX, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                        break;
                    }
                }
            }
        }

        // the highest row (0) is a special case, we must fill it with new pieces if empty
        for (int x = 0; x < xDim; x++)
        {
            GamePiece pieceBelow = _pieces[x, 0];

            if (pieceBelow.Type != PieceType.EMPTY) continue;
            
            Destroy(pieceBelow.gameObject);
            GameObject newPiece = (GameObject)Instantiate(_piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity, this.transform);

            _pieces[x, 0] = newPiece.GetComponent<GamePiece>();
            _pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
            _pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
            _pieces[x, 0].ColorComponent.SetColor((ColorType)Random.Range(0, _pieces[x, 0].ColorComponent.NumColors));
            movedPiece = true;
        }

        return movedPiece;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2.0f + x,
                           transform.position.y + yDim / 2.0f - y);
    }

    private GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(_piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity, this.transform);
        _pieces[x, y] = newPiece.GetComponent<GamePiece>();
        _pieces[x, y].Init(x, y, this, type);

        return _pieces[x, y];
    }

    private static bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X==piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1)
            || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    private void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (_gameOver) { return; }

        if (!piece1.IsMovable() || !piece2.IsMovable()) return;
        
        _pieces[piece1.X, piece1.Y] = piece2;
        _pieces[piece2.X, piece2.Y] = piece1;

        if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null
                                                         || piece1.Type == PieceType.RAINBOW || piece2.Type == PieceType.RAINBOW)
        {
            int piece1X = piece1.X;
            int piece1Y = piece1.Y;

            piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
            piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

            if (piece1.Type == PieceType.RAINBOW && piece1.IsClearable() && piece2.IsColored())
            {
                ClearColorPiece clearColor = piece1.GetComponent<ClearColorPiece>();

                if (clearColor)
                {
                    clearColor.Color = piece2.ColorComponent.Color;
                }

                ClearPiece(piece1.X, piece1.Y);
            }

            if (piece2.Type == PieceType.RAINBOW && piece2.IsClearable() && piece1.IsColored())
            {
                ClearColorPiece clearColor = piece2.GetComponent<ClearColorPiece>();

                if (clearColor)
                {
                    clearColor.Color = piece1.ColorComponent.Color;
                }

                ClearPiece(piece2.X, piece2.Y);
            }
            
            bool r = ClearAllValidMatches();
            if (r)
            {
                steps += 1;
                if (steps >= 3)
                {
                    unityToFlutter("onUnityMessage", $"{{\"gameResult\":\"1\"}}");
                    steps = 0;
                }
            }

            // special pieces get cleared, event if they are not matched
            if (piece1.Type == PieceType.ROW_CLEAR || piece1.Type == PieceType.COLUMN_CLEAR)
            {
                ClearPiece(piece1.X, piece1.Y);
            }

            if (piece2.Type == PieceType.ROW_CLEAR || piece2.Type == PieceType.COLUMN_CLEAR)
            {
                ClearPiece(piece2.X, piece2.Y);
            }

            _pressedPiece = null;
            _enteredPiece = null;

            StartCoroutine(Fill());

            // TODO consider doing this using delegates
            level.OnMove();
        }
        else
        {
            _pieces[piece1.X, piece1.Y] = piece1;
            _pieces[piece2.X, piece2.Y] = piece2;
        }
    }

    public void PressPiece(GamePiece piece)
    {
        _pressedPiece = piece;
    }

    public void EnterPiece(GamePiece piece)
    {
        _enteredPiece = piece;
    }

    public void ReleasePiece()
    {
        if (IsAdjacent (_pressedPiece, _enteredPiece))
        {
            SwapPieces(_pressedPiece, _enteredPiece);
        }
    }

    private bool ClearAllValidMatches()
    {
        bool needsRefill = false;

        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (!_pieces[x, y].IsClearable()) continue;
                
                List<GamePiece> match = GetMatch(_pieces[x, y], x, y);

                if (match == null) continue;
                
                PieceType specialPieceType = PieceType.COUNT;
                GamePiece randomPiece = match[Random.Range(0, match.Count)];
                int specialPieceX = randomPiece.X;
                int specialPieceY = randomPiece.Y;

                // Spawning special pieces
                if (match.Count == 4)
                {
                    if (_pressedPiece == null || _enteredPiece == null)
                    {
                        specialPieceType = (PieceType) Random.Range((int) PieceType.ROW_CLEAR, (int) PieceType.COLUMN_CLEAR);
                    }
                    else if (_pressedPiece.Y == _enteredPiece.Y)
                    {
                        specialPieceType = PieceType.ROW_CLEAR;
                    }
                    else
                    {
                        specialPieceType = PieceType.COLUMN_CLEAR;
                    }
                } // Spawning a rainbow piece
                else if (match.Count >= 5)
                {
                    specialPieceType = PieceType.RAINBOW;
                }

                for (int i = 0; i < match.Count; i++)
                {
                    if (!ClearPiece(match[i].X, match[i].Y)) continue;
                    
                    needsRefill = true;

                    if (match[i] != _pressedPiece && match[i] != _enteredPiece) continue;
                    
                    specialPieceX = match[i].X;
                    specialPieceY = match[i].Y;
                }

                // Setting their colors
                if (specialPieceType == PieceType.COUNT) continue;
                
                Destroy(_pieces[specialPieceX, specialPieceY]);
                GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);

                if ((specialPieceType == PieceType.ROW_CLEAR || specialPieceType == PieceType.COLUMN_CLEAR) 
                    && newPiece.IsColored() && match[0].IsColored())
                {
                    newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                }
                else if (specialPieceType == PieceType.RAINBOW && newPiece.IsColored())
                {
                    newPiece.ColorComponent.SetColor(ColorType.ANY);
                }
            }
        }

        return needsRefill;
    }

    private List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (!piece.IsColored()) return null;
        var color = piece.ColorComponent.Color;
        var horizontalPieces = new List<GamePiece>();
        var verticalPieces = new List<GamePiece>();
        var matchingPieces = new List<GamePiece>();

        // First check horizontal
        horizontalPieces.Add(piece);

        for (int dir = 0; dir <= 1; dir++)
        {
            for (int xOffset = 1; xOffset < xDim; xOffset++)
            {
                int x;

                if (dir == 0)
                { // Left
                    x = newX - xOffset;
                }
                else
                { // right
                    x = newX + xOffset;                        
                }

                // out-of-bounds
                if (x < 0 || x >= xDim) { break; }

                // piece is the same color?
                if (_pieces[x, newY].IsColored() && _pieces[x, newY].ColorComponent.Color == color)
                {
                    horizontalPieces.Add(_pieces[x, newY]);
                }
                else
                {
                    break;
                }
            }
        }

        if (horizontalPieces.Count >= 3)
        {
            for (int i = 0; i < horizontalPieces.Count; i++)
            {
                matchingPieces.Add(horizontalPieces[i]);
            }
        }

        // Traverse vertically if we found a match (for L and T shape)
        if (horizontalPieces.Count >= 3)
        {
            for (int i = 0; i < horizontalPieces.Count; i++ )
            {
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int yOffset = 1; yOffset < yDim; yOffset++)                        
                    {
                        int y;
                            
                        if (dir == 0)
                        { // Up
                            y = newY - yOffset;
                        }
                        else
                        { // Down
                            y = newY + yOffset;
                        }

                        if (y < 0 || y >= yDim)
                        {
                            break;
                        }

                        if (_pieces[horizontalPieces[i].X, y].IsColored() && _pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
                        {
                            verticalPieces.Add(_pieces[horizontalPieces[i].X, y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (verticalPieces.Count < 2)
                {
                    verticalPieces.Clear();
                }
                else
                {
                    for (int j = 0; j < verticalPieces.Count; j++)
                    {
                        matchingPieces.Add(verticalPieces[j]);
                    }
                    break;
                }
            }
        }

        if (matchingPieces.Count >= 3)
        {
            return matchingPieces;
        }


        // Didn't find anything going horizontally first,
        // so now check vertically
        horizontalPieces.Clear();
        verticalPieces.Clear();
        verticalPieces.Add(piece);

        for (int dir = 0; dir <= 1; dir++)
        {
            for (int yOffset = 1; yOffset < xDim; yOffset++)
            {
                int y;

                if (dir == 0)
                { // Up
                    y = newY - yOffset;
                }
                else
                { // Down
                    y = newY + yOffset;                        
                }

                // out-of-bounds
                if (y < 0 || y >= yDim) { break; }

                // piece is the same color?
                if (_pieces[newX, y].IsColored() && _pieces[newX, y].ColorComponent.Color == color)
                {
                    verticalPieces.Add(_pieces[newX, y]);
                }
                else
                {
                    break;
                }
            }
        }

        if (verticalPieces.Count >= 3)
        {
            for (int i = 0; i < verticalPieces.Count; i++)
            {
                matchingPieces.Add(verticalPieces[i]);
            }
        }

        // Traverse horizontally if we found a match (for L and T shape)
        if (verticalPieces.Count >= 3)
        {
            for (int i = 0; i < verticalPieces.Count; i++)
            {
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int xOffset = 1; xOffset < yDim; xOffset++)
                    {
                        int x;

                        if (dir == 0)
                        { // Left
                            x = newX - xOffset;
                        }
                        else
                        { // Right
                            x = newX + xOffset;
                        }

                        if (x < 0 || x >= xDim)
                        {
                            break;
                        }

                        if (_pieces[x, verticalPieces[i].Y].IsColored() && _pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
                        {
                            horizontalPieces.Add(_pieces[x, verticalPieces[i].Y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (horizontalPieces.Count < 2)
                {
                    horizontalPieces.Clear();
                }
                else
                {
                    for (int j = 0; j < horizontalPieces.Count; j++)
                    {
                        matchingPieces.Add(horizontalPieces[j]);
                    }
                    break;
                }
            }
        }

        if (matchingPieces.Count >= 3)
        {
            return matchingPieces;
        }

        return null;
    }

    private bool ClearPiece(int x, int y)
    {
        if (!_pieces[x, y].IsClearable() || _pieces[x, y].ClearableComponent.IsBeingCleared) return false;
        
        _pieces[x, y].ClearableComponent.Clear();
        SpawnNewPiece(x, y, PieceType.EMPTY);

        ClearObstacles(x, y);

        return true;

    }

    private void ClearObstacles(int x, int y)
    {
        for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
        {
            if (adjacentX == x || adjacentX < 0 || adjacentX >= xDim) continue;

            if (_pieces[adjacentX, y].Type != PieceType.BUBBLE || !_pieces[adjacentX, y].IsClearable()) continue;
            
            _pieces[adjacentX, y].ClearableComponent.Clear();
            SpawnNewPiece(adjacentX, y, PieceType.EMPTY);
        }

        for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
        {
            if (adjacentY == y || adjacentY < 0 || adjacentY >= yDim) continue;

            if (_pieces[x, adjacentY].Type != PieceType.BUBBLE || !_pieces[x, adjacentY].IsClearable()) continue;
            
            _pieces[x, adjacentY].ClearableComponent.Clear();
            SpawnNewPiece(x, adjacentY, PieceType.EMPTY);
        }
    }

    public void ClearRow(int row)
    {
        for (int x = 0; x < xDim; x++)
        {
            ClearPiece(x, row);
        }
    }

    public void ClearColumn(int column)
    {
        for (int y = 0; y < yDim; y++)
        {
            ClearPiece(column, y);
        }
    }

    public void ClearColor(ColorType color)
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if ((_pieces[x, y].IsColored() && _pieces[x, y].ColorComponent.Color == color)
                    || (color == ColorType.ANY))
                {
                    ClearPiece(x, y);
                }
            }
        }
    }

    public void GameOver()
    {
        _gameOver = true;
    }

    public List<GamePiece> GetPiecesOfType(PieceType type)
    {
        var piecesOfType = new List<GamePiece>();

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (_pieces[x, y].Type == type)
                {
                    piecesOfType.Add(_pieces[x, y]);
                }
            }
        }

        return piecesOfType;
    }

}
