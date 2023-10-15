using DOTS.Dispatcher.Runtime;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour, IEventListener<PlayerHealthChanged>
{
    [SerializeField]
    private Slider m_PlayerHeath;
    [SerializeField]
    private TMPro.TextMeshProUGUI m_PlayerHeathText;

    private void OnEnable()
    {
        DispatcherSystem.Mono.Subscribe<PlayerHealthChanged>(this);
    }

    private void OnDisable()
    {
        DispatcherSystem.Mono.Unsubscribe<PlayerHealthChanged>(this);
    }

    public void OnEvent(Entity entity, in PlayerHealthChanged data)
    {
        m_PlayerHeath.maxValue = data.Value.maxHealth;
        m_PlayerHeath.minValue = 0;

        m_PlayerHeath.value = data.Value.currentHealth;

        m_PlayerHeathText.text = $"{data.Value.currentHealth}/{data.Value.maxHealth}";

        Debug.Log($"HealthBar Updated: {m_PlayerHeathText.text}");
    }

}
