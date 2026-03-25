using Core;
using System;
using View;

namespace Presenter
{
    public class AudioPresenter
    {
        private readonly AudioView _view;
        
        public void PlayMusic(MusicId id)
        {
            _view.PlayMusic(id);
        }
        
        public void StopMusic(MusicId id)
        {
            _view.StopMusic();
        }
        
        public void PlayLoop(LoopId id)
        {
            _view.PlayLoop(id);
        }

        public void StopLoop(LoopId id)
        {
            _view.StopLoop(id);
        }
        
        public void StopAllLoops()
        {
            _view.StopAllLoops();
        }
        
        public void PlayFootstep(FootstepId id)
        {
            _view.PlayFootstep(id);
        }

        public void StopFootstep()
        {
            _view.StopFootstep();
        }

        public void PlayOneShot(OneShotId id)
        {
            _view.PlayOneShot(id);
        }
    }
}