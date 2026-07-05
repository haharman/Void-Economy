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
        ToSurface,
        Surface,
        ToSpace,
        Space
    }
    public enum InputState
    {
        Disable,
        Surface,
        Space,
        Dialogue,
        UI
    }
    
    public enum Surface
    {
        Space = -1,
        Mine = 0,
        Fog = 1,
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