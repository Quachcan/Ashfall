namespace _Ashfall._Scripts.Gameplay.PuzzleLogic
{
    public enum TokenType
    {
        None = 0,
        Sword = 1,
        Shield =2,
        Orb = 3
    }
    public class TokenModel
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public TokenType Type { get; set; }
        
        public bool IsSelected { get; set; }

        public TokenModel(int x, int y, TokenType type)
        {
            X = x;
            Y = y;
            Type = type;
            IsSelected = false;
        }
        
        public  TokenModel(int newX, int newY)
        {
            X = newX;
            Y = newY;
        }
    }
}