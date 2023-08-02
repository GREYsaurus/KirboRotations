namespace KirboRotations.Magical;

#pragma warning disable CS0612 // Type or member is obsolete

[SourceCode("https://github.com/IncognitoWater/IncognitoWaterRotations/blob/main/IcWaRotations/Magical/Smn_Rota.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/smn/titan-first-opener.png", "TITAN FIRST OPENER")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/smn/garuda-first-opener.png", "GARUDA FIRST OPENER")]
[LinkDescription("https://www.thebalanceffxiv.com/jobs/casters/summoner/", "JOB FUNDAMENTALS")]
#pragma warning restore CS0612 // Type or member is obsolete
[RotationDesc("Rotation for SMN", ActionID.SearingLight)]
public class SMN_Kirbo : SMN_Base
{
    #region Rotation Information
    public override string GameVersion => "6.45";
    public override string RotationName => "Kirbo & Carbu";
    public override string Description => "Most of the code and useage was taken from InCognitoWater's rotation\n As such I'd like to kindly offer you my thanks";
    #endregion Rotation Information

    #region Rotation settings

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetCombo("OpenerSelection", 0, "Opener Selection", "TITAN FIRST OPENER", "GARUDA FIRST OPENER", "test")
            .SetCombo("SummonOrder", 0, "Primal Summoning Order", "[ Titan  >  Garuda  >  Ifrit ]", "[ Titan  >  Ifrit  >  Garuda ]", "[ Garuda  >  Titan  >  Ifrit ]")
            .SetCombo("addSwiftcast", 1, "Use Swiftcast?", "[   No   ]", "[   Garuda ]", "[   Ifrit ]", "[   Both ]")
            .SetBool("addCrimsonCyclone", true, "     Use Crimson Cyclone?")
            .SetFloat("SMN_JumpDistance", 10, "yalm | Distance from target to allow the useage of the Ifrit dash", 0, 25, .1f);
    }

    #endregion Rotation settings

    #region Variable's
    private int Openerstep = 0;
    private bool OpenerActionsAvailable { get; set; }
    private bool InOpener { get; set; }
    #endregion Variable's

    #region

    public static class Buffs
    {
        public const ushort
            Medicated = 49;
    }

    #endregion

    #region Countdown instructions

    protected override IAction CountDownAction(float remainTime)
    {
        if (SummonCarbuncle.CanUse(out _)) return SummonCarbuncle;
        if (Player.Level >= 90 && OpenerActionsAvailable)
        {
            if (remainTime <= Ruin.CastTime + Ruin.AnimationLockTime + Ping + 0.1 && Ruin.CanUse(out _)) return Ruin;
            if (remainTime <= 5 && IsLastAction(ActionID.Ruin))
            {
                InOpener = true;
            }
        }
        return base.CountDownAction(remainTime);
    }

    #endregion

    #region Opener instructions

    private bool Opener(out IAction act)
    {
        // Start of combat encounter
        _ = AetherCharge.CurrentCharges > 1;

        switch (Configs.GetCombo("OpenerSelection"))
        {
            // TITAN FIRST OPENER
            case 0:
                if (InOpener)
                {
                    switch (Openerstep)
                    {
                        case 0:
                            return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));

                        case 1:
                            return ProcessOpenerStep(IsLastAction(true, SummonBahamut), SummonBahamut.CanUse(out act, CanUseOption.MustUse));

                        case 2:
                            return ProcessOpenerStep(IsLastAction(false, SearingLight), SearingLight.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));

                        case 3:  
                            return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));

                        case 4: return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));
                        case 5: return ProcessOpenerStep(IsLastAction(true, Ruin) && !CombatElapsedLessGCD(3), Ruin.CanUse(out act, CanUseOption.MustUse));
                        case 6: return ProcessOpenerStep(IsLastAction(true, Ruin) && !CombatElapsedLessGCD(4), Ruin.CanUse(out act, CanUseOption.MustUse));
                        case 7: return ProcessOpenerStep(IsLastAction(true, EnergyDrain), EnergyDrain.CanUse(out act, CanUseOption.MustUse));
                        case 8: return ProcessOpenerStep(IsLastAction(true, EnkindleBahamut), EnkindleBahamut.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));
                        case 9: return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));
                        case 10: return ProcessOpenerStep(IsLastAction(false, DeathFlare), DeathFlare.CanUse(out act, CanUseOption.MustUse));
                        case 11: return ProcessOpenerStep(IsLastAction(false, Fester), Fester.CanUse(out act, CanUseOption.MustUseEmpty | CanUseOption.OnLastAbility));
                        case 12: return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));
                        case 13: return ProcessOpenerStep(IsLastAction(false, Fester), Fester.CanUse(out act, CanUseOption.MustUseEmpty));
                        case 14: return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));
                        default:
                            InOpener = false;
                            break;
                    }
                }
                break;
            // GARUDA FIRST OPENER
            case 1:
                if (InOpener)
                {
                    switch (Openerstep)
                    {
                        case 0:
                            return ProcessOpenerStep(IsLastAction(true, SummonBahamut), SummonBahamut.CanUse(out act, CanUseOption.MustUse));

                        case 1:
                            return ProcessOpenerStep(IsLastAction(false, SearingLight), SearingLight.CanUse(out act, CanUseOption.MustUseEmpty));

                        case 2:
                            return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUseEmpty));

                        case 3:
                            return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));

                        case 4:
                            return ProcessOpenerStep(IsLastAction(true, EnergyDrain), EnergyDrain.CanUse(out act, CanUseOption.MustUse));

                        case 5:
                            return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));

                        case 6:
                            return ProcessOpenerStep(IsLastAction(true, EnkindleBahamut), EnkindleBahamut.CanUse(out act, CanUseOption.MustUseEmpty));

                        case 7:
                            return ProcessOpenerStep(IsLastAction(false, Fester), Fester.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));

                        case 8:
                            return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));

                        case 9:
                            return ProcessOpenerStep(IsLastAction(false, DeathFlare), DeathFlare.CanUse(out act, CanUseOption.MustUseEmpty));

                        case 10:
                            return ProcessOpenerStep(IsLastAction(false, Fester), Fester.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility));

                        case 11:
                            return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));

                        case 12:
                            return ProcessOpenerStep(IsLastAction(true, Ruin), Ruin.CanUse(out act, CanUseOption.MustUse));

                        case 13:
                            return ProcessOpenerStep(IsLastAction(true, SummonEmerald), SummonEmerald.CanUse(out act, CanUseOption.OnLastAbility | CanUseOption.MustUseEmpty));

                        case 14:
                            return ProcessOpenerStep(IsLastAction(true, Swiftcast), Swiftcast.CanUse(out act, CanUseOption.MustUse));

                        case 15:
                            return ProcessOpenerStep(IsLastAction(false, Slipstream), Slipstream.CanUse(out act, CanUseOption.MustUseEmpty));

                        case 16:
                            return ProcessOpenerStep(IsLastAction(true, Gemshine), Gemshine.CanUse(out act, CanUseOption.MustUse));

                        case 17:
                            return ProcessOpenerStep(IsLastAction(true, Gemshine), Gemshine.CanUse(out act, CanUseOption.MustUse));

                        case 18:
                            return ProcessOpenerStep(IsLastAction(true, Gemshine), Gemshine.CanUse(out act, CanUseOption.MustUse));

                        case 19:
                            return ProcessOpenerStep(IsLastAction(true, Gemshine), Gemshine.CanUse(out act, CanUseOption.MustUse));

                        case 20:
                            return ProcessOpenerStep(IsLastAction(true, SummonTopaz), SummonTopaz.CanUse(out act, CanUseOption.MustUse));

                        default:
                            InOpener = false;
                            break;
                    }
                }
                break;
            // test
            case 2:
                if (Ruin.CanUse(out act)) return true;
                break;
        }
        act = null;
        return false;
    }

    private bool ProcessOpenerStep(bool condition, bool result)
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

    #region Movement related methods

    // Dash Ability / Action
    [RotationDesc(ActionID.CrimsonCyclone)]
    protected override bool MoveForwardGCD(out IAction act)
    {
        if (!IsMoving && CrimsonCyclone.CanUse(out act, CanUseOption.MustUse))
        {
            if (CrimsonCyclone.Target.DistanceToPlayer() <= Configs.GetFloat("SMN_JumpDistance")) return true;
        }
        return base.MoveForwardGCD(out act);
    }

    #endregion

    #region Global Cooldown Actions (GCD's)

    protected override bool GeneralGCD(out IAction act)
    {
        if (InOpener) { return Opener(out act); }
        if (!InOpener)
        {
            if (!InBahamut && !InPhoenix && !InGaruda && !InIfrit && !InTitan && SummonCarbuncle.CanUse(out act)) return true;

            if (Slipstream.CanUse(out act, CanUseOption.MustUse)) return true;

            if (CrimsonStrike.CanUse(out act, CanUseOption.MustUse)) return true;

            if (PreciousBrilliance.CanUse(out act)) return true;

            if (Gemshine.CanUse(out act)) return true;

            if (Configs.GetBool("addCrimsonCyclone") && CrimsonCyclone.CanUse(out act, CanUseOption.MustUse)) return true;

            if ((Player.HasStatus(false, StatusID.SearingLight) || SearingLight.IsCoolingDown) && SummonBahamut.CanUse(out act)) return true;
            if (!SummonBahamut.EnoughLevel && HasHostilesInRange && AetherCharge.CanUse(out act)) return true;

            if (IsMoving && (Player.HasStatus(true, StatusID.GarudasFavor) || InIfrit)
                && !Player.HasStatus(true, StatusID.SwiftCast) && !InBahamut && !InPhoenix
                && RuinIV.CanUse(out act, CanUseOption.MustUse)) return true;

            switch (Configs.GetCombo("SummonOrder"))
            {
                default:
                    if (SummonTopaz.CanUse(out act)) return true;
                    if (SummonEmerald.CanUse(out act)) return true;
                    if (SummonRuby.CanUse(out act)) return true;
                    break;

                case 1:
                    if (SummonTopaz.CanUse(out act)) return true;
                    if (SummonRuby.CanUse(out act)) return true;
                    if (SummonEmerald.CanUse(out act)) return true;
                    break;

                case 2:
                    if (SummonEmerald.CanUse(out act)) return true;
                    if (SummonTopaz.CanUse(out act)) return true;
                    if (SummonRuby.CanUse(out act)) return true;
                    break;
            }
            if (SummonTimeEndAfterGCD() && AttunmentTimeEndAfterGCD() &&
                !Player.HasStatus(true, StatusID.SwiftCast) && !InBahamut && !InPhoenix &&
                RuinIV.CanUse(out act, CanUseOption.MustUse)) return true;
            if (Outburst.CanUse(out act)) return true;

            if (Ruin.CanUse(out act)) return true;
        }
        act = null;
        return false;
    }

    #endregion

    #region Off Global Cooldown Abilities (oGCD's)

    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (InOpener) { return Opener(out act); }
        if (!InOpener && Player.HasStatus(false, StatusID.Silence) && Player.HasStatus(false, StatusID.SearingLight) && !Player.HasStatus(false, StatusID.Weakness) && InBahamut && UseBurstMedicine(out act))
        {
            return true;
        }
        if (EchoDrops.CanUse(out act)) return true;

        switch (Configs.GetCombo("addSwiftcast"))
        {
            case 0:
                break;

            case 1:
                if ((InGaruda || IsLastAction(ActionID.SummonEmerald)) && Swiftcast.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
                break;

            case 2:
                if ((InIfrit || IsLastAction(ActionID.SummonRuby)) && Swiftcast.CanUse(out act, CanUseOption.MustUse))
                    return true;
                break;

            case 3:
                if (InGaruda || InIfrit)
                {
                    if (Swiftcast.CanUse(out act, CanUseOption.MustUse)) return true;
                }
                break;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction act)
    {
        if (InOpener) { return Opener(out act); }

        if (!InOpener)
        {
            if (InBahamut && !Player.HasStatus(false, StatusID.SearingLight) && SearingLight.CanUse(out act, CanUseOption.OnLastAbility | CanUseOption.MustUse))
            {
                return true;
            }

            if ((InBahamut && SummonBahamut.ElapsedOneChargeAfterGCD(1) || InPhoenix || IsTargetBoss && IsTargetDying) && EnkindleBahamut.CanUse(out act, CanUseOption.MustUse)) return true;

            if ((InBahamut && SummonBahamut.ElapsedOneChargeAfterGCD(1) || IsTargetBoss && IsTargetDying) && DeathFlare.CanUse(out act, CanUseOption.MustUse)) return true;

            if (Rekindle.CanUse(out act, CanUseOption.MustUse)) return true;

            if (MountainBuster.CanUse(out act, CanUseOption.MustUse)) return true;

            if ((Player.HasStatus(false, StatusID.SearingLight) && InBahamut && (SummonBahamut.ElapsedOneChargeAfterGCD(2) || !EnergyDrain.IsCoolingDown && SummonBahamut.ElapsedOneChargeAfterGCD(2)) ||
                !SearingLight.EnoughLevel || IsTargetBoss && IsTargetDying) && PainFlare.CanUse(out act)) return true;

            if ((Player.HasStatus(false, StatusID.SearingLight) && InBahamut && (SummonBahamut.ElapsedOneChargeAfterGCD(2) || !EnergyDrain.IsCoolingDown && SummonBahamut.ElapsedOneChargeAfterGCD(2)) ||
                !SearingLight.EnoughLevel ||
                IsTargetBoss && IsTargetDying) && Fester.CanUse(out act)) return true;

            if (AetherCharge.CurrentCharges == 0 && EnergySiphon.CanUse(out act)) return true;

            if (AetherCharge.CurrentCharges == 0 && EnergyDrain.CanUse(out act)) return true;

            return false;
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

            if (OpenerActionsAvailable && InCombat && !InOpener)
            { InOpener = true; }

            if (!InCombat) // safety measure
            {
                Openerstep = 0;
                InOpener = false;
            }

            if (Player.IsDead || !InCombat) { Openerstep = 0; InOpener = false; }
        }
        else
        {
            // Handle opener reset if player level is below 90
            OpenerActionsAvailable = false;
            InOpener = false;
            Openerstep = 0;
        }
    }

    private void HandleOpenerAvailability()
    {
        bool HasSearingLight = !SearingLight.IsCoolingDown;
        bool HasSummonBahamut = !SummonBahamut.IsCoolingDown;
        bool HasEnergyDrain = !EnergyDrain.IsCoolingDown;
        //bool HasAether = AetherCharge.CurrentCharges == 2;

        if (SummonBahamut.EnoughLevel)
        {
            OpenerActionsAvailable = HasSearingLight && HasSummonBahamut && HasEnergyDrain;
            return;
        }
        else
        {
            OpenerActionsAvailable = false;
            return;
        }
    } // This method keeps checking if the actions needed for the opener are available to us and if so will set the variable 'OpenerActionsAvailable' to True

    public override void OnTerritoryChanged() //This method is used when the player changes the terriroty, such as go into a duty.
    {
        // This should make sure that the variable 'Reset'
        Openerstep = 0;
        InOpener = false;

        base.OnTerritoryChanged();
    }

    #endregion

    #region Debug Menu

    public override void DisplayStatus()
    {
        var PartySize = PartyMembers.Count();
        var p_name = Player.Name.ToString();
        #region Opener var's

        var Opener_Available = OpenerActionsAvailable ? "Available" : "Unavailable";
        var Opener_In_Progress = InOpener ? "In Progress" : "Not in Progress";

        #endregion Opener var's

        #region Colours
        var yellow = new Vector4(1, 1, 0, 1);
        var Red = new Vector4(1, 0, 0, 1);
        var Blue = new Vector4(0, (float)0.5882352941176471, (float)0.7843137254901961, 1);
        var Purple = new Vector4((float)0.5882352941176471, 0, 1, 1);
        var Orange = new Vector4((float)0.9607843137254902, (float)0.592156862745098, (float)0.15294117647058825, 1);
        #endregion

        ImGui.BeginChild("child", new Vector2(380, 0), true, ImGuiWindowFlags.None);
        ImGui.TextUnformatted("Content Type: " + TerritoryContentType);
        ImGui.TextColored(Red, "Health:   " + Player.CurrentHp); ImGui.SameLine(); ImGui.Text("   |   "); ImGui.SameLine(); ImGui.TextColored(Blue, "MP:   " + Player.CurrentMp);
        if (ImGui.CollapsingHeader("| Rotation Info |"))
        {
            if (ImGui.BeginTable("RotationInfo", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable))
            {
                ImGui.Indent();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "" + RotationName);
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, Name);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "Game Version: " + GameVersion);
                ImGui.TableNextColumn(); ImGui.TextUnformatted(Description);
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
                ImGui.TableNextColumn(); ImGui.Text("");
                ImGui.TableNextColumn(); ImGui.Text("");
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("");
                ImGui.TableNextColumn(); ImGui.Text("");
                ImGui.Unindent();
                ImGui.EndTable();
            }
        }
        ImGui.Separator();
        ImGui.Separator();
        if (ImGui.CollapsingHeader("| Action Info |"))
        {
            if (ImGui.BeginTable("ActionInfo", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable))
            {
                ImGui.Indent();
                ImGui.TableNextColumn(); ImGui.Text("In Combat: ");
                ImGui.TableNextColumn(); ImGui.Text(InCombat.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("Is Ruin last used GCD:");
                ImGui.TableNextColumn(); ImGui.Text(IsLastGCD(false, Ruin).ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("Searing Light: ");
                ImGui.TableNextColumn(); ImGui.Text(Player.HasStatus(true, StatusID.SearingLight).ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("");
                ImGui.TableNextColumn(); ImGui.Text("");
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
                ImGui.TableNextColumn(); ImGui.TextColored(Purple, $"Ping:    {Ping} ms");
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "GCD remaining: " + WeaponRemain);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "GCD time: " + WeaponTotal);
                ImGui.TableNextColumn();
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "oGCD time: ");
                ImGui.TableNextColumn(); ImGui.Text(NextAbilityToNextGCD.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(Orange, "Combat Timer:");
                ImGui.TableNextColumn(); ImGui.Text(CombatTime.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, "Seconds into the GCD window: ");
                ImGui.TableNextColumn(); ImGui.TextColored(yellow, WeaponElapsed.ToString());
                ImGui.Unindent();
                ImGui.EndTable();
            }
        }
        ImGui.EndChild();
    }

    #endregion
}