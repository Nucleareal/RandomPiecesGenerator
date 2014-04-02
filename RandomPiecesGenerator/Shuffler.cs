using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RandomPiecesGenerator
{
    class Shuffler //画像全体
    {
        private Random _rand = new Random();

        public Shuffler(int x, int y, int sc, int xl, int yl)
        {
            XCount = x;
            YCount = y;
            SelectCount = sc;

            XLength = xl;
            YLength = yl;
        }

        private Chunk[,] _chunk;

        public Chunk this[int x, int y]
        {
            set
            {
                _chunk[y, x] = value;
            }
            get
            {
                return _chunk[y, x];
            }
        }

        public void Init()
        {
            _chunk = new Chunk[YCount, XCount];
            for (int i = 0; i < YCount; i++)
            {
                for (int j = 0; j < XCount; j++)
                {
                    this[j, i] = new Chunk();
                    this[j, i].ToX = j;
                    this[j, i].ToY = i;
                    this[j, i].FromX = j;
                    this[j, i].FromY = i;
                }
            }
        }

        private enum Direction
        {
            Left  = 14,  // 0b 11 10
            Up    = 11,  // 0b 10 11
            Right =  6,  // 0b 01 10
            Down  =  9,  // 0b 10 01
        }

        private int XDis(Direction d)
        {
            return 2 - ( ((int)d) >> 2 );
        }

        private int YDis(Direction d)
        {
            return 2 - ( ((int)d) & 3 );
        }

        public void Shuffle()
        {
            for (int i = 0; i < SelectCount; i++)
            {
                int n = 4 + _rand.Next(XCount * YCount);
                int fx = _rand.Next(XCount);
                int fy = _rand.Next(YCount);

                var dirs = new List<Direction>
                { Direction.Left, Direction.Up, Direction.Right, Direction.Down };

                for (int j = 0; j < n; j++)
                {
                    var dirc = dirs[_rand.Next(dirs.Count)];
                    int dx = XDis(dirc);
                    int dy = YDis(dirc);

                    if (Clamp(0, LastX, fx) == Clamp(0, LastX, fx + dx) && Clamp(0, LastY, fy) == Clamp(0, LastY, fy + dy))
                    {
                        dirs.Remove(dirc);
                        j--;
                        continue;
                    }

                    Swap(fx, fy, fx + dx, fy + dy);

                    fx += dx;
                    fy += dy;

                    dirs = new List<Direction> { Direction.Left, Direction.Up, Direction.Right, Direction.Down };
                }
            }
        }

        private void Swap(int x1, int y1, int x2, int y2)
        {
            Chunk c = this[x1, y1];
            this[x1, y1] = this[x2, y2];
            this[x2, y2] = c;

            this[x1, y1].ToX = x2;
            this[x1, y1].ToY = y2;
            this[x2, y2].ToX = x1;
            this[x2, y2].ToY = y1;
        }

        private int Clamp(int min, int max, int val)
        {
            return val < min ? min : max < val ? max : val;
        }

        private int XCount
        {
            set;
            get;
        }

        private int YCount
        {
            set;
            get;
        }

        private int XLength
        {
            set;
            get;
        }

        private int YLength
        {
            set;
            get;
        }

        private int SelectCount
        {
            set;
            get;
        }

        private int LastX
        {
            get { return XCount - 1; }
        }

        private int LastY
        {
            get { return YCount - 1; }
        }

        public void CopyImage(Bitmap from, Bitmap to, int bx, int by)
        {
            //Console.WriteLine("Div:{0} {1}, Length:{2} {3}", XCount, YCount, XLength, YLength);

            for (int i = 0; i < XLength; i++)
            {
                for (int j = 0; j < YLength; j++)
                {
                    int ix = bx * XLength + i;
                    int iy = by * YLength + j;
                    int dx = this[bx, by].FromX * XLength + i;
                    int dy = this[bx, by].FromY * YLength + j;
                    to.SetPixel(ix, iy, from.GetPixel(dy, dx));
                }
            }
        }

        public class Chunk //断片画像
        {
            public int FromX
            {
                set;
                get;
            }

            public int FromY
            {
                set;
                get;
            }

            public int ToX
            {
                set;
                get;
            }

            public int ToY
            {
                set;
                get;
            }
        }
    }
}
