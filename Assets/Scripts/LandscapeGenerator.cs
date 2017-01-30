using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Assets.Scripts
{
    class LandscapeGenerator
    {
        Room[,] roomMap = new Room[10, 10];
        
        public void Generate()
        {
            int x = 0;
            int y = 3;

            roomMap[x, y].roomHeight = 1;
            roomMap[x, y].roomWidth = 1;
            roomMap[x, y].roomLocation = 0;
            roomMap[x, y].rightDoor = true;
            roomMap[x+1, y].leftDoor = true;
            AddRoom(x+1, y);
        }

        public void AddRoom(int x, int y)
        {
        }
    }

    struct Room
    {
        public int roomHeight, roomWidth;
        public int roomLocation;

        public bool topDoor, bottomDoor, leftDoor, rightDoor;
    }
}

