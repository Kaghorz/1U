public enum EnemyState
{
    Patrol,      // Wandering and avoiding walls
    Investigate, // Moving toward a noise source
    Chase,       // Following the player
    Attack,      // Standing still and attacking
    Death        // Spirit being exorcised
}
