using Dalamud.Logging;
using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.Gui.PartyFinder.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Newtonsoft.Json.Linq;

namespace KirboRotations.Ranged;

#pragma warning disable CS0612 // Type or member is obsolete
[SourceCode("https://github.com/BrakusTapus/KirboRotations/blob/main/Ranged/MCH_Kirbo_AIO.cs")]
[LinkDescription("https://i.imgur.com/23r8kFK.png", "Early AA")]
[LinkDescription("https://i.imgur.com/vekKW2k.jpg", "Delayed Tools")]
[LinkDescription("https://i.imgur.com/bkLg5WS.png", "123 Tools")]
[LinkDescription("https://i.imgur.com/qdCOQKy.png", "Fast Wildfire")]
#pragma warning restore CS0612 // Type or member is obsolete
[RotationDesc("Rotation for MCH", ActionID.Wildfire)]
sealed class MCH_Kirbo_AIO : MCH_Base
{
    #region Rotation Info

    public override string GameVersion => "6.45";

    public override string RotationName => "Kirbo's - All In One";

    public override string Description => "My work on MCH rotations found on the Balance.\n\nMeant and Tested for:\n\n  Level: 90 \n  GCD: 2.50s\n\n\n\n:)";

    /// <summary>
    ///A lot of reference material was and is being used while working on this rotation as i have no coding background whatsoever and im self learning how to do all this
    /// As such I'd like to kindly thank the following people:
    ///
    /// ArchiTed,   obviously for making RotationSolver but especially for making the rotation template!
    /// RiotNOR,    for the detailed explanation comments in their DRG rotation code!
    /// Bolt,       for the MCH rotation he made, the Queen timings were especially helpfull!
    /// </summary>
    #endregion

