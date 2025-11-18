using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class SkillAnimationController
{
    private PlayableGraph _graph;
    private AnimationClipPlayable _clipPlayable;
    private Animator _animator;
    private float _duration;
    private float _elapsed;
    private bool _isPlaying;
    private Action _onComplete;

    public void Play(Animator animator, SkillData data, float attackSpeed = 1f, Action onComplete = null)
    {
        if (animator == null || data == null || data.animation == null)
        {
            onComplete?.Invoke();
            return;
        }

        Stop(); //limpeza

        _animator = animator;
        _onComplete = onComplete;

        // calcula o playbackSpeed que será passado ao AnimationClipPlayable.SetSpeed
        float playbackSpeed = Mathf.Max(0.0001f, TimeOfAnimation(data, attackSpeed));

        // cria PlayableGraph e conecta ao Animator
        _graph = PlayableGraph.Create($"SkillAnim_{Guid.NewGuid()}");
        var output = AnimationPlayableOutput.Create(_graph, "SkillAnimOutput", _animator);

        _clipPlayable = AnimationClipPlayable.Create(_graph, data.animation);
        _clipPlayable.SetApplyFootIK(false);
        _clipPlayable.SetApplyPlayableIK(false);
        _clipPlayable.SetSpeed(playbackSpeed);

        output.SetSourcePlayable(_clipPlayable);
        _graph.Play();

        // duração real da reprodução (segundos)
        _duration = Mathf.Abs(data.animation.length / Mathf.Max(0.0001f, playbackSpeed));
        _elapsed = 0f;
        _isPlaying = true;
    } 

    private float TimeOfAnimation(SkillData data, float attackSpeed)
    {
        if (data == null || data.animation == null) return 1f;

        attackSpeed = Mathf.Max(data.minSpeedVelocity, attackSpeed, 0.0001f);

        float playbackSpeed = data.animation.length * attackSpeed;

        // limites para evitar valores extremos
        float minPlayback = 0.01f;
        float maxPlayback = Mathf.Max(0.01f, data.animation.length * 100f);
        playbackSpeed = Mathf.Clamp(playbackSpeed, minPlayback, maxPlayback);

        return playbackSpeed;
    }

    public void Stop()
    {
        if (!_isPlaying) return;
        Finish();
    }

    private void Finish()
    {
        _isPlaying = false;
        try
        {
            if (_graph.IsValid())
                _graph.Destroy();
        }
        catch { /* evitar exceptions no teardown */ }

        _onComplete?.Invoke();
        _onComplete = null;
        _animator = null;
    }
   
    // Notifica progresso normalizado a cada Tick
    public event Action<float> OnProgressChanged;

    public void Tick(float deltaTime)
    {
        if (!_isPlaying) return;
        _elapsed += deltaTime;
        float normalized = NormalizedProgress;
        OnProgressChanged?.Invoke(normalized);
        if (_elapsed >= _duration)
            Finish();
    }
   
   
    public bool IsPlaying => _isPlaying;

    // Progresso normalizado da reprodução (0..1). Útil para disparar eventos baseados em tempo.
    public float NormalizedProgress
    {
        get
        {
            if (!_isPlaying || _duration <= 0f) return 0f;
            return Mathf.Clamp01(_elapsed / _duration);
        }
    }
    
}