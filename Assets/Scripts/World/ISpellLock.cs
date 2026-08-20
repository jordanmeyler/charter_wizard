using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Every enemy — and much terrain — is a lock. The right assembled spell is a key.
    /// </summary>
    public interface ISpellLock
    {
        string DisplayName { get; }
        string FormulaId { get; }
        SpellId[] AcceptedKeys { get; }
        bool Resolved { get; }
        Vector3 WorldPosition { get; }

        string FormulaText();
        string Resolve(SpellId spell);
    }
}
