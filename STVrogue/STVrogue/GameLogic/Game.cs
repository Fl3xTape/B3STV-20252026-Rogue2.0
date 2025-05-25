using System;
using System.Linq;
using STVrogue.Utils;

namespace STVrogue.GameLogic
{
    /// <summary>
    /// This class represents the whole game-state of STVRogue. It also contains
    /// the method <see cref="Update"/> that performs a single turn update to
    /// the game state.
    ///
    /// The game's main-loop is put separately in the class <see cref="GameRunner"/>.
    /// 
    /// <para></para>
    /// The main methods for this class are:
    ///
    /// <list type="bullet">
    ///
    /// <item> The constructor, for creating an instance of this Game, with a 
    /// dungeon according to a given configuration. </item>
    /// 
    /// <item> The method <see cref="Update"/> to do a single turn update. This is called
    /// from the mainloop in <see cref="GameRunner"/>. </item>
    /// 
    /// <item> The method <see cref="Flee"/> for you to program the logic of fleeing creatures.</item>
    ///
    /// </list>
    /// </summary>
    public class Game 
    {
        #region fields and properties
        
        public GameConfiguration Config { get; private set; }
        
        public Player Player { get ; private set; }
        
        public Dungeon Dungeon { get ; private set; }

        public bool Gameover { get; private set; } = false;

        /// <summary>
        /// To count the number of passed turns. 
        /// </summary>
        public int TurnNumber { get; private set; } 
        
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

        
        /// <summary>
        /// A <see cref="GameConsole"/> provides a text-based Console. You can print strings
        /// on this console, or read strings from it. Use this to handle your text I/O,
        /// e.g. to print msgs when a monster attacks or flees.
        /// NOTE: Don't read and write directly to the System's Console.
        /// </summary>
        public GameConsole GameConsole { get; set;  }

        /// <summary>
        /// A random generator you can use for making random decisions. The type
        /// is intentionally set to be an instance of <see cref="IRandomGenerator"/>
        /// to prevent you from directly using <see cref="Random"/>.
        /// When testing the game you need a setup where all your random generators
        /// behave deterministically, to avoid your testing to become flaky.
        /// The code below will use an instance of <see cref="RandomGenerator"/>,
        /// which is NOT deterministic.
        /// <para></para>
        /// Check out the other implementation of <see cref="IRandomGenerator"/>, namely
        /// <see cref="STVControlledRandom"/>, or else write your own implementation.
        /// </summary>
        IRandomGenerator rnd = new RandomGenerator();
        //IRandomGenerator rnd = new STVControlledRandom();
        
        #endregion

        /// <summary>
        /// Try to create an instance of Game satisfying the specified configuration.
        /// It should throw an exception if it does not manage to generate a dungeon
        /// satisfying the configuration.
        /// </summary>

        public Game()
        {
            
        }
        
        public Game(GameConfiguration conf) 
        {
            RandomGenerator.SetSeed(conf.RndSeed);
            rnd = RandomGenerator.Instance;

            if (conf.NumberOfRooms < 3) 
                throw new ArgumentOutOfRangeException("Number of rooms is too little");
            if (conf.NumberOfRooms < 4 && conf.DungeonShape == DungeonShapeType.GRID) 
                throw new ArgumentOutOfRangeException("Number of rooms is too little");
            if (conf.NumberOfRooms < 5 && conf.DungeonShape == DungeonShapeType.TREE) 
                throw new ArgumentOutOfRangeException("Number of rooms is too little");
            if (conf.MaxRoomCapacity <= 0)
                throw new ArgumentOutOfRangeException("Capacity has to be greater than 0");
            
            Config = conf;
            int k = 10;     // amount of retries we will do before failing the constructor.
            bool seedSuccess = false;
            

            //STVControlledRandom.SetSeed(conf.RndSeed);

            Player = new Player("0", "Bagginssess");
            
            Dungeon d = null;

            while (!seedSuccess && k > 0)
            {
                d = new Dungeon(this.rnd, 
                    conf.DungeonShape, 
                    conf.NumberOfRooms, 
                    conf.MaxRoomCapacity);


                seedSuccess = d.SeedMonstersAndItems(this.rnd,
                    conf.InitialNumberOfMonsters,
                    conf.InitialNumberOfHealingPots,
                    conf.InitialNumberOfRagePots);
                
                seedSuccess = d.SeedMonstersAndItems(this.rnd,
                    conf.InitialNumberOfMonsters,
                    conf.InitialNumberOfHealingPots,
                    conf.InitialNumberOfRagePots);

                Player.Hp = Player.HpMax;
                Player.Location = d.StartRoom;

                k--;
                // if seed was succesful, we now exit the while-loop. Otherwise, we keep retrying until k = 0.
            }
            
            if (seedSuccess)
            {
                Dungeon = d;
                
            }
            else
            {
                throw new ApplicationException("Seed unsuccesful");
            }
        }
        

