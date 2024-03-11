using System;
    
[Flags]
public enum GameState
{
    Default = 0,
    Playing = 1,
    PowerUp = 2,
    Pause = 4,
    Lose = 8,
    Win = 16,
    PowerUpAnimation=32,
    Finished=64
}

