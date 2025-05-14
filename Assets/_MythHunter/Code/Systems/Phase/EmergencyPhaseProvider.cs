using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;
using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// Аварійний провайдер фаз для випадків, коли EventBus недоступний
/// </summary>
internal class EmergencyPhaseProvider : IPhaseProvider
{
    private readonly IMythLogger _logger;
    private string _currentPhaseId = "None";
    private readonly List<Action<string, string>> _callbacks = new List<Action<string, string>>();

    public EmergencyPhaseProvider(IMythLogger logger)
    {
        _logger = logger;
        _logger.LogWarning("Using EmergencyPhaseProvider - this is not intended for production use", "Phase");
    }

    public string GetCurrentPhaseId() => _currentPhaseId;

    public bool IsCurrentPhase(string phaseId) => _currentPhaseId == phaseId;

    public void SubscribeToPhaseChange(Action<string, string> onPhaseChanged)
    {
        if (!_callbacks.Contains(onPhaseChanged))
            _callbacks.Add(onPhaseChanged);
    }

    public void UnsubscribeFromPhaseChange(Action<string, string> onPhaseChanged)
    {
        _callbacks.Remove(onPhaseChanged);
    }

    public string[] GetAllPhaseIds()
    {
        // Повертаємо базові фази
        return new[] { "None", "Rune", "Planning", "Movement", "Combat", "Freeze" };
    }
}
