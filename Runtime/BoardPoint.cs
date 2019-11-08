namespace JackSParrot.Navigation.AStar
{
    public struct BoardPoint
    {
        public int index { get; private set; }
        public int x { get; private set; }
        public int y { get; private set; }

        public void Set(int idx, int boardCols)
        {
            index = idx;
            x = idx % boardCols;
            y = idx / boardCols;
        }

        public void Set(int posX, int posY, int boardCols)
        {
            x = posX;
            y = posY;
            index = y * boardCols + x;
        }
    }
}