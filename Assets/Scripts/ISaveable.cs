/// <summary>
/// Implemented by any scene object that wants to participate in the save/load system.
/// The full hierarchy path is used as the unique key.
/// </summary>
public interface ISaveable
{
    /// <summary>Called during save. Fill in the record and return it.</summary>
    StationSaveRecord CaptureState();

    /// <summary>Called during load. Restore state from the record.</summary>
    void RestoreState(StationSaveRecord record);
}
