public enum PlayerMovementMode
{
    Free,
    DisableLeft,
    VerticalOnly
}

public static class PlayerEnterState
{
    public static PlayerMovementMode movementMode = PlayerMovementMode.Free;
}