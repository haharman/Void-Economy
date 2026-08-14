namespace Presenter
{
    public interface IUpdatable
    {
        void OnUpdate(float deltaTime);
    }

    public interface IUpdateService
    {
        public void Register(IUpdatable updateable);
        public void Unregister(IUpdatable updatable);
        public void ClearUpdatables();
    }
}