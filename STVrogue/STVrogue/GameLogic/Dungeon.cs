using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using STVrogue.Utils;

namespace STVrogue.GameLogic
{
    
    /// <summary>
    /// Representing a dungeon. A dungeon consists of rooms, connected to from a graph.
    /// It has one unique starting room and one unique exit room. All rooms should be
    /// reachable from the starting room.
    /// </summary>
    public class Dungeon
    {

        #region Fields and Properties
        
        /// <summary>
        /// All rooms in the dungeon, including the start and exit rooms.
        /// </summary>
        public List<Room> Rooms { get; } = new List<Room>();
        public Room StartRoom { get; set; }
        public Room ExitRoom { get; set; }

        /// <summary>
        /// Return all creatures in the Dungeon. The player is excluded.
        /// </summary>
        public List<Creature> Creatures { get; } = new List<Creature>();

        /// <summary>
        /// Return all items in this Dungeon. The items in the player's bag
        /// are excluded.
        /// </summary>
        /// 
        public List<Item> Items { get; } = new List<Item>();
        
        IRandomGenerator randomGenerator;

        #endregion
        
        protected Dungeon() { }
        
        /// <summary>
        /// Create a dungeon with the indicated number of rooms and the indicated shape.
        /// A dungeon shape can be "linear" (list-shaped), "tree", or "random".
        /// <list type="bullet">
        /// <item>
        /// A dungeon should have a unique start-room and a unique exit-room. </item>
        /// <item>
        /// All rooms in the dungeon must be reachable from the start-room. </item>
        /// <item>
        /// Each room is set to have a random capacity between 1 and the given maximum-capacity.
        /// Start and exit-rooms should have capacity 0. </item>
        /// </list>
        ///
        /// The constructor also expects a random generator to be passed to it. For testing,
        /// use a deterministic random generator. 
        /// </summary>
        public Dungeon(IRandomGenerator rnd, DungeonShapeType shape, int numberOfRooms, int maximumRoomCapacity) : base()
        {   
            randomGenerator = rnd;
            switch (shape)
            {
                case DungeonShapeType.LINEAR:
                    MkLinearDungeon(numberOfRooms, maximumRoomCapacity);
                    break;
                case DungeonShapeType.TREE:
                    MkTreeDungeon(numberOfRooms, maximumRoomCapacity);
                    break;
                case DungeonShapeType.GRID:
                    MkGridDungeon(numberOfRooms, maximumRoomCapacity);
                    break;
            }
        }
        
        /// <summary>
        /// This creates a linear-shaped dungeon with the given number of rooms.
        /// </summary>
        private void MkLinearDungeon(int numberOfRooms, int maximumRoomCapacity)
        {
            if (numberOfRooms < 3)
                throw new ArgumentException("A linear dungeon should have at least three rooms.");
            
            if (maximumRoomCapacity < 1)
                throw new ArgumentException("A dungeon should have at least one capacity.");
            
            Room prev = null;
            Room r = null;
            for (int k = 0 ; k<numberOfRooms; k++)
            {
                int capacity = randomGenerator.NextInt(maximumRoomCapacity) + 1 ; // kutu note
                if (k == 0)
                {
                    capacity = 0;
                    r = new Room("R" + k, RoomType.STARTroom, capacity); 
                    StartRoom = r;
                    prev = r;
                }
                else if (k == numberOfRooms - 1)
                {
                    capacity = 0;
                    r = new Room("R" + k, RoomType.EXITroom, capacity);
                    ExitRoom = r;
                    prev.Connect(r, Direction.EAST);
                    prev = r;
                }
                else if (k == numberOfRooms - 2)
                {
                    capacity = maximumRoomCapacity;
                    r = new Room("R" + k, RoomType.ORDINARYroom, capacity);
                    prev.Connect(r, Direction.EAST);
                    prev = r;
                }
                else
                {
                    r = new Room("R" + k, RoomType.ORDINARYroom, capacity);
                    prev.Connect(r, Direction.EAST);
                    prev = r;
                }
                Rooms.Add(r);
            }
        }
        
