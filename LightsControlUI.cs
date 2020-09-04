using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Daniels.Common;
using Daniels.UI;
using Daniels.Lighting;

namespace Daniels.PrincipalHall.Lighting
{
    public class LightsControlUI
    {
        private LightsControl _lightsControl;
        private List<BasicTriListWithSmartObject> _panels = new List<BasicTriListWithSmartObject>();
        private List<LightGroup> _stageLights;
        private LightGroup _ptzLights;

        /// <summary>
        /// Enumeration of the UI smart object ids in the project.
        /// </summary>
        private enum eUISmartObjectIds : uint
        {
            StageMasters = 4,
            StageIndividuals = 5,
            StagePresets = 12,
            PTZMasters = 18,
            PTZIndividuals = 19,
            PTZExtended = 15,
            PTZVerticalPresets = 16,
            PTZHorizontalPresets = 17,
        }

        private enum eMastersSLRJoins : uint
        {
            // Boolean
            Selected = 1,
            Raise = 3,
            Slider = 4,
            Lower = 5,
            Toggle = 6,
            // Analog
            Intensity = 1,
            // Text
            Name = 1,

            // SLR Steps
            BooleanStep = 10,
            UShortStep = 10,
            StringStep = 10,
        }

        private enum eIndividualSLRJoins : uint
        {
            // Boolean
            Selected = 1,
            Raise = 3,
            Slider = 4,
            Lower = 5,
            Toggle = 6,
            EffectiveOn = 8,
            // Analog
            Intensity = 1,
            EffectiveIntensity = 7,
            // Text
            Name = 1,

            // SLR Steps
            BooleanStep = 10,
            UShortStep = 10,
            StringStep = 10,
        }

        private enum ePTZExtendedSLRJoins : uint
        {
            // Boolean
            PanSet = 6,
            TiltSet = 7,
            ZoomSet = 3,
            FocusSet = 4,
            IrisSet = 5,
            ZoomPlus = 13,
            ZoomMinus = 14,
            FocusPlus = 15,
            FocusMinus = 16,
            IrisPlus = 17,
            IrisMinus = 18,
            BladePlus = 19,
            BladeMinus = 20,
            BladeRotatePlus = 21,
            BladeRotateMinus = 22,
            Blade1 = 23,
            Blade2 = 24,
            Blade3 = 25,
            Blade4 = 26,
            // Analog
            Pan = 6,
            Tilt = 7,
            Zoom = 3,
            Focus = 4,
            Iris = 5,
            Blade = 8,
            BladeRotation = 9,
            // Text
            Name = 1,

            // SLR Steps
            BooleanStep = 30,
            UShortStep = 20,
            StringStep = 20,
        }

        private SubPageParameters ptzExtendedSubPageParameters = new SubPageParameters()
        {
            VisibilityJoin = 107,
            TransitionJoin = 107,
            CloseJoins = new List<uint>(1) { 1 },
            BooleanOffset = 700,
            AnalogOffset = 700,
            SerialOffset = 700,
        };

        private enum ePresetsSLRJoins : uint
        {
            // Boolean
            Selected = 1,
            Save = 2,
            Delete = 3,
            SaveDeleteEnabled = 4,
            // Analog
            // Text
            MasterName = 1,
            Name = 2,

            // SLR Steps
            BooleanStep = 10,
            UShortStep = 10,
            StringStep = 10,
        }

        private const uint nameSubPageOK = 1;
        private TextEntrySubPageParameters nameSubPageParameters = new TextEntrySubPageParameters()
        {
            VisibilityJoin = 120,
            TransitionJoin = 120,
            CloseJoins = new List<uint>(2) { nameSubPageOK, 2 },
            TextEntryJoin = 1,
            BooleanOffset = 2000,
            AnalogOffset = 2000,
            SerialOffset = 2000,
        };

        private UISLRHelper _stageMasterSLR = new UISLRHelper((uint)eUISmartObjectIds.StageMasters, (uint)eMastersSLRJoins.BooleanStep, (uint)eMastersSLRJoins.UShortStep, (uint)eMastersSLRJoins.StringStep);
        private UISLRHelper _stageIndividualSLR = new UISLRHelper((uint)eUISmartObjectIds.StageIndividuals, (uint)eIndividualSLRJoins.BooleanStep, (uint)eIndividualSLRJoins.UShortStep, (uint)eIndividualSLRJoins.StringStep);
        private UISLRHelper _stagePresetsSLR = new UISLRHelper((uint)eUISmartObjectIds.StagePresets, (uint)ePresetsSLRJoins.BooleanStep, (uint)ePresetsSLRJoins.UShortStep, (uint)ePresetsSLRJoins.StringStep);

        private UISLRHelper _ptzMasterSLR = new UISLRHelper((uint)eUISmartObjectIds.PTZMasters, (uint)eMastersSLRJoins.BooleanStep, (uint)eMastersSLRJoins.UShortStep, (uint)eMastersSLRJoins.StringStep);
        private UISLRHelper _ptzIndividualSLR = new UISLRHelper((uint)eUISmartObjectIds.PTZIndividuals, (uint)eIndividualSLRJoins.BooleanStep, (uint)eIndividualSLRJoins.UShortStep, (uint)eIndividualSLRJoins.StringStep);
        private UISLRHelper _ptzExtendedSLR = new UISLRHelper((uint)eUISmartObjectIds.PTZExtended, (uint)ePTZExtendedSLRJoins.BooleanStep, (uint)ePTZExtendedSLRJoins.UShortStep, (uint)ePTZExtendedSLRJoins.StringStep);
        private UISLRHelper _ptzVPresetsSLR = new UISLRHelper((uint)eUISmartObjectIds.PTZVerticalPresets, (uint)ePresetsSLRJoins.BooleanStep, (uint)ePresetsSLRJoins.UShortStep, (uint)ePresetsSLRJoins.StringStep);
        private UISLRHelper _ptzHPresetsSLR = new UISLRHelper((uint)eUISmartObjectIds.PTZHorizontalPresets, (uint)ePresetsSLRJoins.BooleanStep, (uint)ePresetsSLRJoins.UShortStep, (uint)ePresetsSLRJoins.StringStep);

