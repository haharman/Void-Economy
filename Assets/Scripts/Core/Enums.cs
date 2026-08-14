namespace Core
{
    public enum SystemType
    {
        PC,
        Mobile,
        Switch
    }

    public enum SceneState
    {
        ToTitle,
        Title,
        ToOrrery,
        Orrery,
    }
    public enum InputState
    {
        Disable,
        Player,
        Dialogue,
        UI
    }
    
    public enum CoordSystemId
    {
        Global = 0,
        Soul = 1,
        Mine = 2,
        Red = 3,
        Fog = 4
    }
    
    public enum EntityType
    {
        Object,
        Npc
    }
    
    public enum MusicId {
        Arctrus,
        Observatory,
        ARedPlanet,
        SpaceWalker,
        FogTrain,
        StoryAboutU,
        SolarSystem
    }

    public enum LoopId
    {
        
    }
    
    public enum FootstepId
    {
        Default
    }
    
    public enum OneShotId
    {
        
    }
}