using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.PuzzleLogic
{
    public class BoardDebugger : MonoBehaviour
    {
        [Header("Board Settings")]
        public int width = 7;
        public int height = 7;

        private BoardLogic _boardLogic;

        private void Start()
        {
            // Khởi tạo cỗ máy Logic
            _boardLogic = new BoardLogic(width, height);
            _boardLogic.InitializeBoard();

            // In kết quả ra Console
            PrintBoardToConsole();
        }

        private void PrintBoardToConsole()
        {
            string debugText = $"--- BẢN ĐỒ MATCH-3 ({width}x{height}) ---\n";

            // Vòng lặp Y phải chạy ngược từ trên xuống dưới để Console in ra đúng chiều (đỉnh ở trên, đáy ở dưới)
            for (int y = _boardLogic.Height - 1; y >= 0; y--)
            {
                string rowText = "";
                for (int x = 0; x < _boardLogic.Width; x++)
                {
                    TokenType type = _boardLogic.Grid[x, y].Type;
                
                    // Lấy chữ cái đầu tiên để in cho gọn (S = Sword, H = Shield, O = Orb)
                    string shortName = type == TokenType.Sword ? "[⚔️]" : 
                        type == TokenType.Shield ? "[🛡️]" : 
                        type == TokenType.Orb ? "[🔮]" : "[  ]";
                
                    rowText += shortName + " ";
                }
                debugText += rowText + "\n";
            }

            Debug.Log(debugText);
        }
    }
}