        public LightsControlUI(List<BasicTriListWithSmartObject> panels)
        {
            _panels = panels;

            foreach (var panel in _panels)
            {
                panel.UserSpecifiedObject = new SubPageManager(panel, new List<SubPage>()
                {
                    new SubPage(ptzExtendedSubPageParameters),
                    //new SubPage((uint)eUISubPageIds.PTZExtendedControl, (uint)eUISubPageIds.PTZExtendedControlTranzition, new List<uint>(){(uint)ePTZExtendedSubPageJoins.Close}, (uint)ePTZExtendedSubPageJoins.BooleanOffset, (uint)ePTZExtendedSubPageJoins.UShortOffset, (uint)ePTZExtendedSubPageJoins.StringOffset),
                    new TextEntrySubPage(nameSubPageParameters),
                    //new TextEntrySubPage((uint)eUISubPageIds.PresetName, (uint)eUISubPageIds.PresetNameTranzition, (uint)eNameSubPageJoins.Text, new List<uint>(){(uint)eNameSubPageJoins.OK, (uint)eNameSubPageJoins.Cancel}, (uint)eNameSubPageJoins.BooleanOffset, (uint)eNameSubPageJoins.UShortOffset, (uint)eNameSubPageJoins.StringOffset),
                });
            }
        }

        public void Initialize()
        {
            try
            {

                _lightsControl = LightsControl.GetInstance();

                _stageLights = _lightsControl.Where(g => (g.Name == "Podium") || (g.Name == "Stage")).ToList();
                _ptzLights = _lightsControl.Find(g => g.Name == "PTZ");

                //ushort stageLightsCount = (ushort)(_stageLights.Count + _stageLights.SelectMany(g => g).Count());

                foreach (var panel in _panels)
                {
                    // Global SLR assigments
                    SmartObject stageMasterSO = panel.SmartObjects[_stageMasterSLR.Id];
                    SmartObject stageIndividualSO = panel.SmartObjects[_stageIndividualSLR.Id];
                    SmartObject stagePresetsSO = panel.SmartObjects[_stagePresetsSLR.Id];

                    SmartObject ptzMasterSO = panel.SmartObjects[_ptzMasterSLR.Id];
                    SmartObject ptzIndividualSO = panel.SmartObjects[_ptzIndividualSLR.Id];
                    SmartObject ptzExtendedSO = panel.SmartObjects[_ptzExtendedSLR.Id];
                    SmartObject ptzVPresetsSO = panel.SmartObjects[_ptzVPresetsSLR.Id];
                    SmartObject ptzHPresetsSO = panel.SmartObjects[_ptzHPresetsSLR.Id];

                    stageMasterSO.UShortInput[_stageMasterSLR.SetNumberOfItems].UShortValue = (ushort)_stageLights.Count;
                    stageIndividualSO.UShortInput[_stageIndividualSLR.SetNumberOfItems].UShortValue = (ushort)_stageLights.SelectMany(g => g).Count();

                    ptzMasterSO.UShortInput[_ptzMasterSLR.SetNumberOfItems].UShortValue = 1;
                    ptzIndividualSO.UShortInput[_ptzIndividualSLR.SetNumberOfItems].UShortValue = (ushort)_ptzLights.Count();
                    ptzExtendedSO.UShortInput[_ptzExtendedSLR.SetNumberOfItems].UShortValue = (ushort)_ptzLights.Count();

                    recreateStagePresetsSO(stagePresetsSO);
                    recreatePTZPresetsSO(ptzVPresetsSO);
                    recreatePTZPresetsSO(ptzHPresetsSO);

                    stageMasterSO.SigChange += new Crestron.SimplSharpPro.SmartObjectSigChangeEventHandler(UIActionEvents);
                    stageIndividualSO.SigChange += new Crestron.SimplSharpPro.SmartObjectSigChangeEventHandler(UIActionEvents);
                    stagePresetsSO.SigChange += new Crestron.SimplSharpPro.SmartObjectSigChangeEventHandler(UIActionEvents);

                    ptzMasterSO.SigChange += new Crestron.SimplSharpPro.SmartObjectSigChangeEventHandler(UIActionEvents);
                    ptzIndividualSO.SigChange += new Crestron.SimplSharpPro.SmartObjectSigChangeEventHandler(UIActionEvents);
                    ptzExtendedSO.SigChange += new Crestron.SimplSharpPro.SmartObjectSigChangeEventHandler(UIActionEvents);
                    ptzVPresetsSO.SigChange += new Crestron.SimplSharpPro.SmartObjectSigChangeEventHandler(UIActionEvents);
                    ptzHPresetsSO.SigChange += new Crestron.SimplSharpPro.SmartObjectSigChangeEventHandler(UIActionEvents);

                    // Stage Lights
                    uint lightMasterIndex = 0;
                    uint lightFixtureIndex = 0;
                    foreach (LightGroup lightGroup in _stageLights)
                    {
                        // Masters
                        lightMasterIndex++;

                        //Values
                        stageMasterSO.BooleanInput[_stageMasterSLR.ItemVisible(lightMasterIndex)].UserObject = false; // Selected Master
                        stageMasterSO.BooleanInput[_stageMasterSLR.BooleanInput(lightMasterIndex, (uint)eMastersSLRJoins.Selected)].BoolValue = false; // Selected Master
                        stageMasterSO.StringInput[_stageMasterSLR.StringInput(lightMasterIndex, (uint)eMastersSLRJoins.Name)].StringValue = lightGroup.Name;
                        stageMasterSO.UShortInput[_stageMasterSLR.UShortInput(lightMasterIndex, (uint)eMastersSLRJoins.Intensity)].UShortValue = lightGroup.Intensity;
                        stageMasterSO.BooleanInput[_stageMasterSLR.BooleanInput(lightMasterIndex, (uint)eMastersSLRJoins.Toggle)].BoolValue = !lightGroup.Muted;

                        // Actions
                        LightGroup lightGroupFix = lightGroup; // For .NET 3.5 bug in Action inline functions
                        uint lightMasterIndexFix = lightMasterIndex; // For .NET 3.5 bug in Action inline functions
                        stageMasterSO.BooleanOutput[_stageMasterSLR.BooleanOutput(lightMasterIndex, (uint)eMastersSLRJoins.Selected)].UserObject = new Action<bool>(x =>
                        {
                            if (x)
                            {
                                //bool selected = !(bool)stageMasterSO.BooleanInput[_stageMasterSLR.ItemVisible(currentIndex)].UserObject;

                                uint selectedLightMasterIndex = 0;
                                foreach (LightGroup lightMaster in _stageLights)
                                {
                                    selectedLightMasterIndex++;
                                    bool selected = false;
                                    if (selectedLightMasterIndex == lightMasterIndexFix)
                                        selected = true;
                                    stageMasterSO.BooleanInput[_stageMasterSLR.ItemVisible(selectedLightMasterIndex)].UserObject = selected;
                                    stageMasterSO.BooleanInput[_stageMasterSLR.BooleanInput(selectedLightMasterIndex, (uint)eMastersSLRJoins.Selected)].BoolValue = selected;

                                    //uint firstFixtureIndex = GetStageLightSLROrdinal(lightGroupFix.FirstOrDefault());
                                    uint firstFixtureIndex = GetStageLightSLROrdinal(lightMaster.FirstOrDefault());
                                    for (uint index = firstFixtureIndex; index < (firstFixtureIndex + lightMaster.Count()); index++)
                                    {
                                        stageIndividualSO.BooleanInput[_stageMasterSLR.BooleanInput(index, (uint)eIndividualSLRJoins.Selected)].BoolValue = selected;
                                    }
                                }
                            }
                        });
                        stageMasterSO.BooleanOutput[_stageMasterSLR.BooleanOutput(lightMasterIndex, (uint)eMastersSLRJoins.Toggle)].UserObject = new Action<bool>(x => { if (x) lightGroupFix.Muted = !lightGroupFix.Muted; });
                        stageMasterSO.BooleanOutput[_stageMasterSLR.BooleanOutput(lightMasterIndex, (uint)eMastersSLRJoins.Raise)].UserObject = new Action<bool>(x => { if (x) lightGroupFix.Intensity += ushort.MaxValue / 100; });
                        stageMasterSO.BooleanOutput[_stageMasterSLR.BooleanOutput(lightMasterIndex, (uint)eMastersSLRJoins.Lower)].UserObject = new Action<bool>(x => { if (x) lightGroupFix.Intensity -= ushort.MaxValue / 100; });
                        stageMasterSO.UShortOutput[_stageMasterSLR.UShortOutput(lightMasterIndex, (uint)eMastersSLRJoins.Intensity)].UserObject = new Action<ushort>(x => { lightGroupFix.Intensity = x; });
                        
                        // Individual Lights
                        foreach (LightFixture lightFixture in lightGroup)
                        {
                            lightFixtureIndex++;
                            
                            // Values
                            stageIndividualSO.StringInput[_stageIndividualSLR.StringInput(lightFixtureIndex, (uint)eIndividualSLRJoins.Name)].StringValue = lightFixture.Name;
                            stageIndividualSO.UShortInput[_stageIndividualSLR.UShortInput(lightFixtureIndex, (uint)eIndividualSLRJoins.Intensity)].UShortValue = lightFixture.Intensity;
                            stageIndividualSO.UShortInput[_stageIndividualSLR.UShortInput(lightFixtureIndex, (uint)eIndividualSLRJoins.EffectiveIntensity)].UShortValue = lightFixture.EffectiveIntensity;
                            stageIndividualSO.BooleanInput[_stageIndividualSLR.BooleanInput(lightFixtureIndex, (uint)eIndividualSLRJoins.Toggle)].BoolValue = !lightFixture.Muted;
                            stageIndividualSO.BooleanInput[_stageIndividualSLR.BooleanInput(lightFixtureIndex, (uint)eIndividualSLRJoins.EffectiveOn)].BoolValue = !lightFixture.EffectiveMute;

                            // Actions
                            LightFixture lightFixtureFix = lightFixture; // For .NET 3.5 bug in Action inline functions
                            stageIndividualSO.BooleanOutput[_stageIndividualSLR.BooleanOutput(lightFixtureIndex, (uint)eIndividualSLRJoins.Toggle)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Muted = !lightFixtureFix.Muted; });
                            stageIndividualSO.BooleanOutput[_stageIndividualSLR.BooleanOutput(lightFixtureIndex, (uint)eIndividualSLRJoins.Raise)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Intensity += ushort.MaxValue / 100; });
                            stageIndividualSO.BooleanOutput[_stageIndividualSLR.BooleanOutput(lightFixtureIndex, (uint)eIndividualSLRJoins.Lower)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Intensity -= ushort.MaxValue / 100; });
                            stageIndividualSO.UShortOutput[_stageIndividualSLR.UShortOutput(lightFixtureIndex, (uint)eIndividualSLRJoins.Intensity)].UserObject = new Action<ushort>(x => { lightFixtureFix.Intensity = x; });
                            }
                    }

