using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface representing an event.
/// </summary>
public interface IEvent { }

/// <summary>
/// Static class for managing event bindings and raising events of type T.
/// </summary>
/// <typeparam name="T">The type of event, which must implement IEvent.</typeparam>
public static class EventBus<T> where T : IEvent 
{
    // A set of event bindings for the event type T.
    static readonly HashSet<IEventBinding<T>> bindings = new HashSet<IEventBinding<T>>();
    
    /// <summary>
    /// Registers an event binding.
    /// </summary>
    /// <param name="binding">The event binding to register.</param>
    public static void Register(EventBinding<T> binding) => bindings.Add(binding);

    /// <summary>
    /// Deregisters an event binding.
    /// </summary>
    /// <param name="binding">The event binding to deregister.</param>
    public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

    /// <summary>
    /// Raises an event, invoking all registered bindings.
    /// </summary>
    /// <param name="event">The event to raise.</param>
    public static void Raise(T @event) {
        // Create a snapshot of the current bindings to avoid modification during iteration.
        var snapshot = new HashSet<IEventBinding<T>>(bindings);

        // Invoke each binding's event handlers.
        foreach (var binding in snapshot) {
            if (bindings.Contains(binding)) {
                binding.OnEvent.Invoke(@event);
                binding.OnEventNoArgs.Invoke();
            }
        }
    }

    /// <summary>
    /// Clears all event bindings.
    /// </summary>
    static void Clear() {
        Debug.Log($"Clearing {typeof(T).Name} bindings");
        bindings.Clear();
    }
}