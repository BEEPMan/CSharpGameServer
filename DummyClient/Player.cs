using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    public class Player
    {
        public float posX;
        public float posY;
        public float posZ;

        public float velX;
        public float velY;
        public float velZ;

        public Player()
        {
            posX = 0;
            posY = 0;
            posZ = 0;

            velX = 0;
            velY = 0;
            velZ = 0;
        }

        public Player(float posX, float posY, float posZ)
        {
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
        }

        public void SetPosition(float x, float y, float z)
        {
            posX = x;
            posY = y;
            posZ = z;
        }

        public void SetVelocity(float x, float y, float z)
        {
            velX = x;
            velY = y;
            velZ = z;
        }

        public void Move(float sec)
        {
            posX += velX * sec;
            posY += velY * sec;
            posZ += velZ * sec;
        }
    }
}
