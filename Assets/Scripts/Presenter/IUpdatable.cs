namespace Presenter
{
    public interface IUpdatable
    {
        void OnUpdate(float deltaTime);
    }

    public interface IUpdatableService
    {
        public void Register(IUpdatable updateable);
        public void Unregister(IUpdatable updatable);
        public void ClearUpdatables();
    }
}