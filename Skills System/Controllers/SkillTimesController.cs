using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTimesController
{
    private SkillData _data;
    private CharacterManager _character;

    // Tempo de recarga (cooldown)
    private float _cooldownTimer = 0f;

    // Animação / sincronização de hit
    private SkillAnimationController _animationController;
    private List<float> _pendingHitTimes;
    private SkillContext _currentContext;

    public SkillTimesController(SkillData data, CharacterManager character)
    {
        _data = data;
        _character = character;
    }

    // API de cooldown
    public bool IsOnCooldown => _cooldownTimer > 0f;
    public float CooldownRemaining => Mathf.Max(0f, _cooldownTimer);
    public float CooldownPercent => _data != null && _data.cooldownTime > 0f ? (Mathf.Clamp01(_cooldownTimer / _data.cooldownTime)) : 0f;

    public void StartCooldown()
    {
        _cooldownTimer = _data != null ? _data.cooldownTime : 0f;
    }

    public void TickCooldown(float deltaTime)
    {
        if (_cooldownTimer <= 0f) return;
        _cooldownTimer = Mathf.Max(0f, _cooldownTimer - deltaTime);
    }

    // Animação + sincronização de hit (eventos de acerto)
    public void StartAnimation(SkillContext context, Action<SkillContext> onHitCallback, Action onComplete = null, float explicitAttackSpeed = -1f)
    {
        if (_data == null || _character == null) { onComplete?.Invoke(); return; }

        Animator animator = _character.GetComponent<Animator>();
        if (animator == null || _data.animation == null)
        {
            onComplete?.Invoke();
            return;
        }

        float playbackSpeed = CalculatePlaybackSpeed(_data, _character, explicitAttackSpeed);

        _animationController = new SkillAnimationController();
        _currentContext = context;
        _pendingHitTimes = new List<float>();
        if (_data.hitEventNormalizedTimes != null && _data.hitEventNormalizedTimes.Count > 0)
        {
            _pendingHitTimes.AddRange(_data.hitEventNormalizedTimes);
            _pendingHitTimes.Sort();
        }

        // inscrever callback de progresso para sincronizar hits
        _animationController.OnProgressChanged += (normalized) => HandleAnimationProgress(normalized, onHitCallback, onComplete);

        _animationController.Play(animator, _data, playbackSpeed, onComplete);
    }

    public void TickAnimation(float deltaTime)
    {
        if (_animationController == null) return;
        _animationController.Tick(deltaTime);
        if (!_animationController.IsPlaying)
        {
            // dessinscrever e limpar estado quando a reprodução terminar
            _animationController.OnProgressChanged -= (normalized) => HandleAnimationProgress(normalized, null, null);
            _animationController = null;
            _pendingHitTimes = null;
            _currentContext = default;
        }
    }

    public void StopAnimation()
    {
        if (_animationController == null) return;
        // dessinscrever e parar animação explicitamente
        _animationController.OnProgressChanged -= (normalized) => HandleAnimationProgress(normalized, null, null);
        _animationController.Stop();
        _animationController = null;
        _pendingHitTimes = null;
        _currentContext = default;
    }

    private void HandleAnimationProgress(float normalized, Action<SkillContext> onHitCallback, Action onComplete)
    {
        if (_pendingHitTimes == null || _pendingHitTimes.Count == 0) return;

        while (_pendingHitTimes.Count > 0 && normalized >= _pendingHitTimes[0])
        {
            _pendingHitTimes.RemoveAt(0);
            if (_currentContext.Caster != null)
            {
                onHitCallback?.Invoke(_currentContext);
            }
        }

        if (normalized >= 1f)
        {
            // fim da animação: dessinscrever e limpar estado
            if (_animationController != null)
                _animationController.OnProgressChanged -= (n) => HandleAnimationProgress(n, onHitCallback, onComplete);
            _pendingHitTimes = null;
            _currentContext = default;
        }
    }

    private float CalculatePlaybackSpeed(SkillData data, CharacterManager character, float explicitAttackSpeed = -1f)
    {
        if (data == null || data.animation == null) return 1f;

        float clipLength = data.animation.length;

        float attackSpeed = explicitAttackSpeed > 0f ? explicitAttackSpeed : (character != null ? character.Data.TotalAttackSpeed : 1f);

        float desiredDuration = 0f;
        if (data.targetDurationOverride > 0f)
        {
            desiredDuration = data.targetDurationOverride;
        }
        else if (data.playbackMode == PlaybackMode.FitToCastTime && data.castTime > 0f)
        {
            desiredDuration = data.castTime;
        }
        else if (data.playbackMode == PlaybackMode.FitToAttackSpeed && data.useAttackSpeedForPlayback)
        {
            desiredDuration = 1f / Mathf.Max(0.0001f, attackSpeed);
        }
        else
        {
            desiredDuration = data.castTime > 0f ? data.castTime : clipLength;
        }

        float playbackSpeed = clipLength / Mathf.Max(0.0001f, desiredDuration);
        playbackSpeed *= Mathf.Max(0.0001f, data.playbackSpeedMultiplier);
        playbackSpeed = Mathf.Clamp(playbackSpeed, data.minPlaybackSpeed, data.maxPlaybackSpeed);

        return playbackSpeed;
    }
}