        /// <summary>
        /// This creates a tree-shaped dungeon with the given number of rooms.
        /// </summary>
        private void MkTreeDungeon(int numberOfRooms, int maximumRoomCapacity)
        {   
            if (numberOfRooms < 5)
                throw new ArgumentException("A tree-shaped dungeon should have at least five rooms.");
            
            if (maximumRoomCapacity < 1)
                throw new ArgumentException("A dungeon should have at least one capacity.");
            
            // we will create the start-room, then we will breadth-firstly expand it to a tree.
            
            // list to be used as fifo-queue for the breadth-first expansion:
            List<Room> toBeExpanded = new List<Room>();
            StartRoom = new Room("R" + 0, RoomType.STARTroom, 0);
            Rooms.Add(StartRoom);
            toBeExpanded.Add(StartRoom);
            while (Rooms.Count < numberOfRooms)
            {
                Room R = toBeExpanded[0];
                toBeExpanded.RemoveAt(0);
                // make the children
                int branchingDegree = 3;
                int numOfChildrenToAdd = Math.Min(branchingDegree, numberOfRooms - Rooms.Count);
                for (int k = 0; k < numOfChildrenToAdd; k++)
                {
                    int capacity = 0;
                    if (Rooms.Count + 1 < numberOfRooms)
                    {
                        capacity = randomGenerator.NextInt(maximumRoomCapacity) + 1 ; // kutu note
                    }
          
                    Room childRoom = new Room("R" + Rooms.Count, RoomType.ORDINARYroom, capacity);
                    Direction dir = Direction.NORTH;
                    switch (k % branchingDegree)
                    {
                        case 1: dir = Direction.EAST; break;  
                        case 2: dir = Direction.SOUTH; break;
                    }
                    
                    R.Connect(childRoom, dir);
                    Rooms.Add(childRoom);
                    toBeExpanded.Add(childRoom);
                }
            }
            // now we have a tree with N rooms. We need to make the last room the exit-room.
            ExitRoom = Rooms[Rooms.Count - 1];
            // the neighbours of this exit room should be recreated with maximum capacity:
            foreach ((Room,Direction) n in ExitRoom.Neighbors.ToList())
            {
                Room room = n.Item1;
                Room newRoom = new Room(room.Id, room.RoomType, maximumRoomCapacity);
                // disconnect room from neighbours and connect new one
                foreach ((Room,Direction) m in room.Neighbors.ToList())
                {
                    Room neighborRoom = m.Item1;
                    Direction neighborDir = m.Item2;
                    neighborRoom.Disconnect(room);
                    neighborRoom.Connect(newRoom, Opposite(neighborDir));
                }
                Rooms.Remove(room);
                Rooms.Add(newRoom);
            }
        }
        
        /// <summary>
        /// This creates a dungeon in the shape of a grid with rooms connected to
        /// the neighbors on left,right,up, and down. 
        /// </summary>
        void MkGridDungeon(int numberOfRooms, int maximumRoomCapacity)
        {
            if (numberOfRooms < 4)
                throw new ArgumentException("A grid-dungeon should have at least five rooms.");
            
            if (maximumRoomCapacity < 1)
                throw new ArgumentException("A dungeon should have at least one capacity.");
            
            int numOfColumn = (int) Math.Sqrt((double)numberOfRooms) ;
            int numOfRow = numberOfRooms / numOfColumn ;
            if (numberOfRooms % numOfColumn != 0)
            {
                numOfRow++;
            }
            Room[ , ] created = new Room[numOfColumn, numOfRow] ;
            
            // calculate the exit room coords to identify neighbours:
            int exitK = (numberOfRooms - 1) / numOfRow;
            int exitJ = (numberOfRooms - 1) % numOfRow;
            for (int k = 0; k < numOfColumn; k++)
            {
                for (int j = 0; j < numOfRow && Rooms.Count < numberOfRooms; j++)
                {
                    bool isNeighborOfExit =
                        (k == exitK - 1 && j == exitJ) || // west
                        (k == exitK && j == exitJ - 1);  // north

                    int capacity;
                    if ((k == 0 && j == 0) || (Rooms.Count + 1 == numberOfRooms)) 
                        capacity = 0;
                    else if (isNeighborOfExit)
                        capacity = maximumRoomCapacity; 
                    else
                        capacity = randomGenerator.NextInt(maximumRoomCapacity) + 1 ; // kutu note
                    
                    created[k,j] = new Room("R" + (k*numOfRow+j), RoomType.ORDINARYroom, capacity);
                    Rooms.Add(created[k,j]);
                    // make the last added room to be the exit room:
                    ExitRoom = created[k,j];
                }
            }
            // make the (0,0) to be the start-room:
            StartRoom = created[0, 0];
            // connect the rooms to form a 2D grid:
            for (int k = 0; k < numOfColumn; k++)
            {
                for (int j = 0; j < numOfRow; j++)
                {
                    if (created[k,j] == null)
                        continue;
                    if (k < numOfColumn - 1 && created[k+1,j] != null)
                    {
                        created[k,j].Connect(created[k+1,j], Direction.EAST);
                    }
                    if (j < numOfRow - 1 && created[k,j+1] != null)
                    {
                        created[k,j].Connect(created[k,j+1], Direction.SOUTH);
                    }
                }
            }
        }
        
