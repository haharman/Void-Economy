using System;
using Core;
using R3;
using UnityEngine;

namespace Presenter
{
    public interface IInputService
    {
        ReactiveProperty<InputState> CurrentInputState { get; set; }
        ReactiveProperty<Vector2> CurrentMoveInput { get; }
        event Action OnPlayerSubmitPressed;
        event Action OnPlayerCancelPressed;
        event Action OnUISubmitPressed;
        event Action OnUICancelPressed;
    }
}