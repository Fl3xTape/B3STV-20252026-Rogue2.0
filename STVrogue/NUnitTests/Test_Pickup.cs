using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using NUnit.Framework;
using STVrogue.GameLogic;
using STVrogue.Utils;

namespace NUnitTests
{
    [TestFixture]
    public class Test_Pickup
    {
        [Test]
        public void TestPlayerPickup()
        {   
            Item item1 = new HealingPotion("H0",2);
            Item item2 = new HealingPotion("H1",2);
            
            List<Item> items = new List<Item>();
            items.Add(item1);
            items.Add(item2);
            
            Room r = new Room("room", RoomType.ORDINARYroom, 5);
            r.Items.Add(item1);
            r.Items.Add(item2);
            
            Player p = new Player("P0", "Player0");
            p.Location = r;
            
            p.Pickup(0, item1);
            p.Pickup(0, item2);
            Assert.AreEqual(p.Bag, items);
            Assert.True(p.Location.Items.Count == 0);
        }
    }
}