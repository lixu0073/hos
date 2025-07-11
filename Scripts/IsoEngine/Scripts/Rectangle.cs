using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoEngine
{
	[Serializable]
	public struct Rectangle
	{
		public int x, y, xSize, ySize;
		
		public bool Contains(Vector2i tile)
		{
			return tile.x >= x && tile.x < x + xSize && tile.y >= y && tile.y < y + ySize;
		}

        public bool Contains(Rectangle rect)
        {
           for (int i = 0; i<rect.xSize; i++)
           {
                for (int j = 0; j<rect.ySize; j++)
                {
                    if (Contains(rect.x + i, rect.y + j))
                        return true; 
                }
            }
           return false;
        }

        public bool Contains(int X, int Y)
		{
			return X >= x && Y >= y && X < x + xSize && Y < y + ySize;
		}
		public Rectangle(int X, int Y, int XSize, int YSize)
		{
			x = X;
			y = Y;
			xSize = XSize;
			ySize = YSize;
		}
		public Rectangle(Vector2i position, Vector2i size)
		{
			x = position.x;
			y = position.y;
			xSize = size.x;
			ySize = size.y;
		}
	}
}