                    foreach(LightGroup lightMaster in _stageLights)
                    {
                        lightMaster.MuteChanged += new EventHandler<ReadOnlyEventArgs<bool>>(lightMasterMuteChangedEvent);
                        lightMaster.IntensityChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(lightMasterIntensityChangedEvent);
                        foreach (LightFixture lightFixture in lightMaster)
                        {
                            lightFixture.MuteChanged += new EventHandler<ReadOnlyEventArgs<bool>>(lightFixtureMuteChangedEvent);
                            lightFixture.IntensityChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(lightFixtureIntensityChangedEvent);
                            lightFixture.EffectiveIntensityChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(lightFixtureEffectiveIntensityChangedEvent);
                            lightFixture.EffectiveMuteChanged += new EventHandler<ReadOnlyEventArgs<bool>>(lightFixtureEffectiveMuteChangedEvent);
                        }
                    }


                    //PTZ Lights
                    //Values
                    ptzMasterSO.BooleanInput[_ptzMasterSLR.ItemVisible(1)].UserObject = false; // Selected Master
                    ptzMasterSO.BooleanInput[_ptzMasterSLR.BooleanInput(1, (uint)eMastersSLRJoins.Selected)].BoolValue = false; // Selected Master
                    ptzMasterSO.StringInput[_ptzMasterSLR.StringInput(1, (uint)eMastersSLRJoins.Name)].StringValue = _ptzLights.Name;
                    ptzMasterSO.UShortInput[_ptzMasterSLR.UShortInput(1, (uint)eMastersSLRJoins.Intensity)].UShortValue = _ptzLights.Intensity;
                    ptzMasterSO.BooleanInput[_ptzMasterSLR.BooleanInput(1, (uint)eMastersSLRJoins.Toggle)].BoolValue = !_ptzLights.Muted;

