using System;

namespace _Ashfall._Scripts.Gameplay.PuzzleLogic
{
    public class BoardLogic
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TokenModel[,] Grid { get; private set; }
        
        public BoardLogic(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new TokenModel[width, height];
        }

        public void InitializeBoard()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Grid[x, y] = new TokenModel(x, y, GetRandomBaseToken());
                }
            }
        }

        private TokenType GetRandomBaseToken()
        {
            Array values = Enum.GetValues(typeof(TokenType));
            Random random = new Random();
            
            return (TokenType)values.GetValue(random.Next(1, values.Length));
        }
    }
}