        /// <summary>
        /// THIS IS A DUMMY IMPLEMENTAION. you are expected to implement
        /// this method according to the description in the Project Document.
        /// 
        /// <para></para>
        /// Populate the dungeon in this Game with the specified number of monsters and items.
        ///
        /// <para></para>
        /// The monsters and items are dropped in random locations. Keep in mind that
        /// the number of monsters in a room should not exceed the room's capacity.
        /// There are also other constraints; see the Project Document.
        /// <para></para>
        /// Note that it is not always possible to populate the dungeon according to
        /// the specified parameters. E.g. in a dungeon with N rooms whose capacity
        /// are between 0 and k, it is definitely not possible to populate it with
        /// more than (N-2)*k monsters.
        /// <para></para>
        /// The method returns true if it manages to populate the dungeon as specified,
        /// else it returns false.
        /// </summary>
        public bool SeedMonstersAndItems(IRandomGenerator rnd, int numberOfMonster, int numberOfHealingPotion, int numberOfRagePotion)
        {
            if (numberOfHealingPotion < 0 || numberOfRagePotion < 0 || numberOfMonster < 0)
                return false;
            // Check if the monsters can even fit
            if(numberOfMonster > Rooms.Sum(r => r.Capacity))
                return false;
            // Total number of healing potions can NOT exceed half the  number of rooms 
            if (numberOfHealingPotion + numberOfRagePotion > Rooms.Count/2)
                return false;
            
            // Add monsters to rooms, starting with the rooms next to the exitroom
            int numMonstersLeft = numberOfMonster;
            while (numMonstersLeft > 0)
            {
                // Add 1 monster to each neighbour of the exitroom, so long as we dont go negative.
                foreach (Room exitNeighbour in ExitRoom.ReachableRooms())
                {
                    if (numMonstersLeft > 0)
                    {
                        AddMonsterToRoom(exitNeighbour);
                        numMonstersLeft--;
                    }
                }
                // for every non-exitneighbour room with leftover capacity.. Add 1 while supplies last
                foreach (Room other in 
                         Rooms.Where(r => r.Capacity > r.Creatures.Count(c => c is Monster) 
                                          && !r.ReachableRooms().Contains(ExitRoom)))
                {
                    if (numMonstersLeft > 0)
                    {
                        AddMonsterToRoom(other);
                        numMonstersLeft--;
                    }
                }
            }
            
            // Add potions to dungeon
            int roomsWithItems = 0;
            int maxRoomsWithItems = Rooms.Count / 2;
            List<Room> visited;
            
            bool potionSuccess = false;
            int attemptsLeft = 300;   // arbitrary
            while (!potionSuccess) // Outer loop,  retry until success or attempts run out
            {
                visited = new List<Room>(); // we empty this list when we retry.
                int healingPotionsLeft = numberOfHealingPotion;
                int ragePotionsLeft = numberOfRagePotion;
                
                // Actual potion-distribution loop
                while ((healingPotionsLeft > 0 || ragePotionsLeft > 0) && visited.Count < Rooms.Count)
                {
                    List<Room> candidates = Rooms.Except(visited).ToList();
                    Room target = candidates[rnd.NextInt(candidates.Count)]; // randomly choose from candidates
                    
                    // If none of the target-room's neighbours has a healing potions, we add a healing potion here.
                    if (healingPotionsLeft > 0 && !target.ReachableRooms().Any(n => n.Items.OfType<HealingPotion>().Any()))
                    {
                        if (target.Items.Count == 0)        // if there were not yet any items here, we count up.
                            roomsWithItems++;
                        AddHealingPotion(target);           // only after that, do we add our new item.
                        healingPotionsLeft--;               // we make sure to increment how many potions are left
                    }
                    // if the target-room is a non-exitroom (or start) leaf, we add a rage potion.
                    if (ragePotionsLeft > 0 && target.ReachableRooms().Count == 1 && target.RoomType == RoomType.ORDINARYroom)
                    {
                        if (target.Items.Count == 0)        // if there were not yet any items here, we count up.
                            roomsWithItems++;
                        AddRagePotion(target);              // only after that, do we add our new item.
                        ragePotionsLeft--;                  // we make sure to increment how many potions are left
                    }
                    
                    visited.Add(target);                // We add the target-room to our list of visited rooms.
                }
                
                // If we have no potions of any kind left to distribute, and we have not exceeded maxRoomsWithItems
                if (healingPotionsLeft + ragePotionsLeft == 0 && roomsWithItems <= maxRoomsWithItems)
                {
                    potionSuccess = true;
                    Console.WriteLine("Succesful distribution found.");
                }
                // otherwise, we have failed, and we will try again.. until our tries are up.
                else
                {
                    foreach (Room r in visited)
                    {
                        // Remove the items from the rooms again
                        r.Items.RemoveAll(i => i is HealingPotion || i is RagePotion);
                    }
                    attemptsLeft--;
                    // If we have exceeded our attempts without finding a valid distribution, our seed must be invalid.

                    if (attemptsLeft <= 0)
                    {
                        Console.WriteLine("No more attempts. failed");
                        return false; 
                    }
                }
            }
            
            return true;
        }
        
