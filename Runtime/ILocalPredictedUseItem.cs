namespace RoachRace.Interaction
{
    /// <summary>
    /// Optional interface for items that want immediate owner-only local presentation
    /// when use input is pressed, before the server confirms the action.
    /// </summary>
    public interface ILocalPredictedUseItem
    {
        void BeginPredictedUse();
        void EndPredictedUse();
    }
}