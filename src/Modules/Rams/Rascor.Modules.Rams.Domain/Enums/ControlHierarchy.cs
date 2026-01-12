namespace Rascor.Modules.Rams.Domain.Enums;

/// <summary>
/// Hierarchy of controls (most effective to least effective)
/// </summary>
public enum ControlHierarchy
{
    Elimination = 1,      // Remove the hazard entirely
    Substitution = 2,     // Replace with less hazardous
    Engineering = 3,      // Isolate people from hazard
    Administrative = 4,   // Change the way people work
    PPE = 5              // Personal Protective Equipment (last resort)
}
