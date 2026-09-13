using UnityEngine;
using DG.Tweening;
namespace Service
{
    public static class DOTweenResetter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => DOTween.KillAll();
    }
}