                    // Actions
                    BasicTriList panelFix = panel; // For .NET 3.5 bug in Action inline functions
                    ptzMasterSO.BooleanOutput[_ptzMasterSLR.BooleanOutput(1, (uint)eMastersSLRJoins.Selected)].UserObject = new Action<bool>(x =>
                    {
                        if (x)
                        {
                            SubPageManager manager = panelFix.UserSpecifiedObject as SubPageManager;
                            manager[ptzExtendedSubPageParameters.VisibilityJoin].Visible = true;
                        }
                    });
                    ptzMasterSO.BooleanOutput[_ptzMasterSLR.BooleanOutput(1, (uint)eMastersSLRJoins.Toggle)].UserObject = new Action<bool>(x => { if (x) _ptzLights.Muted = !_ptzLights.Muted; });
                    ptzMasterSO.BooleanOutput[_ptzMasterSLR.BooleanOutput(1, (uint)eMastersSLRJoins.Raise)].UserObject = new Action<bool>(x => { if (x) _ptzLights.Intensity += ushort.MaxValue / 100; });
                    ptzMasterSO.BooleanOutput[_ptzMasterSLR.BooleanOutput(1, (uint)eMastersSLRJoins.Lower)].UserObject = new Action<bool>(x => { if (x) _ptzLights.Intensity -= ushort.MaxValue / 100; });
                    ptzMasterSO.UShortOutput[_ptzMasterSLR.UShortOutput(1, (uint)eMastersSLRJoins.Intensity)].UserObject = new Action<ushort>(x => { _ptzLights.Intensity = x; });

