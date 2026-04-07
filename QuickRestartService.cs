using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace FastRestart;

internal static class QuickRestartService
{
    internal static void RestartFromAutosave()
    {
        TaskHelper.RunSafely(RestartFromAutosaveAsync());
    }

    private static async Task RestartFromAutosaveAsync()
    {
        try
        {
            if (!SaveManager.Instance.HasRunSave)
            {
                MainFile.Logger.Error("FastRestart aborted because there is no current run autosave.");
                return;
            }

            ReadSaveResult<SerializableRun> saveResult = SaveManager.Instance.LoadRunSave();
            if (!saveResult.Success || saveResult.SaveData == null)
            {
                MainFile.Logger.Error($"FastRestart failed to load run autosave. Status={saveResult.Status}");
                return;
            }

            SerializableRun serializableRun = saveResult.SaveData;
            RunState runState = RunState.FromSerializable(serializableRun);

            RunManager.Instance.ActionQueueSet.Reset();
            NRunMusicController.Instance?.StopMusic();

            if (NGame.Instance?.Transition != null)
                await NGame.Instance.Transition.FadeOut();

            RunManager.Instance.CleanUp();

            RunManager.Instance.SetUpSavedSinglePlayer(runState, serializableRun);
            NGame.Instance?.ReactionContainer.InitializeNetworking(new NetSingleplayerGameService());

            if (NGame.Instance == null)
            {
                MainFile.Logger.Error("FastRestart could not continue because NGame.Instance was null.");
                return;
            }

            await NGame.Instance.LoadRun(runState, serializableRun.PreFinishedRoom);

            if (NGame.Instance.Transition != null)
                await NGame.Instance.Transition.FadeIn();

            MainFile.Logger.Info("FastRestart reloaded the current autosave successfully.");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"FastRestart failed while reloading autosave: {ex}");
        }
    }
}