        void AddMonsterToRoom(Room r)
        {
            // id equal index. if list is empty, id=0. If list has 10 other elements, ids start at 0, so id=10
            Monster newMonster = new Monster("M" + Creatures.Count, "Monster");
            // this is automatically accounted for in  the Creatures-list of dungeon, when you call it.
            r.Creatures.Add(newMonster);
            Creatures.Add(newMonster);
            newMonster.Location = r;
            //Console.WriteLine("Monster added to " + r.Id);
        }

        void AddHealingPotion(Room r)
        {
            // id equal index. if list is empty, id=0. If list has 10 other elements, ids start at 0, so id=10
            HealingPotion newPotion = new HealingPotion("I" + Items.Count, 10);
            // this is automatically accounted for in the Item-list of dungeon, when you call it.
            r.Items.Add(newPotion);
        }
        
        void AddRagePotion(Room r)
        {
            // id equal index. if list is empty, id=0. If list has 10 other elements, ids start at 0, so id=10
            RagePotion newPotion = new RagePotion("I" + Items.Count);
            // this is automatically accounted for in the Item-list of dungeon, when you call it.
            r.Items.Add(newPotion);
        }

        #region additional getters


        
        #endregion
        
        Direction Opposite(Direction dir)
        {
            switch (dir)
            {
                case Direction.EAST:
                    return Direction.WEST;
                case Direction.WEST:
                    return Direction.EAST;
                case Direction.NORTH:
                    return Direction.SOUTH;
                case Direction.SOUTH:
                    return Direction.NORTH;
            }
            throw new Exception("Invalid direction");
        }
    }

