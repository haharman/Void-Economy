using System;
using Core;

namespace Presenter
{
    public interface IInputService
    {
        event Action OnPlayerSubmitPressed;
        event Action OnPlayerCancelPressed;
        event Action OnUISubmitPressed;
        event Action OnUICancelPressed;
        void OnMenuClosed(InputState state);
        
    }
}