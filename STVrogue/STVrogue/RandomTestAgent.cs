using System;
using System.Collections.Generic;
using STVrogue.GameLogic;

namespace STVrogue;

public class RandomTestAgent : TestAgent
{
    private int _amountOfActions;
    
    public RandomTestAgent(int amountOfActions) {
        _amountOfActions = amountOfActions;
    }
    
    // completely random agent that performs a random action until it runs out of actions.
    public override char NextAction(List<string> consoleOutput, Game gameState) {
        if (_amountOfActions <= 0)
        {
            return 'q';
        }
        _amountOfActions--;
        
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
        }
        
        throw new NotImplementedException("Not supposed to reach here");
    }
}