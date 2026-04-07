using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Saves;

namespace FastRestart;

[HarmonyPatch(typeof(NPauseMenu), nameof(NPauseMenu._Ready))]
public static class PauseMenuReadyPatch
{
    private const string ButtonName = "FastRestartButton";
    private static readonly LocString ButtonLabelLoc = new("gameplay_ui", "FAST_RESTART.pause_menu.restart_room");

    [HarmonyPostfix]
    public static void Postfix(NPauseMenu __instance)
    {
        if (!TryGetButtonContainer(__instance, out Control? buttonContainerNullable))
            return;

        Control buttonContainer = buttonContainerNullable!;
        if (buttonContainer.GetNodeOrNull<NPauseMenuButton>(ButtonName) != null)
        {
            PauseMenuButtonState.Refresh(buttonContainer);
            return;
        }

        try
        {
            NPauseMenuButton? sourceButton = FindSourceButton(buttonContainer);
            NPauseMenuButton? giveUpButton = buttonContainer.GetNodeOrNull<NPauseMenuButton>("GiveUp");
            if (sourceButton == null || giveUpButton == null)
            {
                MainFile.Logger.Error("FastRestart could not find the expected pause menu buttons.");
                return;
            }

            NPauseMenuButton restartButton = (NPauseMenuButton)sourceButton.Duplicate();
            restartButton.Name = ButtonName;
            restartButton.GetNode<MegaLabel>("Label").SetTextAutoSize(GetButtonText());
            CloneButtonMaterial(restartButton);

            buttonContainer.AddChild(restartButton);
            buttonContainer.MoveChild(restartButton, giveUpButton.GetIndex());
            restartButton.Connect(
                NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ => PauseMenuButtonState.OnRestartPressed(buttonContainer))
            );

            PauseMenuButtonState.Refresh(buttonContainer);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"FastRestart failed to create its pause menu button: {ex}");
        }
    }

    private static bool TryGetButtonContainer(NPauseMenu pauseMenu, out Control? buttonContainer)
    {
        buttonContainer = pauseMenu.GetNodeOrNull<Control>("%ButtonContainer");
        if (buttonContainer != null)
            return true;

        MainFile.Logger.Error("FastRestart could not find %ButtonContainer on NPauseMenu.");
        return false;
    }

    private static NPauseMenuButton? FindSourceButton(Control buttonContainer)
    {
        return buttonContainer.GetNodeOrNull<NPauseMenuButton>("Settings")
               ?? buttonContainer.GetNodeOrNull<NPauseMenuButton>("SaveAndQuit");
    }

    private static string GetButtonText()
    {
        try
        {
            return ButtonLabelLoc.GetFormattedText();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"FastRestart failed to resolve localized label, falling back to English: {ex.Message}");
            return "快速重开";
        }
    }

    private static void CloneButtonMaterial(NPauseMenuButton restartButton)
    {
        TextureRect? buttonImage = restartButton.GetNodeOrNull<TextureRect>("ButtonImage");
        if (buttonImage?.Material is ShaderMaterial material)
            buttonImage.Material = (ShaderMaterial)material.Duplicate();
    }
}

[HarmonyPatch(typeof(NPauseMenu), "Initialize")]
public static class PauseMenuInitializePatch
{
    [HarmonyPostfix]
    public static void Postfix(NPauseMenu __instance)
    {
        Control? buttonContainer = __instance.GetNodeOrNull<Control>("%ButtonContainer");
        if (buttonContainer != null)
            PauseMenuButtonState.Refresh(buttonContainer);
    }
}

internal static class PauseMenuButtonState
{
    private const string ButtonName = "FastRestartButton";

    internal static void Refresh(Control buttonContainer)
    {
        NPauseMenuButton? restartButton = buttonContainer.GetNodeOrNull<NPauseMenuButton>(ButtonName);
        if (restartButton == null)
            return;

        if (CanUseQuickRestart())
            restartButton.Enable();
        else
            restartButton.Disable();

        RebuildFocusNeighbors(buttonContainer);
    }

    internal static void OnRestartPressed(Control buttonContainer)
    {
        if (!CanUseQuickRestart())
        {
            Refresh(buttonContainer);
            return;
        }

        DisableAllPauseButtons(buttonContainer);
        QuickRestartService.RestartFromAutosave();
    }

    private static bool CanUseQuickRestart()
    {
        return SaveManager.Instance.HasRunSave
               && MegaCrit.Sts2.Core.Runs.RunManager.Instance.NetService.Type == NetGameType.Singleplayer;
    }

    private static void DisableAllPauseButtons(Control buttonContainer)
    {
        foreach (Node child in buttonContainer.GetChildren())
        {
            if (child is NPauseMenuButton button)
                button.Disable();
        }
    }

    private static void RebuildFocusNeighbors(Control buttonContainer)
    {
        List<NPauseMenuButton> buttons = [];
        foreach (Node child in buttonContainer.GetChildren())
        {
            if (child is NPauseMenuButton { Visible: true } button)
                buttons.Add(button);
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            NPauseMenuButton button = buttons[i];
            NodePath self = button.GetPath();
            button.FocusNeighborLeft = self;
            button.FocusNeighborRight = self;
            button.FocusNeighborTop = i > 0 ? buttons[i - 1].GetPath() : self;
            button.FocusNeighborBottom = i < buttons.Count - 1 ? buttons[i + 1].GetPath() : self;
        }
    }
}
