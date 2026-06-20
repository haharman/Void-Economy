using Core;
using System;
using System.Threading;
using Model;
using R3;
using UnityEngine;
using View;

namespace Presenter
{
    public class AudioPresenter : IUpdatable
    {
        private readonly AudioView _view;
        private float _lastMoveTime;
        // 速度の移動平均(MovingAverage)
        private float _velocityMa;
        private bool _wasMusicSourcePlaying;
        // view.IsMusicSourcePlaying && フェードアウトしていない → 音楽再生中とする
        private bool _isMusicSourcePlaying => _view.IsMusicSourcePlaying() && !_view.IsMusicSourceFadeOut;
        private MusicId _musicId;

        // トラックの連続再生時間の限界値
        private float _playbackDurationLimit = 300;//
        // トラックの終了時、再生を止める閾値
        private float _playbackDurationThreshold = 120;//
        // トラックの連続停止時間の限界値
        private float _pauseDurationLimit = 6;//00
        private float  _lastMusicStartTime;
        private float _lastMusicStopTime;
        private bool _hadFocusLastFrame;
        // フォーカスロス前に再生中だったか
        private bool _wasPlayingBeforeFocusLoss;
        
        public AudioPresenter(AudioView view, SurfacePlayerModel playerModel, CancellationToken cancellationToken)
        {
            _view = view;
            _lastMusicStartTime = Time.time;
            _lastMusicStopTime = Time.time;
            _hadFocusLastFrame = Application.isFocused;

            playerModel.position
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(_ => _view.PlayFootstep(FootstepId.Default))
                .RegisterTo(cancellationToken);
        }
        
        public void OnUpdate(float deltaTime)
        {
            bool isFocused = Application.isFocused;
            bool focusLostThisFrame = _hadFocusLastFrame && !isFocused;
            bool focusRegainedThisFrame = !_hadFocusLastFrame && isFocused;
            _hadFocusLastFrame = isFocused;

            if (focusLostThisFrame)
            {
                _wasPlayingBeforeFocusLoss = _isMusicSourcePlaying;
                _view.PauseMusic();
            }
            else if (focusRegainedThisFrame && _wasPlayingBeforeFocusLoss)
            {
                _view.ResumeMusic();
            }

            // フォーカスが外れている間は曲管理ロジックを止める
            if (!isFocused)
            {
                return;
            }

            //Debug.Log("[AudioPresenter] OnUpdate]");
            var currentMusicDuration = Time.time - _lastMusicStartTime;
            // 再生が自然に終了したとき
            if (!_isMusicSourcePlaying && _wasMusicSourcePlaying)
            {
                if (Time.time - _lastMusicStartTime < _playbackDurationThreshold)
                {
                    // AudioDataSOがトラックリストを保持しているため、次の曲の指定を委譲する
                    //_musicId = (MusicId)(((int)_musicId + 1) % Enum.GetValues(typeof(MusicId)).Length);
                    _musicId = _view.PlayMusic(_musicId);
                    _lastMusicStartTime = Time.time;
                }
            }
            // 音楽の連続再生時間がLimitを超えたら、停止する
            if (_isMusicSourcePlaying && Time.time - _lastMusicStartTime > _playbackDurationLimit)
            {
                _view.StopMusic(5f);
                _lastMusicStopTime = Time.time;
            }
            // 音楽の連続停止時間がLimitを超えたら、再生する
            if (!_isMusicSourcePlaying && Time.time - _lastMusicStopTime > _pauseDurationLimit)
            {
                _musicId = _view.PlayMusic(_musicId, 0f);
                _lastMusicStartTime = Time.time;
            }
            // 実装予定: 立ち止まりを検知して再生する

        }
    }
}