    [Serializable()]
    public enum DungeonShapeType
    {
        LINEAR, 
        TREE,
        GRID
    }
    
    /// <summary>
    /// Representing different types of rooms.
    /// </summary>
    public enum RoomType
    {
        STARTroom,  // the starting room of the player. 
        EXITroom,   // representing the player's final destination.
        ORDINARYroom  // the type of the rest of the rooms. 
    }

    public enum Direction
    {
        NORTH, EAST, SOUTH, WEST
    }
    
    

    /// <summary>
    /// Representing a room in a dungeon.
    /// </summary>
    public class Room : GameEntity
    {

        #region fields and properties

        /// <summary>
        /// The type of this node: either start-node, exit-node, or common-node.
        /// </summary>
        public RoomType RoomType { get; }

        /// <summary>
        /// The number of monsters in this room cannot exceed this capacity.
        /// </summary>
        public int Capacity { get;  }
        
        /// <summary>
        /// Neighbors are nodes that are considered connected to this node.
        /// The connection is bidirectional in the sense that in can be traverse
        /// in both directions (from this room to a neighbor, and the otherway around).
        /// So, if u is in this.neighbors of this room, you have to make sure that
        /// this room is also in u.neighbors.
        /// <para></para>
        /// A connection also has a "direction", e.g. to indicate that a neighbor
        /// room u is to the north or east of this room. There are four directions
        /// possible: north, east, south, and west.
        /// </summary>
        public List<(Room,Direction)> Neighbors { get; } = new List<(Room,Direction)>();

        /// <summary>
        /// All creatures, excluding the players, which are currently in this room.
        /// </summary>
        public List<Creature> Creatures { get;  } = new List<Creature>();

        /// <summary>
        /// All items, excluding those in the player's bag, which are currently in this room.
        /// </summary>
        public List<Item> Items { get; } = new List<Item>();
        
        #endregion
        
        
        public Room(string uniqueId, RoomType roomTy, int capacity) : base(uniqueId)
        {
            RoomType = roomTy;
            Capacity = capacity;
        }

        #region additional getters
       
        /// <summary>
        /// The number of monsters in this room. The player does not count as a monster.
        /// </summary>
        public int NumberOfMonsters => Creatures.Count(c => c is Monster);

        #endregion

        /// <summary>
        /// To add the given room as a neighbor of this room.
        /// </summary>
        public void Connect(Room r, Direction direction)
        {
            Direction opposite = Direction.NORTH;
            switch (direction)
            {
                case Direction.NORTH:
                    opposite = Direction.SOUTH;
                    break;
                case Direction.EAST:
                    opposite = Direction.WEST;
                    break;
                case Direction.SOUTH:
                    opposite = Direction.NORTH;
                    break;
                case Direction.WEST:
                    opposite = Direction.EAST;
                    break;
            }
            Neighbors.Add((r,direction)); 
            r.Neighbors.Add((this,opposite));
        }

        /// <summary>
        /// To disconnect the given room. That is, the room r will no longer be a
        /// neighbor of this room.
        /// </summary>
        public void Disconnect(Room r)
        {
            Neighbors.RemoveAll(n => n.Item1 == r);
            r.Neighbors.RemoveAll(n => n.Item1 == this);
        }

        /// <summary>
        /// return the set of all rooms which are reachable from this room.
        /// </summary>
        public List<Room> ReachableRooms()
        {
            Room x = this;
            var seen = new List<Room>();
            var todo = new List<Room>();
            todo.Add(x);
            while (todo.Count > 0)
            {
                x = todo[0] ; todo.RemoveAt(0) ;
                seen.Add(x);
                foreach ((Room,Direction) y in x.Neighbors)
                {   Room r = y.Item1;
                    if (seen.Contains(r) 
                        || todo.Contains(r))
                        continue;
                    todo.Add(r);
                }
            }
            return seen;
        }

        /// <summary>
        /// Check if the given room is reachable from this room.
        /// </summary>
        public bool CanReach(Room r)
        {
            return ReachableRooms().Contains(r); // not the most efficient way of checking it btw
        }
    }



}
