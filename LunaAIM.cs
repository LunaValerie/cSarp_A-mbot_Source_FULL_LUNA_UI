using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using swed32;
using SwedAim.EntityBase;

namespace SwedAim
{
    internal class Program
    {

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        #region mem

        // client
        const int localplayer = 0x0F41618;
        const int entitylist = 0xF38730;

        // engine
        const int clientstate = 0x555;
        const int viewangles = 0x555;

        // offsets
        const int health = 0x894;
        const int xyz = 0x555;
        const int team = 0x555;
        const int dormant = 0x555;


        #endregion


        static void Main(string[] args)
        {
            swed swed = new swed();
            swed.GetProcess("PointBlank");

            var client = swed.GetModuleBase("PointBlank.exe");

            var engine = swed.GetModuleBase("PointBlank.exe");

            entity player = new entity();
            List<entity> entities = new List<entity>();

            while(true)
            {
                if (GetAsyncKeyState(Keys.XButton2) < 0)
                {
                    updatelocal();
                    updateentities();

                    entities = entities.OrderBy(o => o.mag).ToList();
                    if (entities.Count > 0)
                        aim(entities[0]); 
                }
                Thread.Sleep(1);
            }

            float calcmag(entity e)
            {
                return (float)Math.Sqrt(Math.Pow(e.x - player.x, 2) + Math.Pow(e.y - player.y, 2) + Math.Pow(e.z - player.z, 2));
            }

            void updatelocal()
            {
                var buffer = swed.ReadPointer(client, localplayer);

                var coords = swed.ReadBytes(buffer, xyz, 12);

                player.x = BitConverter.ToSingle(coords, 0);
                player.y = BitConverter.ToSingle(coords, 4);
                player.z = BitConverter.ToSingle(coords, 8);

                player.team = BitConverter.ToInt32(swed.ReadBytes(buffer, team, 4), 0);

            }

            void updateentities()
            {
                entities.Clear();

                for (int i = 1; i < 32; i++) 
                {
                    var buffer = swed.ReadPointer(client, entitylist + i * 0x10);
                    var tm = BitConverter.ToInt32(swed.ReadBytes(buffer, team, 4), 0);

                    var dorm = BitConverter.ToInt32(swed.ReadBytes(buffer, dormant, 4), 0);

                    var hp = BitConverter.ToInt32(swed.ReadBytes(buffer, health, 4), 0);

                    if (hp < 2 || dorm != 0 || tm == player.team)
                        continue;

                    var coords = swed.ReadBytes(buffer, xyz, 12);

                    var ent = new entity();


                    ent.x = BitConverter.ToSingle(coords, 0);
                    ent.y = BitConverter.ToSingle(coords, 4);
                    ent.z = BitConverter.ToSingle(coords, 8);
                    ent.team = tm;
                    ent.health = hp;

                    ent.mag = calcmag(ent);
                    entities.Add(ent);
                }
            }

            void aim(entity ent)
            {
                float deltaX = ent.x - player.x;
                float deltaY = ent.y - player.y;

                float X = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

                float deltaZ = ent.z - player.z;

                double dist = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

                float Y = -(float)(Math.Atan2(deltaZ, dist) * 180 / Math.PI);

                var buffer = swed.ReadPointer(engine, clientstate);
                swed.WriteBytes(buffer, viewangles, BitConverter.GetBytes(Y));
                swed.WriteBytes(buffer, viewangles + 0x4, BitConverter.GetBytes(X));
            }
        }
    }
}





//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using swed32;

//namespace SwedAim
//{
//    internal class Program
//    {
//        static void Main(string[] args)
//        {
//            swed swed = new swed();
//            swed.GetProcess("PointBlank");

//            var client = swed.GetModuleBase("PointBlank.exe");

//            var engine = swed.GetModuleBase("PointBlank.exe");

//            int dwLocalPlayer = 0x00F41618;
//            int m_iHealth = 0x36C;

//            var buffer = swed.ReadPointer(client, dwLocalPlayer);

//            while (true)
//            {
//                var hp = BitConverter.ToInt32(swed.ReadBytes(buffer, m_iHealth, 4), 0);

//                Console.WriteLine("Player Health ==> " + hp);
//            }
//        }
//    }
//}
