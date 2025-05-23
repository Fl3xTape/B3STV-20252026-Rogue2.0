using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using STVrogue.GameLogic;

namespace STVrogue;

public class TestAgent
{
    
    public TestAgent() {
        // Constructor for the TestAgent class.
        // You can initialize any necessary variables or data structures here.
    }
    
    /// <summary>
    /// This method is called to determine the next action for the agent. It takes the console output
    /// from the game and the current game state as input.
    /// </summary>
    public virtual char NextAction(List<string> consoleOutput, Game gameState) {
        // This is the completely random text agent. only excluding the quit command.
        int rand = new Random().Next(0, 6);
        
        switch (rand)
        {
            case 0:
                return ' ';
            case 1:
                return 'm';
            case 2:
                return 'p';
            case 3:
                return 'a';
            case 4:
                return 'f';
            case 5:
                return 'u';
            //case 6: lets not make the ai randomly quit
                //return 'q';
        }
        
        throw new NotImplementedException("Not supposed to reach here");
    }
}