using UnityEngine;

/// <summary>
/// Модель для інформації про дію
/// </summary>

[System.Serializable]
public class ActionInfo
{
    public string ActionId;
    public string DisplayName;
    public string Description;
    public bool IsEnabled;
    public int Cost;
    public Sprite Icon;
}