        /// <summary>
        /// Cause a creature to flee a combat. This will take the creature to a neighboring
        /// room. This should not breach the capacity of that room. Note that fleeing a
        /// combat is not always possible --see the Project Document.
        /// The method returns true if fleeing was successful, else false.
        /// </summary>
        public bool Flee(Creature c)
        {
            if (!Player.Location.Creatures.Any())
            {
                // GameConsole.WriteLines("      You are not in combat.");
                return false;
            }
            else if (c.Flee(this))
            {
                // GameConsole.WriteLines("      We knew you are a coward.");
                return true;
            }
            else
            {
                // GameConsole.WriteLines("      Your flee failed.");    
                return false; 
            }
        }
        
        public bool Move(Creature c)
        {
            if (!Player.Location.Creatures.Any())
            {
                return true;
            }
            else if (c.GetType() != typeof(Monster) && c.Location != Player.Location)
            {
                return true;
            }
            else
            {  
                return false; 
            }
        }

        /// <summary>
        /// Perform a single turn-update on the game. In every turn, each creature
        /// is allowed to do one action. What the player does is specified in the argument
        /// of this method. A monster can either do nothing, move, attack, or flee.
        /// See the Project Document that defines when these are possible.
        /// The order in which creatures execute their actions is left for you to decide.
        /// </summary>
        public void Update(Command playerAction)
        {
            bool success = false;
            
            string args = playerAction.Args[0];

            GameConsole console = new GameConsole();

            switch (playerAction.Name)
            {
                case CommandType.MOVE:
                    string roomId = playerAction.Args[0];
                    Room roomToMoveTo = (from r in Dungeon.Rooms where r.Id == roomId select r).First();
                    if (Move(Player))
                    {
                        Player.Move(roomToMoveTo);
                        success = true;
                    }
                    break;

                case CommandType.ATTACK:
                    Creature monster = (from c in Player.Location.Creatures where c.Id == args select c).First();
                    Player.Attack(monster);
                    console.WriteLines(
                        $"You dealt {Player.AttackRating} damage. {monster.Name}: {monster.Hp}/{monster.HpMax}HP");
                    success = true;
                    break;

                case CommandType.PICKUP:
                    Item itemPick = (from i in Player.Location.Items where i.Id == args select i).First();
                    Player.Pickup(TurnNumber, itemPick);
                    success = true;
                    break;

                case CommandType.USE:
                    Item itemBag = (from i in Player.Bag where i.Id == args select i).First();
                    Player.Use(TurnNumber, itemBag);
                    Player.TurnsUntilFlee = 2;
                    success = true;
                    break;

                case CommandType.FLEE:
                    if (Flee(Player))
                    {
                        success = true;
                    }
                    break;

                case CommandType.DoNOTHING:
                    success = true;
                    break;
            }

            if (success)
            {
                UpdateCreatures();
                TurnNumber++;
                if (Player.TurnsUntilFlee > 0) Player.TurnsUntilFlee--;
            }
        }
        void UpdateCreatures()
        {
            foreach (Monster m in Creatures)
            {
                while (true)
                {
                    rnd = RandomGenerator.Instance;
                    rnd.NextInt(4);
                    break;
                }
                
                // Before going into the switch case. We must know whether the monster is in combat or not.
                
                switch (rnd.NextInt(4))
                {
                    case 1: // Do nothing
                        break;
                    
                    case 2: // Attack player
                        m.Attack(Player);
                        break;
                    
                    case 3: // Move
                        
                        break;
                    
                    case 4: // Flee
                        break;
                }
            }
        }
    }
}