                    // Individual PTZ Lights
                    lightFixtureIndex = 0;
                    foreach (DMXPTZFixture lightFixture in _ptzLights)
                    {
                        lightFixtureIndex++;

                        // Values
                        ptzIndividualSO.StringInput[_ptzIndividualSLR.StringInput(lightFixtureIndex, (uint)eIndividualSLRJoins.Name)].StringValue = lightFixture.Name;
                        ptzIndividualSO.UShortInput[_ptzIndividualSLR.UShortInput(lightFixtureIndex, (uint)eIndividualSLRJoins.Intensity)].UShortValue = lightFixture.Intensity;
                        ptzIndividualSO.UShortInput[_ptzIndividualSLR.UShortInput(lightFixtureIndex, (uint)eIndividualSLRJoins.EffectiveIntensity)].UShortValue = lightFixture.EffectiveIntensity;
                        ptzIndividualSO.BooleanInput[_ptzIndividualSLR.BooleanInput(lightFixtureIndex, (uint)eIndividualSLRJoins.Toggle)].BoolValue = !lightFixture.Muted;
                        ptzIndividualSO.BooleanInput[_ptzIndividualSLR.BooleanInput(lightFixtureIndex, (uint)eIndividualSLRJoins.EffectiveOn)].BoolValue = !lightFixture.EffectiveMute;

                        ptzExtendedSO.StringInput[_ptzExtendedSLR.StringInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Name)].StringValue = lightFixture.Name;
                        ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Pan)].UShortValue = lightFixture.Pan;
                        ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Tilt)].UShortValue = lightFixture.Tilt;
                        ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Zoom)].UShortValue = lightFixture.Zoom;
                        ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Focus)].UShortValue = lightFixture.Focus;
                        ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Iris)].UShortValue = lightFixture.Iris;
                        ptzExtendedSO.BooleanInput[_ptzIndividualSLR.BooleanInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade1)].BoolValue = true;
                        ptzExtendedSO.BooleanInput[_ptzIndividualSLR.BooleanInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade2)].BoolValue = false;
                        ptzExtendedSO.BooleanInput[_ptzIndividualSLR.BooleanInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade3)].BoolValue = false;
                        ptzExtendedSO.BooleanInput[_ptzIndividualSLR.BooleanInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade4)].BoolValue = false;
                        ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = lightFixture.Blade1;
                        ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = lightFixture.Blade1Rotate;
                        ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade)].UserObject = 1; //Blade#

                        // Actions
                        DMXPTZFixture lightFixtureFix = lightFixture; // For .NET 3.5 bug in Action inline functions
                        uint lightFixtureIndexFix = lightFixtureIndex; // For .NET 3.5 bug in Action inline functions
                        ptzIndividualSO.BooleanOutput[_ptzIndividualSLR.BooleanOutput(lightFixtureIndex, (uint)eIndividualSLRJoins.Toggle)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Muted = !lightFixtureFix.Muted; });
                        ptzIndividualSO.BooleanOutput[_ptzIndividualSLR.BooleanOutput(lightFixtureIndex, (uint)eIndividualSLRJoins.Raise)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Intensity += ushort.MaxValue / 100; });
                        ptzIndividualSO.BooleanOutput[_ptzIndividualSLR.BooleanOutput(lightFixtureIndex, (uint)eIndividualSLRJoins.Lower)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Intensity -= ushort.MaxValue / 100; });
                        ptzIndividualSO.UShortOutput[_ptzIndividualSLR.UShortOutput(lightFixtureIndex, (uint)eIndividualSLRJoins.Intensity)].UserObject = new Action<ushort>(x => { lightFixtureFix.Intensity = x; });

                        ptzExtendedSO.UShortOutput[_ptzExtendedSLR.UShortOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Pan)].UserObject = new Action<ushort>(x => { lightFixtureFix.Pan = x; });
                        ptzExtendedSO.UShortOutput[_ptzExtendedSLR.UShortOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Tilt)].UserObject = new Action<ushort>(x => { lightFixtureFix.Tilt = x; });
                        ptzExtendedSO.UShortOutput[_ptzExtendedSLR.UShortOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Zoom)].UserObject = new Action<ushort>(x => { lightFixtureFix.Zoom = x; });
                        ptzExtendedSO.UShortOutput[_ptzExtendedSLR.UShortOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Focus)].UserObject = new Action<ushort>(x => { lightFixtureFix.Focus = x; });
                        ptzExtendedSO.UShortOutput[_ptzExtendedSLR.UShortOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Iris)].UserObject = new Action<ushort>(x => { lightFixtureFix.Iris = x; });

                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.ZoomPlus)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Zoom += ushort.MaxValue / 100; });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.ZoomMinus)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Zoom -= ushort.MaxValue / 100; });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.FocusPlus)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Focus += ushort.MaxValue / 100; });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.FocusMinus)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Focus -= ushort.MaxValue / 100; });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.IrisPlus)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Iris += ushort.MaxValue / 100; });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.IrisMinus)].UserObject = new Action<bool>(x => { if (x) lightFixtureFix.Iris -= ushort.MaxValue / 100; });

                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade1)].UserObject = new Action<bool>(x => 
                        {
                            if (x)
                            {
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade1)].BoolValue = true;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade2)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade3)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade4)].BoolValue = false;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = lightFixtureFix.Blade1;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = lightFixtureFix.Blade1Rotate;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject = 1;
                            }
                        });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade2)].UserObject = new Action<bool>(x =>
                        {
                            if (x)
                            {
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade1)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade2)].BoolValue = true;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade3)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade4)].BoolValue = false;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = lightFixtureFix.Blade2;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = lightFixtureFix.Blade2Rotate;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject = 2;
                            }
                        });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade3)].UserObject = new Action<bool>(x =>
                        {
                            if (x)
                            {
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade1)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade2)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade3)].BoolValue = true;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade4)].BoolValue = false;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = lightFixtureFix.Blade3;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = lightFixtureFix.Blade3Rotate;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject = 3;
                            }
                        });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade4)].UserObject = new Action<bool>(x =>
                        {
                            if (x)
                            {
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade1)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade2)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade3)].BoolValue = false;
                                ptzExtendedSO.BooleanInput[_ptzExtendedSLR.BooleanInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade4)].BoolValue = true;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = lightFixtureFix.Blade4;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = lightFixtureFix.Blade4Rotate;
                                ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject = 4;
                            }
                        });

                        ptzExtendedSO.UShortOutput[_ptzExtendedSLR.UShortOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.Blade)].UserObject = new Action<ushort>(x => 
                        {
                            Int32 blade = (Int32)ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject;
                            if (blade == 1)
                                lightFixtureFix.Blade1 = x;
                            else if (blade == 2)
                                lightFixtureFix.Blade2 = x;
                            else if (blade == 3)
                                lightFixtureFix.Blade3 = x;
                            else if (blade == 4)
                                lightFixtureFix.Blade4 = x;
                        });
                        ptzExtendedSO.UShortOutput[_ptzExtendedSLR.UShortOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.BladeRotation)].UserObject = new Action<ushort>(x =>
                        {
                            Int32 blade = (Int32)ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject;
                            if (blade == 1)
                                lightFixtureFix.Blade1Rotate = x;
                            else if (blade == 2)
                                lightFixtureFix.Blade2Rotate = x;
                            else if (blade == 3)
                                lightFixtureFix.Blade3Rotate = x;
                            else if (blade == 4)
                                lightFixtureFix.Blade4Rotate = x;
                        });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.BladePlus)].UserObject = new Action<bool>(x =>
                        {
                            if (x)
                            {
                                Int32 blade = (Int32)ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject;
                                if (blade == 1)
                                    lightFixtureFix.Blade1 += ushort.MaxValue / 100;
                                else if(blade == 2)
                                    lightFixtureFix.Blade2 += ushort.MaxValue / 100;
                                else if (blade == 3)
                                    lightFixtureFix.Blade3 += ushort.MaxValue / 100;
                                else if (blade == 4)
                                    lightFixtureFix.Blade4 += ushort.MaxValue / 100;
                            }
                        });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.BladeMinus)].UserObject = new Action<bool>(x =>
                        {
                            if (x)
                            {
                                Int32 blade = (Int32)ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject;
                                if (blade == 1)
                                    lightFixtureFix.Blade1 -= ushort.MaxValue / 100;
                                else if (blade == 2)
                                    lightFixtureFix.Blade2 -= ushort.MaxValue / 100;
                                else if (blade == 3)
                                    lightFixtureFix.Blade3 -= ushort.MaxValue / 100;
                                else if (blade == 4)
                                    lightFixtureFix.Blade4 -= ushort.MaxValue / 100;
                            }
                        });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.BladeRotatePlus)].UserObject = new Action<bool>(x =>
                        {
                            if (x)
                            {
                                Int32 blade = (Int32)ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject;
                                if (blade == 1)
                                    lightFixtureFix.Blade1Rotate += ushort.MaxValue / 100;
                                else if (blade == 2)
                                    lightFixtureFix.Blade2Rotate += ushort.MaxValue / 100;
                                else if (blade == 3)
                                    lightFixtureFix.Blade3Rotate += ushort.MaxValue / 100;
                                else if (blade == 4)
                                    lightFixtureFix.Blade4Rotate += ushort.MaxValue / 100;
                            }
                        });
                        ptzExtendedSO.BooleanOutput[_ptzExtendedSLR.BooleanOutput(lightFixtureIndex, (uint)ePTZExtendedSLRJoins.BladeRotateMinus)].UserObject = new Action<bool>(x =>
                        {
                            if (x)
                            {
                                Int32 blade = (Int32)ptzExtendedSO.UShortInput[_ptzExtendedSLR.UShortInput(lightFixtureIndexFix, (uint)ePTZExtendedSLRJoins.Blade)].UserObject;
                                if (blade == 1)
                                    lightFixtureFix.Blade1Rotate -= ushort.MaxValue / 100;
                                else if (blade == 2)
                                    lightFixtureFix.Blade2Rotate -= ushort.MaxValue / 100;
                                else if (blade == 3)
                                    lightFixtureFix.Blade3Rotate -= ushort.MaxValue / 100;
                                else if (blade == 4)
                                    lightFixtureFix.Blade4Rotate -= ushort.MaxValue / 100;
                            }
                        });
                    }

                    _ptzLights.MuteChanged += new EventHandler<ReadOnlyEventArgs<bool>>(ptzMasterMuteChangedEvent);
                    _ptzLights.IntensityChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzMasterIntensityChangedEvent);
                    foreach (DMXPTZFixture lightFixture in _ptzLights)
                    {
                        lightFixture.MuteChanged += new EventHandler<ReadOnlyEventArgs<bool>>(ptzFixtureMuteChangedEvent);
                        lightFixture.IntensityChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureIntensityChangedEvent);
                        lightFixture.EffectiveIntensityChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureEffectiveIntensityChangedEvent);
                        lightFixture.EffectiveMuteChanged += new EventHandler<ReadOnlyEventArgs<bool>>(ptzFixtureEffectiveMuteChangedEvent);
                        lightFixture.PanChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixturePanChanged);
                        lightFixture.TiltChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureTiltChanged);
                        lightFixture.ZoomChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureZoomChanged);
                        lightFixture.FocusChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureFocusChanged);
                        lightFixture.IrisChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureIrisChanged);
                        lightFixture.Blade1Changed += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureBlade1Changed);
                        lightFixture.Blade1RotateChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureBlade1RotationChanged);
                        lightFixture.Blade2Changed += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureBlade2Changed);
                        lightFixture.Blade2RotateChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureBlade2RotationChanged);
                        lightFixture.Blade3Changed += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureBlade3Changed);
                        lightFixture.Blade3RotateChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureBlade3RotationChanged);
                        lightFixture.Blade4Changed += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureBlade4Changed);
                        lightFixture.Blade4RotateChanged += new EventHandler<ReadOnlyEventArgs<ushort>>(ptzFixtureBlade4RotationChanged);
                    }
                }

                ErrorLog.Notice(">>> LightsControlUI: initialized successfully");
            }
            catch (Exception e)
            {
                ErrorLog.Error(">>> LightsControlUI: Error in Initialize: {0}\r\n{1}", e.Message, e.StackTrace);
            }
        }

        void lightMasterMuteChangedEvent(object sender, ReadOnlyEventArgs<bool> e)
        {
            LightGroup lightMaster = sender as LightGroup;

            uint index = GetStageLightSLROrdinal(lightMaster);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_stageMasterSLR.Id].BooleanInput[_stageMasterSLR.BooleanInput(index, (uint)eMastersSLRJoins.Toggle)].BoolValue = !e.Parameter;
            }
        }

        void lightMasterIntensityChangedEvent(object sender, ReadOnlyEventArgs<ushort> e)
        {
            LightGroup lightMaster = sender as LightGroup;

            uint index = GetStageLightSLROrdinal(lightMaster);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_stageMasterSLR.Id].UShortInput[_stageMasterSLR.UShortInput(index, (uint)eMastersSLRJoins.Intensity)].UShortValue = e.Parameter;
            }
        }


        void lightFixtureMuteChangedEvent(object sender, ReadOnlyEventArgs<bool> e)
        {
            LightFixture lightFixture = sender as LightFixture;

            uint index = GetStageLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_stageIndividualSLR.Id].BooleanInput[_stageIndividualSLR.BooleanInput(index, (uint)eIndividualSLRJoins.Toggle)].BoolValue = !e.Parameter;
            }
        }

        void lightFixtureIntensityChangedEvent(object sender, ReadOnlyEventArgs<ushort> e)
        {
            LightFixture lightFixture = sender as LightFixture;

            uint index = GetStageLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_stageIndividualSLR.Id].UShortInput[_stageMasterSLR.UShortInput(index, (uint)eIndividualSLRJoins.Intensity)].UShortValue = e.Parameter;
            }
        }

        void lightFixtureEffectiveMuteChangedEvent(object sender, ReadOnlyEventArgs<bool> e)
        {
            LightFixture lightFixture = sender as LightFixture;

            uint index = GetStageLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_stageIndividualSLR.Id].BooleanInput[_stageIndividualSLR.BooleanInput(index, (uint)eIndividualSLRJoins.EffectiveOn)].BoolValue = !e.Parameter;
            }
        }

        void lightFixtureEffectiveIntensityChangedEvent(object sender, ReadOnlyEventArgs<ushort> e)
        {
            LightFixture lightFixture = sender as LightFixture;

            uint index = GetStageLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_stageIndividualSLR.Id].UShortInput[_stageIndividualSLR.UShortInput(index, (uint)eIndividualSLRJoins.EffectiveIntensity)].UShortValue = e.Parameter;
            }
        }

        #region PTZ Lights Events

        void ptzMasterMuteChangedEvent(object sender, ReadOnlyEventArgs<bool> e)
        {
            LightGroup lightMaster = sender as LightGroup;

            uint index = 1;

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzMasterSLR.Id].BooleanInput[_ptzMasterSLR.BooleanInput(index, (uint)eMastersSLRJoins.Toggle)].BoolValue = !e.Parameter;
            }
        }

        void ptzMasterIntensityChangedEvent(object sender, ReadOnlyEventArgs<ushort> e)
        {
            LightGroup lightMaster = sender as LightGroup;

            uint index = 1;

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzMasterSLR.Id].UShortInput[_ptzMasterSLR.UShortInput(index, (uint)eMastersSLRJoins.Intensity)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureMuteChangedEvent(object sender, ReadOnlyEventArgs<bool> e)
        {
            LightFixture lightFixture = sender as LightFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzIndividualSLR.Id].BooleanInput[_ptzIndividualSLR.BooleanInput(index, (uint)eIndividualSLRJoins.Toggle)].BoolValue = !e.Parameter;
            }
        }

        void ptzFixtureIntensityChangedEvent(object sender, ReadOnlyEventArgs<ushort> e)
        {
            LightFixture lightFixture = sender as LightFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzIndividualSLR.Id].UShortInput[_ptzMasterSLR.UShortInput(index, (uint)eIndividualSLRJoins.Intensity)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureEffectiveMuteChangedEvent(object sender, ReadOnlyEventArgs<bool> e)
        {
            LightFixture lightFixture = sender as LightFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzIndividualSLR.Id].BooleanInput[_ptzIndividualSLR.BooleanInput(index, (uint)eIndividualSLRJoins.EffectiveOn)].BoolValue = !e.Parameter;
            }
        }

        void ptzFixtureEffectiveIntensityChangedEvent(object sender, ReadOnlyEventArgs<ushort> e)
        {
            LightFixture lightFixture = sender as LightFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzIndividualSLR.Id].UShortInput[_ptzIndividualSLR.UShortInput(index, (uint)eIndividualSLRJoins.EffectiveIntensity)].UShortValue = e.Parameter;
            }
        }

        void ptzFixturePanChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Pan)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureTiltChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Tilt)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureZoomChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Zoom)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureFocusChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Focus)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureIrisChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Iris)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureBlade1Changed(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                if((Int32)panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UserObject == 1)
                    panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureBlade1RotationChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                if ((Int32)panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UserObject == 1)
                    panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureBlade2Changed(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                if ((Int32)panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UserObject == 2)
                    panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureBlade2RotationChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                if ((Int32)panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UserObject == 2)
                    panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureBlade3Changed(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                if ((Int32)panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UserObject == 3)
                    panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureBlade3RotationChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                if ((Int32)panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UserObject == 3)
                    panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureBlade4Changed(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                if ((Int32)panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UserObject == 4)
                    panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UShortValue = e.Parameter;
            }
        }

        void ptzFixtureBlade4RotationChanged(object sender, ReadOnlyEventArgs<ushort> e)
        {
            DMXPTZFixture lightFixture = sender as DMXPTZFixture;

            uint index = GetPTZLightSLROrdinal(lightFixture);

            foreach (var panel in _panels)
            {
                if ((Int32)panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.Blade)].UserObject == 4)
                    panel.SmartObjects[_ptzExtendedSLR.Id].UShortInput[_ptzExtendedSLR.UShortInput(index, (uint)ePTZExtendedSLRJoins.BladeRotation)].UShortValue = e.Parameter;
            }
        }

        #endregion PTZ Lights Events

        void UIActionEvents(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            //CrestronConsole.PrintLine("LightsControlUI.UIActionEvents: {0}:{1}:{2}", args.Event, args.Sig.Number, args.Sig.Name);

            var sig = args.Sig;
            var uo = sig.UserObject;

            if (uo is Action<bool>)                             // If the userobject for this signal with boolean
            {
                (uo as Action<bool>)(sig.BoolValue);     // cast this signal's userobject as delegate Action<bool>
                // passing one parm - the value of the bool
            }
            else if (uo is Action<ushort>)
            {
                (uo as Action<ushort>)(sig.UShortValue);
            }
            else if (uo is Action<string>)
            {
                (uo as Action<string>)(sig.StringValue);
            }
        }

        uint GetStageLightSLROrdinal(LightFixture light)
        {
            uint masterIndex = 0;
            uint fixtureIndex = 0;
            foreach (LightGroup lightMaster in _stageLights)
            {
                masterIndex++;
                if (lightMaster == light)
                    return masterIndex;

                foreach (LightFixture lightFixture in lightMaster)
                {
                    fixtureIndex++;
                    if (lightFixture == light)
                        return fixtureIndex;
                }
            }
            throw new IndexOutOfRangeException("lightMaster");
        }

        uint GetPTZLightSLROrdinal(LightFixture light)
        {
            uint fixtureIndex = 0;
            foreach (LightFixture lightFixture in _ptzLights)
            {
                fixtureIndex++;
                if (lightFixture == light)
                    return fixtureIndex;
            }
            throw new IndexOutOfRangeException("lightMaster");
        }

        void recreateStagePresetsSO(SmartObject presetSO)
        {
            int presetCount = 0;
            uint presetIndex = 0;
            //Dictionary<LightGroup, String[]> presets = new Dictionary<LightGroup, String[]>();
            foreach (LightGroup lightMaster in _stageLights)
            {
                string[] lightMasterPresets = LightsPresetManager.List(lightMaster);
                presetCount += lightMasterPresets.Length;
                presetSO.UShortInput[_stagePresetsSLR.SetNumberOfItems].UShortValue = (ushort)presetCount;
                foreach (string presetName in lightMasterPresets)
                {
                    string presetNameFix = presetName; // For .NET 3.5 bug in Action inline functions
                    LightGroup lightMasterFix = lightMaster; // For .NET 3.5 bug in Action inline functions
                    presetIndex++;
                    presetSO.StringInput[_stagePresetsSLR.StringInput(presetIndex, (uint)ePresetsSLRJoins.MasterName)].StringValue = lightMaster.Name;
                    presetSO.StringInput[_stagePresetsSLR.StringInput(presetIndex, (uint)ePresetsSLRJoins.Name)].StringValue = presetName;
                    presetSO.BooleanInput[_stagePresetsSLR.BooleanInput(presetIndex, (uint)ePresetsSLRJoins.SaveDeleteEnabled)].BoolValue = true;
                    presetSO.BooleanOutput[_stagePresetsSLR.BooleanOutput(presetIndex, (uint)ePresetsSLRJoins.Selected)].UserObject = new Action<bool>(x => { if (x) LightsPresetManager.Load(presetNameFix, lightMasterFix); });
                    presetSO.BooleanOutput[_stagePresetsSLR.BooleanOutput(presetIndex, (uint)ePresetsSLRJoins.Save)].UserObject = new Action<bool>(x => { if (x) LightsPresetManager.Save(presetNameFix, lightMasterFix); });
                    presetSO.BooleanOutput[_stagePresetsSLR.BooleanOutput(presetIndex, (uint)ePresetsSLRJoins.Delete)].UserObject = new Action<bool>(x => 
                    {
                        if (x)
                        {
                            LightsPresetManager.Delete(presetNameFix, lightMasterFix);
                            recreateStagePresetsSO(presetSO);
                        }
                    });
                }
            }
            presetCount += 1;
            presetSO.UShortInput[_stagePresetsSLR.SetNumberOfItems].UShortValue = (ushort)presetCount;
            presetSO.StringInput[_stagePresetsSLR.StringInput(presetIndex + 1, (uint)ePresetsSLRJoins.MasterName)].StringValue = "New";
            presetSO.StringInput[_stagePresetsSLR.StringInput(presetIndex + 1, (uint)ePresetsSLRJoins.Name)].StringValue = "Preset";
            presetSO.BooleanInput[_stagePresetsSLR.BooleanInput(presetIndex +1, (uint)ePresetsSLRJoins.SaveDeleteEnabled)].BoolValue = false;

            BasicTriListWithSmartObject panel = presetSO.Device as BasicTriListWithSmartObject;
            SubPageManager manager = panel.UserSpecifiedObject as SubPageManager;
            TextEntrySubPage textEntrySubPage = manager[nameSubPageParameters.VisibilityJoin] as TextEntrySubPage;
            presetSO.BooleanOutput[_stagePresetsSLR.BooleanOutput(presetIndex + 1, (uint)ePresetsSLRJoins.Selected)].UserObject = new Action<bool>(x =>
            {
                if (x)
                {
                    // Find out selected master
                    SmartObject stageMasterSO = panel.SmartObjects[_stageMasterSLR.Id];
                    LightGroup selectedLightMaster = null;
                    for (int i = 0; i < _stageLights.Count; i++)
                    {
                        bool selected = (bool)stageMasterSO.BooleanInput[_stageMasterSLR.ItemVisible((uint)i+1)].UserObject; // Selected Master
                        if (selected)
                        {
                            selectedLightMaster = _stageLights[i];
                            break;
                        }
                    }

                    if (selectedLightMaster != null)
                    {
                        // Request preset name
                        textEntrySubPage.Text = String.Empty;
                        textEntrySubPage.ShowModal((sender, args) =>
                        {
                            if (args.CloseReason == nameSubPageOK)
                            {
                                TextEntrySubPage subPage = sender as TextEntrySubPage;
                                //CrestronConsole.PrintLine("PresetName:{0} for {1}", subPage.Text, selectedLightMaster.Name);
                                if (!String.IsNullOrEmpty(subPage.Text))
                                {
                                    LightsPresetManager.Save(subPage.Text, selectedLightMaster);
                                    recreateStagePresetsSO(presetSO);
                                }
                            }
                        });
                    }
                }
            });
        }

        void recreatePTZPresetsSO(SmartObject presetSO)
        {
            int presetCount = 0;
            uint presetIndex = 0;

            LightGroup lightMaster = _ptzLights;

            string[] lightMasterPresets = LightsPresetManager.List(lightMaster);
            presetCount += lightMasterPresets.Length;
            presetSO.UShortInput[_ptzVPresetsSLR.SetNumberOfItems].UShortValue = (ushort)presetCount;
            foreach (string presetName in lightMasterPresets)
            {
                string presetNameFix = presetName; // For .NET 3.5 bug in Action inline functions
                LightGroup lightMasterFix = lightMaster; // For .NET 3.5 bug in Action inline functions
                presetIndex++;
                presetSO.StringInput[_ptzVPresetsSLR.StringInput(presetIndex, (uint)ePresetsSLRJoins.MasterName)].StringValue = lightMaster.Name;
                presetSO.StringInput[_ptzVPresetsSLR.StringInput(presetIndex, (uint)ePresetsSLRJoins.Name)].StringValue = presetName;
                presetSO.BooleanInput[_ptzVPresetsSLR.BooleanInput(presetIndex, (uint)ePresetsSLRJoins.SaveDeleteEnabled)].BoolValue = true;
                presetSO.BooleanOutput[_ptzVPresetsSLR.BooleanOutput(presetIndex, (uint)ePresetsSLRJoins.Selected)].UserObject = new Action<bool>(x => { if (x) LightsPresetManager.Load(presetNameFix, lightMasterFix); });
                presetSO.BooleanOutput[_ptzVPresetsSLR.BooleanOutput(presetIndex, (uint)ePresetsSLRJoins.Save)].UserObject = new Action<bool>(x => { if (x) LightsPresetManager.Save(presetNameFix, lightMasterFix); });
                presetSO.BooleanOutput[_ptzVPresetsSLR.BooleanOutput(presetIndex, (uint)ePresetsSLRJoins.Delete)].UserObject = new Action<bool>(x =>
                {
                    if (x)
                    {
                        LightsPresetManager.Delete(presetNameFix, lightMasterFix);
                        recreatePTZPresetsSO(presetSO);
                    }
                });
            }

            presetCount += 1;
            presetSO.UShortInput[_ptzVPresetsSLR.SetNumberOfItems].UShortValue = (ushort)presetCount;
            presetSO.StringInput[_ptzVPresetsSLR.StringInput(presetIndex + 1, (uint)ePresetsSLRJoins.MasterName)].StringValue = "New";
            presetSO.StringInput[_ptzVPresetsSLR.StringInput(presetIndex + 1, (uint)ePresetsSLRJoins.Name)].StringValue = "Preset";
            presetSO.BooleanInput[_ptzVPresetsSLR.BooleanInput(presetIndex + 1, (uint)ePresetsSLRJoins.SaveDeleteEnabled)].BoolValue = false;

            BasicTriList panel = presetSO.Device as BasicTriList;
            SubPageManager manager = panel.UserSpecifiedObject as SubPageManager;
            TextEntrySubPage textEntrySubPage = manager[nameSubPageParameters.VisibilityJoin] as TextEntrySubPage;
            presetSO.BooleanOutput[_ptzVPresetsSLR.BooleanOutput(presetIndex + 1, (uint)ePresetsSLRJoins.Selected)].UserObject = new Action<bool>(x =>
            {
                if (x)
                {
                    textEntrySubPage.Text = String.Empty;
                    textEntrySubPage.ShowModal((sender, args) =>
                        {
                            if (args.CloseReason == nameSubPageOK)
                            {
                                TextEntrySubPage subPage = sender as TextEntrySubPage;
                                //CrestronConsole.PrintLine("PresetName:{0} for {1}", subPage.Text, lightMaster.Name);
                                if (!String.IsNullOrEmpty(subPage.Text))
                                {
                                    LightsPresetManager.Save(subPage.Text, lightMaster);
                                    recreatePTZPresetsSO(presetSO);
                                }
                            }
                        });
                }
            });
        }
    }
}