    #region Rotation Settings
    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
        .SetCombo("OpenerSelection", 5, "Opener Selection", "Early AA", "Delayed Tools", "123 Tools", "Fast Wildfire", "The FFlogs Opener", "No Opener");
    }
    #endregion Rotation Settings

    #region Variable's
    private bool SafeToUseHypercharge { get; set; }

    private int Openerstep = 0;
    private bool OpenerActionsAvailable { get; set; }
    private bool InOpener { get; set; }
    #endregion

    #region Countdown instructions
    protected override IAction CountDownAction(float remainTime)
    {
        if (Player.Level >= 90 && OpenerActionsAvailable)
        {
            switch (Configs.GetCombo("OpenerSelection"))
            {
                // Early AA [Done]
                case 0:
                    if (remainTime <= 0.5 + Ping && AirAnchor.CanUse(out _)) return AirAnchor;
                    if (remainTime <= 1.1 + 0.6 + Ping && UseBurstMedicine(out var act, false)) return act;
                    if (remainTime < 5 && Reassemble.CurrentCharges > 1) return Reassemble;
                    break;

                // Delayed Tools [Done]
                case 1:
                    if (remainTime <= 0.5 + Ping && SplitShot.CanUse(out _)) return SplitShot;
                    if (remainTime <= 1.1 + 0.6 + Ping && UseBurstMedicine(out var act1, false)) return act1;
                    break;

                // 123 Tools [Done]
                case 2:
                    if (remainTime <= 0.6 + Ping && SplitShot.CanUse(out _)) return SplitShot;
                    break;

                // Fast Wildfire [Done]
                case 3:
                    if (remainTime <= 0.5 + Ping && AirAnchor.CanUse(out _)) return AirAnchor;
                    if (remainTime < 5 && Reassemble.CurrentCharges > 1) return Reassemble;
                    break;

                // The FFlogs Opener [Done]
                case 4:
                    if (remainTime <= 0.5 + Ping && AirAnchor.CanUse(out _)) return AirAnchor;
                    if (remainTime <= 1.2f + Ping && UseBurstMedicine(out var act2, false)) return act2;
                    if (remainTime < 5 && Reassemble.CurrentCharges > 1) return Reassemble;
                    break;

                case 5:
                    if (remainTime <= 0.6 && SplitShot.CanUse(out var act3)) return act3;
                    break;

            }
        }


        if (Player.Level < 90)
        {
            if (AirAnchor.EnoughLevel && remainTime <= 0.6 + Ping && AirAnchor.CanUse(out _))
            { return AirAnchor; }

            if (!AirAnchor.EnoughLevel && Drill.EnoughLevel && remainTime <= 0.6 + Ping && Drill.CanUse(out _))
            { return Drill; }

            if (!AirAnchor.EnoughLevel && !Drill.EnoughLevel && HotShot.EnoughLevel && remainTime <= 0.6 + Ping && HotShot.CanUse(out _))
            { return HotShot; }

            if (!AirAnchor.EnoughLevel && !Drill.EnoughLevel && !HotShot.EnoughLevel && remainTime <= 0.6 + Ping && CleanShot.CanUse(out _))
            { return CleanShot; }

            if (remainTime < 5 && Reassemble.CurrentCharges > 1) return Reassemble;
        }

        return base.CountDownAction(remainTime);
    }

    #endregion

    #region Opener instructions
    /// <summary>
    /// The logic for the Opener goes
    /// 1. The 'OpenerInProgress' variable is set to True during the countdown
    /// 2. While the 'OpenerInProgress' is True the 'ProcessOpenerStep' will do the opener rotation step by step
    /// 3. Once 'OpenerInProgress' reaches the final step it will then set the 'ReachedEndOfOpener' variable to True
    /// 4. When the variable 'ReachedEndOfOpener' becomes True, the variable 'OpenerFinshed' is set to True, the variable 'OpenerInProgress' to False and the 'OpenerStep' value set back to 0.
    /// </summary>
    private bool Opener(out IAction act)
    {
        // Start of combat encounter
        var OverHeatStacks = Player.StatusStack(true, StatusID.Overheated);
        switch (Configs.GetCombo("OpenerSelection"))
        {
            // Early AA [Done]
            case 0:
                if (InOpener) // With the variable 'OpenerInProgress' set to True the opener rotation should be done in this order
                {
                    switch (Openerstep)
                    {
                        case 0: return OpenerStep(IsLastAction(false, AirAnchor), AirAnchor.CanUse(out act, CanUseOption.MustUse));
                        case 1: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 2: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 3: return OpenerStep(IsLastAction(false, Drill), Drill.CanUse(out act, CanUseOption.MustUse));
                        case 4: return OpenerStep(IsLastAction(false, BarrelStabilizer), BarrelStabilizer.CanUse(out act, CanUseOption.MustUse));
                        case 5: return OpenerStep(IsLastAction(true, SplitShot), SplitShot.CanUse(out act, CanUseOption.MustUse));
                        case 6: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 7: return OpenerStep(IsLastAction(true, SlugShot), SlugShot.CanUse(out act, CanUseOption.MustUse));
                        case 8: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 9: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 10: return OpenerStep(IsLastAction(true, CleanShot), CleanShot.CanUse(out act, CanUseOption.MustUse));
                        case 11: return OpenerStep(IsLastAction(false, Reassemble), Reassemble.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 12: return OpenerStep(IsLastAction(false, Wildfire), Wildfire.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));
                        case 13: return OpenerStep(IsLastAction(false, ChainSaw), ChainSaw.CanUse(out act, CanUseOption.MustUse));
                        case 14: return OpenerStep(IsLastAbility(true, RookAutoturret), RookAutoturret.CanUse(out act, CanUseOption.MustUse));
                        case 15: return OpenerStep(IsLastAbility(false, Hypercharge), Hypercharge.CanUse(out act, CanUseOption.IgnoreClippingCheck | CanUseOption.OnLastAbility | CanUseOption.MustUseEmpty));
                        case 16: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 4, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 17: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 18: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 3, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 19: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 20: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 2, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 21: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 22: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 1, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 23: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 24: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 0, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 25: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 26: return OpenerStep(IsLastGCD(false, Drill), Drill.CanUse(out act, CanUseOption.MustUse));
                        case 27:
                            InOpener = false; // The final step of the opener will set the variable 'ReachedEndOfOpener' to True indicating that it's done
                            break;
                    }
                }
                break;

            // Delayed Tools Opener [Done]
            case 1:
                if (InOpener) // With the variable 'OpenerInProgress' set to True the opener rotation should be done in this order
                {
                    switch (Openerstep)
                    {
                        case 0: return OpenerStep(IsLastAction(true, SplitShot), SplitShot.CanUse(out act, CanUseOption.MustUse));
                        case 1: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 2: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 3: return OpenerStep(IsLastAction(false, Drill), Drill.CanUse(out act, CanUseOption.MustUse));
                        case 4: return OpenerStep(IsLastAction(false, BarrelStabilizer), BarrelStabilizer.CanUse(out act, CanUseOption.MustUse));
                        case 5: return OpenerStep(IsLastAction(true, SlugShot), SlugShot.CanUse(out act, CanUseOption.MustUse));
                        case 6: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 7: return OpenerStep(IsLastAction(true, CleanShot), CleanShot.CanUse(out act, CanUseOption.MustUse));
                        case 8: return OpenerStep(IsLastAction(false, Reassemble), Reassemble.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 9: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 10: return OpenerStep(IsLastAction(false, AirAnchor), AirAnchor.CanUse(out act, CanUseOption.MustUse));
                        case 11: return OpenerStep(IsLastAction(false, Reassemble), Reassemble.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 12: return OpenerStep(IsLastAction(false, Wildfire), Wildfire.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));
                        case 13: return OpenerStep(IsLastAction(false, ChainSaw), ChainSaw.CanUse(out act, CanUseOption.MustUse));
                        case 14: return OpenerStep(IsLastAbility(true, RookAutoturret), RookAutoturret.CanUse(out act, CanUseOption.MustUse));
                        case 15: return OpenerStep(IsLastAbility(false, Hypercharge), Hypercharge.CanUse(out act, CanUseOption.IgnoreClippingCheck | CanUseOption.OnLastAbility | CanUseOption.MustUseEmpty));
                        case 16: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 4, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 17: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 18: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 3, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 19: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 20: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 2, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 21: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 22: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 1, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 23: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 24: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 0, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 25: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 26: return OpenerStep(IsLastGCD(false, Drill), Drill.CanUse(out act, CanUseOption.MustUse));
                        case 27:
                            InOpener = false; // The final step of the opener will set the variable 'ReachedEndOfOpener' to True indicating that it's done
                            break;
                    }
                }
                break;

            // 123 Tools [Semi - Done *Needs tincture in opener]
            case 2:
                if (InOpener) // With the variable 'OpenerInProgress' set to True the opener rotation should be done in this order
                {
                    switch (Openerstep)
                    {
                        case 0: return OpenerStep(IsLastAction(true, SplitShot), SplitShot.CanUse(out act, CanUseOption.MustUse));
                        case 1: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 2: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 3: return OpenerStep(IsLastAction(true, SlugShot), SlugShot.CanUse(out act, CanUseOption.MustUse));
                        case 4: return OpenerStep(IsLastAction(false, BarrelStabilizer), BarrelStabilizer.CanUse(out act, CanUseOption.MustUse));
                        case 5: return OpenerStep(IsLastAction(true, CleanShot), CleanShot.CanUse(out act, CanUseOption.MustUse));
                        case 6: return OpenerStep(IsLastAction(false, AirAnchor), AirAnchor.CanUse(out act, CanUseOption.MustUse));
                        case 7: return OpenerStep(IsLastAction(false, Reassemble), Reassemble.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 8: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 9: return OpenerStep(IsLastAction(false, Drill), Drill.CanUse(out act, CanUseOption.MustUse));
                        case 10: return OpenerStep(IsLastAction(false, Reassemble), Reassemble.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 11: return OpenerStep(IsLastAction(false, Wildfire), Wildfire.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));
                        case 12: return OpenerStep(IsLastAction(false, ChainSaw), ChainSaw.CanUse(out act, CanUseOption.MustUse));
                        case 13: return OpenerStep(IsLastAbility(true, RookAutoturret), RookAutoturret.CanUse(out act, CanUseOption.MustUse));
                        case 14: return OpenerStep(IsLastAbility(false, Hypercharge), Hypercharge.CanUse(out act, CanUseOption.IgnoreClippingCheck | CanUseOption.OnLastAbility | CanUseOption.MustUseEmpty));
                        case 15: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 4, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 16: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 17: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 3, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 18: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 19: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 2, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 20: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 21: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 1, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 22: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 23: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 0, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 24: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 25: return OpenerStep(IsLastAction(true, SplitShot), SplitShot.CanUse(out act, CanUseOption.MustUse));
                        case 26: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 27: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 28:
                            InOpener = false; // The final step of the opener will set the variable 'ReachedEndOfOpener' to True indicating that it's done
                            break;
                    }
                }
                break;

            // Fast Wildfire [Done]
            case 3:
                if (InOpener)
                {
                    switch (Openerstep)
                    {
                        case 0: return OpenerStep(IsLastAction(false, AirAnchor), AirAnchor.CanUse(out act, CanUseOption.MustUse));
                        case 1: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 2: return OpenerStep(IsLastAction(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 3: return OpenerStep(IsLastAction(false, ChainSaw), ChainSaw.CanUse(out act, CanUseOption.MustUse));
                        case 4: return OpenerStep(IsLastAction(false, BarrelStabilizer), BarrelStabilizer.CanUse(out act, CanUseOption.MustUse));
                        case 5: return OpenerStep(IsLastAction(false, Wildfire), Wildfire.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));
                        case 6: return OpenerStep(IsLastAction(true, Drill), Drill.CanUse(out act, CanUseOption.MustUse));
                        case 7: return OpenerStep(IsLastAction(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 8: return OpenerStep(IsLastAbility(false, Hypercharge), Hypercharge.CanUse(out act, CanUseOption.IgnoreClippingCheck | CanUseOption.OnLastAbility | CanUseOption.MustUseEmpty));
                        case 9: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 4, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 10: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 11: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 3, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 12: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 13: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 2, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 14: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 15: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 1, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 16: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 17: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 0, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 18: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 19:
                            InOpener = false; // The final step of the opener will set the variable 'ReachedEndOfOpener' to True indicating that it's done
                            break;
                    }
                }
                break;

            // The FFlogs Opener
            case 4:
                if (InOpener)
                {
                    switch (Openerstep)
                    {
                        case 0: return OpenerStep(IsLastGCD(false, AirAnchor), AirAnchor.CanUse(out act, CanUseOption.MustUse));
                        case 1: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 2: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty | CanUseOption.OnLastAbility));
                        case 3: return OpenerStep(IsLastGCD(true, Drill), Drill.CanUse(out act, CanUseOption.MustUse));
                        case 4: return OpenerStep(IsLastAbility(false, BarrelStabilizer), BarrelStabilizer.CanUse(out act, CanUseOption.MustUse));
                        case 5: return OpenerStep(IsLastAbility(false, Reassemble), Reassemble.CanUse(out act, CanUseOption.MustUseEmpty | CanUseOption.OnLastAbility));
                        case 6: return OpenerStep(IsLastGCD(false, ChainSaw), ChainSaw.CanUse(out act, CanUseOption.MustUse));
                        case 7: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 8: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty | CanUseOption.OnLastAbility));
                        case 9: return OpenerStep(IsLastGCD(true, SplitShot), SplitShot.CanUse(out act, CanUseOption.MustUse));
                        case 10: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 11: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty | CanUseOption.OnLastAbility));
                        case 12: return OpenerStep(IsLastGCD(true, SlugShot), SlugShot.CanUse(out act, CanUseOption.MustUse));
                        case 13: return OpenerStep(IsLastAbility(false, Wildfire), Wildfire.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));
                        case 14: return OpenerStep(IsLastGCD(true, CleanShot), CleanShot.CanUse(out act, CanUseOption.MustUse));
                        case 15: return OpenerStep(IsLastAbility(true, RookAutoturret), RookAutoturret.CanUse(out act, CanUseOption.MustUse));
                        case 16: return OpenerStep(IsLastAbility(false, Hypercharge), Hypercharge.CanUse(out act, CanUseOption.IgnoreClippingCheck | CanUseOption.OnLastAbility | CanUseOption.MustUseEmpty));
                        case 17: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 4, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 18: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 3, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 19: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 20: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 2, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 21: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 22: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 1, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 23: return OpenerStep(IsLastAbility(false, Ricochet), Ricochet.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 24: return OpenerStep(IsLastGCD(false, HeatBlast) && OverHeatStacks == 0, HeatBlast.CanUse(out act, CanUseOption.MustUse));
                        case 25: return OpenerStep(IsLastAbility(false, GaussRound), GaussRound.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 26:
                            InOpener = false; // The final step of the opener will set the variable 'ReachedEndOfOpener' to True indicating that it's done
                            break;
                    }
                }
                break;


        }

        act = null;
        return false;
    }
    private bool OpenerStep(bool condition, bool result)
    {
        if (condition)
        {
            Openerstep++;
        }
        else
        {
            return result;
        }
        return false;
    }
    #endregion

    #region Global Cooldown Actions (GCD's)
    protected override bool EmergencyGCD(out IAction act)
    {
        // Loops the opener until done
        if (InOpener) { return Opener(out act); }

        if (!InOpener) { return GeneralGCD(out act); }

        return base.EmergencyGCD(out act);
    }

    protected override bool GeneralGCD(out IAction act)
    {
        if (InOpener) { return Opener(out act); }

        if (!InOpener)
        {
            // For when overheated
            if (AutoCrossbow.CanUse(out act)) return true;
            if (HeatBlast.CanUse(out act)) return true;

            // these 2 share the same cooldown timer
            if (BioBlaster.CanUse(out act)) return true;
            if (Drill.CanUse(out act)) return true;

            // These 2 are getting the .MustUse so we can be sure of their useage
            if (AirAnchor.CanUse(out act, CanUseOption.MustUse)) return true;
            if (!AirAnchor.EnoughLevel && HotShot.CanUse(out act, CanUseOption.MustUse)) return true;
            if (ChainSaw.CanUse(out act, CanUseOption.MustUse)) return true;

            // Spread or scattergun
            if (SpreadShot.CanUse(out act)) return true;

            // Basic 1 - 2 -3
            if (CleanShot.CanUse(out act)) return true;
            if (SlugShot.CanUse(out act)) return true;
            if (SplitShot.CanUse(out act)) return true;
        }

        act = null;
        return false;
    }

    #endregion Global Cooldown Actions (GCD's)

    #region Off Global Cooldown Abilities (oGCD's)
    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {

        var Dungeon = TerritoryContentType is TerritoryContentType.Dungeons;
        if (Dungeon)
        {
            if (Wildfire.CanUse(out act) && !IsTargetDying) return true;
            if (BarrelStabilizer.CanUse(out act)) return true;
            if (Hypercharge.CanUse(out act)) return true;
            if (Reassemble.CanUse(out act, CanUseOption.MustUseEmpty) && (nextGCD == CleanShot || nextGCD == Drill || nextGCD == HotShot || nextGCD == AirAnchor)) return true;
            if (RookAutoturret.CanUse(out act) && ((!IsTargetBoss && CombatElapsedLess(30)) || IsTargetBoss && !IsTargetDying) && InCombat) return true;
        }


        // Loops the Opener method
        if (InOpener) { return Opener(out act); }

        // Handles the useage of oGCD's of each rotation
        if (!InOpener)
        {
            switch (Configs.GetCombo("OpenerSelection"))
            {
                /// Early AA
                case 0:
                    // 1. Wildfire
                    if (nextGCD == ChainSaw || IsLastGCD(ActionID.ChainSaw) || !Wildfire.IsCoolingDown)
                    {
                        if (Wildfire.CanUse(out act, CanUseOption.EmptyOrSkipCombo | CanUseOption.OnLastAbility)) return true;
                    }

                    // 2. BarrelStabilizer
                    if (BarrelStabilizer.CanUse(out act)) return true;

                    // 3. Reassemble
                    if (Reassemble.CurrentCharges > 0)
                    {
                        if (Reassemble.CurrentCharges == 1)
                        {
                            if (nextGCD == ChainSaw && Wildfire.ElapsedAfter(65))
                            {
                                if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return false;
                            }

                            if ((nextGCD == ChainSaw || nextGCD == Drill || nextGCD == AirAnchor) && !Wildfire.ElapsedAfter(65))
                            {
                                if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
                            }
                        }
                        if (Reassemble.CurrentCharges == 1 && Reassemble.WillHaveOneCharge(55))
                        {
                            if ((nextGCD == ChainSaw || nextGCD == Drill || nextGCD == AirAnchor))
                            {
                                if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
                            }
                        }
                    }

                    // 4. Queen
                    if (InCombat)
                    {
                        if (!Wildfire.ElapsedAfter(60))
                        {
                            if (RookAutoturret.CanUse(out act)) return true;
                        }

                        if (Wildfire.ElapsedAfter(60) || !Wildfire.IsInCooldown)
                        {
                            if (Battery >= 80 && (nextGCD == AirAnchor || nextGCD == ChainSaw) || Battery == 100 && (nextGCD == CleanShot || nextGCD == AirAnchor || nextGCD == ChainSaw))

                                if (RookAutoturret.CanUse(out act)) return true;
                        }
                    }

                    // 5. Hypercharge
                    if (SafeToUseHypercharge || !Drill.EnoughLevel)
                    {
                        if (!Wildfire.ElapsedAfter(80))
                        {
                            if (Hypercharge.CanUse(out act)) return true;
                        }

                        if (Wildfire.ElapsedAfter(80) || !Wildfire.IsInCooldown)
                        {
                            if (Heat == 100 && (nextGCD == SplitShot || nextGCD == SlugShot || nextGCD == CleanShot))

                                if (Hypercharge.CanUse(out act)) return true;
                        }
                    }

                    break;

                /// Delayed Tools
                case 1:
                    // 4. Queen
                    if (CombatElapsedLess(61) && !CombatElapsedLess(31))
                    { if (RookAutoturret.CanUse(out act)) return true; }

                    if (Wildfire.ElapsedAfter(115) && Battery >= 50 && nextGCD == AirAnchor)
                    { if (RookAutoturret.CanUse(out act)) return true; }

                    if (Battery >= 80 && !Wildfire.ElapsedAfter(70))
                    { if (RookAutoturret.CanUse(out act)) return true; }

                    // 3. Reassemble
                    if (nextGCD.IsTheSameTo(true, AirAnchor))
                    { if (Reassemble.CanUse(out act, CanUseOption.MustUse)) return true; }

                    if (nextGCD.IsTheSameTo(true, ChainSaw))
                    { if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true; }

                    // 1. Wildfire
                    if (nextGCD == ChainSaw && WeaponRemain > 0.65)
                    { if (Wildfire.CanUse(out act, CanUseOption.OnLastAbility)) return true; }

                    // 2. BarrelStabilizer
                    if (BarrelStabilizer.CanUse(out act, CanUseOption.MustUse))
                    { if (Wildfire.ElapsedAfter(100) || Player.HasStatus(true, StatusID.Wildfire)) return true; }

                    // 5. Hypercharge
                    if (Hypercharge.CanUse(out act) && (SafeToUseHypercharge || !Drill.EnoughLevel))
                    {
                        if (Player.HasStatus(true, StatusID.Wildfire)) return true;
                        if (!Wildfire.ElapsedAfter(90) && Wildfire.IsCoolingDown) return true;
                    }
                    break;

                /// 123 tools [Should not be used as of now]
                case 2:
                    // 1. Wildfire
                    if (Wildfire.CanUse(out act, CanUseOption.EmptyOrSkipCombo | CanUseOption.OnLastAbility)) { if (nextGCD == ChainSaw && Heat >= 50 || IsLastGCD(ActionID.ChainSaw) && Heat >= 50) return true; }

                    // 2. BarrelStabilizer
                    if (BarrelStabilizer.CanUse(out act)) return true;

                    // 3. Reassemble
                    if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo) && (AirAnchor.IsCoolingDown)) { if (nextGCD == ChainSaw) return true; }
                    if (!Wildfire.ElapsedAfter(70) && Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) { if (nextGCD == Drill || nextGCD == AirAnchor) return true; }
                    if (Wildfire.ElapsedAfter(110) && Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) { if (nextGCD == ChainSaw) return true; }

                    // 4. Queen
                    if (InCombat && RookAutoturret.EnoughLevel)
                    {
                        if (CombatElapsedLess(61) && !CombatElapsedLess(31))
                        { if (RookAutoturret.CanUse(out act)) return true; }

                        if (Wildfire.ElapsedAfter(110) && (Battery == 100 || Battery >= 70 && nextGCD == ChainSaw || Battery >= 70 && nextGCD == AirAnchor))
                        { if (RookAutoturret.CanUse(out act)) return true; }

                        if (Battery >= 80 && !Wildfire.ElapsedAfter(70))
                        { if (RookAutoturret.CanUse(out act)) return true; }
                    }

                    // 5. Hypercharge
                    if (Hypercharge.CanUse(out act) && (SafeToUseHypercharge || !Drill.EnoughLevel)) { if (!Wildfire.ElapsedAfter(90) && Wildfire.IsCoolingDown) return true; }

                    break;

                /// Fast Wildfire
                case 3:
                    // 1. Wildfire
                    if (Wildfire.CanUse(out act, CanUseOption.EmptyOrSkipCombo | CanUseOption.OnLastAbility)) { if (nextGCD == ChainSaw || IsLastGCD(ActionID.ChainSaw)) return true; }

                    // 2. BarrelStabilizer
                    if (BarrelStabilizer.CanUse(out act)) return true;

                    // 3. Reassemble
                    if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo) && (AirAnchor.IsCoolingDown)) { if (nextGCD == ChainSaw || nextGCD == AirAnchor) return true; }

                    // 4. Queen
                    if (InCombat && RookAutoturret.CanUse(out act))
                    {
                        // The second Queen at one minute
                        if (!CombatElapsedLess(31) && CombatElapsedLess(80) && Battery >= 60) { return true; }

                        // Even Batteries
                        if (Wildfire.ElapsedAfter(115) && Battery >= 50) { return true; }

                        // Odd Batteries
                        if (Battery >= 50 && !Wildfire.ElapsedAfter(80)) { return true; }
                    }

                    // 5. Hypercharge
                    if (Hypercharge.CanUse(out act) && (SafeToUseHypercharge || !Drill.EnoughLevel)) { if (!Wildfire.ElapsedAfter(90) && Wildfire.IsCoolingDown) return true; }

                    break;

                /// The FFlogs Opener
                case 4:
                    // 1. Wildfire
                    if (Wildfire.CanUse(out act, CanUseOption.EmptyOrSkipCombo | CanUseOption.OnLastAbility)) return true;

                    // 2. BarrelStabilizer      //if ((Wildfire.ElapsedAfter(105) || !Wildfire.IsCoolingDown || Wildfire.IsCoolingDown) && BarrelStabilizer.CanUse(out act)) return true;
                    if (BarrelStabilizer.CanUse(out act)) return true;

                    // 3. Reassemble
                    if (Reassemble.CurrentCharges > 0)
                    {
                        if (Reassemble.CurrentCharges == 1)
                        {
                            if (nextGCD == ChainSaw && Wildfire.ElapsedAfter(65))
                            {
                                if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return false;
                            }

                            if ((nextGCD == ChainSaw || nextGCD == Drill || nextGCD == AirAnchor) && !Wildfire.ElapsedAfter(65))
                            {
                                if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
                            }
                        }
                        if (Reassemble.CurrentCharges == 1 && Reassemble.WillHaveOneCharge(55))
                        {
                            if ((nextGCD == ChainSaw || nextGCD == Drill || nextGCD == AirAnchor))
                            {
                                if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
                            }
                        }
                    }

                    // 4. Queen
                    if (InCombat)
                    {
                        if (!Wildfire.ElapsedAfter(60))
                        {
                            if (RookAutoturret.CanUse(out act)) return true;
                        }

                        if (Wildfire.ElapsedAfter(60) || !Wildfire.IsInCooldown)
                        {
                            if (Battery >= 80 && (nextGCD == AirAnchor || nextGCD == ChainSaw) || Battery == 100 && (nextGCD == CleanShot || nextGCD == AirAnchor || nextGCD == ChainSaw))

                                if (RookAutoturret.CanUse(out act)) return true;
                        }
                    }

                    // 5. Hypercharge
                    if (SafeToUseHypercharge || !Drill.EnoughLevel)
                    {
                        if (!Wildfire.ElapsedAfter(80))
                        {
                            if (Hypercharge.CanUse(out act)) return true;
                        }

                        if (Wildfire.ElapsedAfter(80) || !Wildfire.IsInCooldown)
                        {
                            if (Heat == 100 && (nextGCD == SplitShot || nextGCD == SlugShot || nextGCD == CleanShot))

                                if (Hypercharge.CanUse(out act)) return true;
                        }
                    }

                    break;

                /// No opener
                case 5:
                    // 1. Wildfire
                    if (Wildfire.CanUse(out act, CanUseOption.EmptyOrSkipCombo | CanUseOption.OnLastAbility)) { if (nextGCD == ChainSaw || IsLastGCD(ActionID.ChainSaw)) return true; }

                    // 2. BarrelStabilizer
                    if (BarrelStabilizer.CanUse(out act)) return true;

                    // 3. Reassemble
                    if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo) && (AirAnchor.IsCoolingDown)) { if (nextGCD == ChainSaw || nextGCD == AirAnchor) return true; }

                    // 4. Queen
                    if (InCombat && RookAutoturret.CanUse(out act))
                    {
                        // The second Queen at one minute
                        if (!CombatElapsedLess(31) && CombatElapsedLess(80) && Battery >= 60) { return true; }

                        // Even Batteries
                        if (Wildfire.ElapsedAfter(115) && Battery >= 50) { return true; }

                        // Odd Batteries
                        if (Battery >= 50 && !Wildfire.ElapsedAfter(80)) { return true; }
                    }

                    // 5. Hypercharge
                    if ((SafeToUseHypercharge
                        || !Drill.EnoughLevel) && !Wildfire.ElapsedAfter(90) && Wildfire.IsCoolingDown) return true; break;
            }
        }


        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction act)
    {
        // Loops the Opener method
        if (InOpener) { return Opener(out act); }

        // Handles the useage of GR and RC
        if (!InOpener)
        {
            if (GaussRound.CurrentCharges >= Ricochet.CurrentCharges)
            {
                if (NextAbilityToNextGCD > GaussRound.AnimationLockTime + Ping && GaussRound.CanUse(out act, CanUseOption.MustUseEmpty))
                    return true;
            }
            if (NextAbilityToNextGCD > Ricochet.AnimationLockTime + Ping && Ricochet.CanUse(out act, CanUseOption.MustUseEmpty))
                return true;
        }

        if (Player.Level < 90)
        {
            if (GaussRound.CurrentCharges >= Ricochet.CurrentCharges)
            {
                if (NextAbilityToNextGCD > GaussRound.AnimationLockTime + Ping && GaussRound.CanUse(out act, CanUseOption.MustUseEmpty))
                    return true;
            }
            if (NextAbilityToNextGCD > Ricochet.AnimationLockTime + Ping && Ricochet.CanUse(out act, CanUseOption.MustUseEmpty))
                return true;
        }


        act = null;
        return false;
    }
    #endregion

    #region Extra Methods

    protected override void UpdateInfo()
    {
        if (Player.Level >= 90) // If the player is not level 90 there's no need to check for any of this as the rotation is currently meant for level 90
        {
            HandleOpenerAvailability();
            ToolKitCheck();

            if (OpenerActionsAvailable && InCombat)
            { InOpener = true; }
        }

        if (!InCombat || Player.IsDead) // safety measure
        {
            Openerstep = 0;
            InOpener = false;
        }
    }

    private void ToolKitCheck() // Sets the bool variable 'SafeToUseHypercharge' to True if Hypercharge is safe to use
    {
        bool DrillCheck = !Drill.WillHaveOneCharge(5 + Ping);
        bool AirAnchorCheck = !AirAnchor.WillHaveOneCharge(8f);
        bool ChainSawCheck = !ChainSaw.WillHaveOneCharge(8f);
        if (Player.Level >= 90)
        {
            SafeToUseHypercharge = DrillCheck && AirAnchorCheck && ChainSawCheck;
            
        }
    }

    private void HandleOpenerAvailability()
    {
        bool HasChainSaw = !ChainSaw.IsCoolingDown;
        bool HasBarrelStabilizer = !BarrelStabilizer.IsCoolingDown;
        bool HasRicochet = Ricochet.CurrentCharges == 3;
        bool HasWildfire = !Wildfire.IsCoolingDown;
        bool HasGaussRound = GaussRound.CurrentCharges == 3;

        if (ChainSaw.EnoughLevel)
        {
            OpenerActionsAvailable = HasChainSaw && HasBarrelStabilizer && HasRicochet && HasWildfire && HasGaussRound;
        }
        else
        {
            OpenerActionsAvailable = false;

        }
    }

    public override void OnTerritoryChanged()
    {
        Openerstep = 0;
        InOpener = false;
        PluginLog.LogInformation("Changing Territory | Opener is set to false and steps are 0");
    }
    #endregion Extra Methods

    #region Debug Menu
    public override void DisplayStatus()
    {
        var PartySize = PartyMembers.Count();

        #region Opener var's

        var Opener_Available = OpenerActionsAvailable ? "Available" : "Unavailable";
        var Opener_In_Progress = InOpener ? "In Progress" : "Not in Progress";

        #endregion Opener var's

        #region Colours

        var yellow = new Vector4(1, 1, 0, 1);
        var Red = new Vector4(1, 0, 0, 1);
        var Green = new Vector4(0, 1, 0, 1);
        var Blue = new Vector4(0, (float)0.5882352941176471, (float)0.7843137254901961, 1);
        var Purple = new Vector4((float)0.5882352941176471, 0, 1, 1);
        var Orange = new Vector4((float)0.9607843137254902, (float)0.592156862745098, (float)0.15294117647058825, 1);

        #endregion Colours

        if (ImGui.Button("Test"))
        {
            PluginLog.LogInformation("Test button presed!");
        }

        ImGui.Separator();
        ImGui.BeginChild("child", new Vector2(380, 0), true, ImGuiWindowFlags.None);
        ImGui.TextColored(Red, "Content Type: " + TerritoryContentType + "");
        ImGui.TextColored(Orange, "Heat:   " + Heat); ImGui.SameLine(); ImGui.Text("   |   "); ImGui.SameLine(); ImGui.TextColored(Blue, "Battery:   " + Battery + ""); ImGui.SameLine(); ImGui.SameLine(); ImGui.TextColored(Green, "     Health:   " + Player.CurrentHp + ""); ImGui.SameLine(); ImGui.TextColored(Purple, "       MP:   " + Player.CurrentMp);
        if (ImGui.CollapsingHeader("| Rotation Info |"))
        {
            if (ImGui.BeginTable("RotationInfo", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable))
            {
                ImGui.Indent();
                ImGui.TableNextColumn(); ImGui.TextColored(Orange, "" + RotationName);
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, Name);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "Game Version: " + GameVersion);
                ImGui.TableNextColumn(); ImGui.TextColored(Purple, "Ping:   " + Ping + "ms");
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("Content Type: ");
                ImGui.TableNextColumn(); ImGui.Text(TerritoryContentType.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("Party Size: ");
                ImGui.TableNextColumn(); ImGui.TextDisabled("" + PartySize);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextRow();
                ImGui.Unindent();
                ImGui.EndTable();
            }
        }
        if (ImGui.CollapsingHeader("| Opener Info |"))
        {
            if (ImGui.BeginTable("OpenerInfo", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable))
            {
                ImGui.Indent();
                ImGui.TableNextColumn(); ImGui.Text("Opener availability:");
                ImGui.TableNextColumn(); ImGui.Text(Opener_Available);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("Opener progression:");
                ImGui.TableNextColumn(); ImGui.Text(Opener_In_Progress);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("Opener Step:");
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, Openerstep.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextRow();
                ImGui.Unindent();
                ImGui.EndTable();
            }
        }
        ImGui.Separator();

        if (ImGui.CollapsingHeader("| Action Info |"))
        {
            if (ImGui.BeginTable("ActionInfo", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable))
            {
                ImGui.Indent();
                ImGui.TableNextColumn(); ImGui.Text("Safe To Use Hypercharge:");
                ImGui.TableNextColumn(); ImGui.Text(SafeToUseHypercharge.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("Is HeatBlast last used GCD:");
                ImGui.TableNextColumn(); ImGui.Text(IsLastGCD(false, HeatBlast).ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("OverHeat Stacks:");
                ImGui.TableNextColumn(); ImGui.Text(Player.StatusStack(true, StatusID.Overheated).ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextRow();
                ImGui.Unindent();
                ImGui.EndTable();
            }
        }
        ImGui.Separator();

        if (ImGui.CollapsingHeader("| Other Info |"))
        {
            if (ImGui.BeginTable("OtherInfo", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable))
            {
                ImGui.Indent();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "GCD time: " + WeaponTotal);
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, " " + Ping);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "GCD time remaining: ");
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "" + WeaponRemain);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "oGCD time left: ");
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, " " + NextAbilityToNextGCD);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(Orange, "Combat Timer:");
                ImGui.TableNextColumn(); ImGui.Text("" + ((int)CombatTime));
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.TableNextColumn(); ImGui.TextDisabled("");
                ImGui.Unindent();
                ImGui.EndTable();
            }
        }
        ImGui.EndChild();
    }
    #endregion Debug Menu

}
