﻿//Please, if you use this, share the improvements

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using AgOpenGPS.Properties;
using System.Globalization;
using System.IO;
using System.Media;
using System.Reflection;
using System.Collections.Generic;

namespace AgOpenGPS
{
    public enum TBrand { AGOpenGPS, Case, Claas, Deutz, Fendt, JDeere, Kubota, Massey, NewHolland, Same, Steyr, Ursus, Valtra }
    public enum HBrand { AgOpenGPS, Case, Claas, JDeere, NewHolland }
    public enum WDBrand { AgOpenGPS, Case, Challenger, JDeere, NewHolland, Holder }

    public partial class FormGPS
    {
        //ABLines directory
        public string ablinesDirectory;
        public string fieldData, guidanceLineText;

        //colors for sections and field background
        public byte flagColor = 0;

        //how many cm off line per big pixel
        public int lightbarCmPerPixel =2;

        //polygon mode for section drawing
        public bool isDrawPolygons = false, isPauseFieldTextCounter = false;

        public CFeatureSettings featureSettings = new CFeatureSettings();

        public Color frameDayColor;
        public Color frameNightColor;
        public Color sectionColorDay;
        public Color fieldColorDay;
        public Color fieldColorNight;

        public Color textColorDay;
        public Color textColorNight;

        public Color vehicleColor;
        public double vehicleOpacity;
        public byte vehicleOpacityByte;
        public bool isVehicleImage;

        //Is it in 2D or 3D, metric or imperial, display lightbar, display grid etc
        public bool isMetric = true, isLightbarOn = true, isGridOn, isFullScreen;
        public bool isUTurnAlwaysOn, isCompassOn, isSpeedoOn, isSideGuideLines = true;
        public bool isPureDisplayOn = true, isSkyOn = true, isRollMeterOn = false, isTextureOn = true;
        public bool isDay = true, isDayTime = true, isBrightnessOn = true;
        public bool isLogElevation = false;
        public bool isKeyboardOn = true, isAutoStartAgIO = true, isSvennArrowOn = true, isTermsAccepted = false;

        public bool isUTurnOn = true, isLateralOn = true, isNudgeOn = true;

        public int[] customColorsList = new int[16];

        //sunrise sunset
        public DateTime dateToday = DateTime.Today;
        public DateTime sunrise = DateTime.Now;
        public DateTime sunset = DateTime.Now;

        public bool isFlashOnOff = false, isPanFormVisible = false;
        public bool isPanelBottomHidden = false;

        public int makeUTurnCounter = 0;

        //makes nav panel disappear after 6 seconds
        private int navPanelCounter = 0, trackMethodPanelCounter = 0;
        public uint sentenceCounter = 0;
        public int guideLineCounter = 0;

        private int currentFieldTextCounter = 0;

        //For field saving in background
        private int fileSaveCounter = 1;
        private int fourSecondCounter = 0;
        public int twoSecondCounter = 0;
        private int oneSecondCounter = 0;
        private int oneHalfSecondCounter = 0;

        public List<int> buttonOrder = new List<int>();

        //Timer triggers at 125 msec
        private void tmrWatchdog_tick(object sender, EventArgs e)
        {
            //Check for a newline char, if none then just return
            if (++sentenceCounter > 20)
            {
                ShowNoGPSWarning();
                return;
            }

            ////////////////////////////////////////////// 10 second ///////////////////////////////////////////////////////
            //every 3 second update status
            if (fourSecondCounter >= 3)
            {
                if (!isPauseFieldTextCounter)
                {
                    if (++currentFieldTextCounter > 4) currentFieldTextCounter = 0;
                }

                //reset the counter
                fourSecondCounter = 0;

                /*
                //if (isJobStarted)
                //{
                //    if (isMetric)
                //    {
                //        if (bnd.bndList.Count > 0)
                //        {
                //            fieldData =
                //                 fd.WorkedAreaRemainPercentage + "  "
                //                + fd.AreaBoundaryLessInnersHectares + " - "
                //                + fd.WorkedHectares + " = "
                //                + fd.WorkedAreaRemainHectares + " | "

                //                + fd.ActualAreaWorkedHectares + " = "
                //                + fd.ActualRemainHectares + "  "
                //                + fd.ActualOverlapPercent + " | "

                //                + fd.TimeTillFinished + "  "
                //                + fd.WorkRateHectares;
                //        }
                //        else
                //            fieldData = "Applied: "
                //                + fd.WorkedHectares + "  Actual: "
                //                + fd.ActualAreaWorkedHectares + "  "
                //                + fd.ActualOverlapPercent + "   "
                //                + fd.WorkRateHectares;

                //    }
                //    else //imperial
                //    {
                //        if (bnd.bndList.Count > 0)
                //            fieldData =
                //                 fd.WorkedAreaRemainPercentage + "  "
                //                + fd.AreaBoundaryLessInnersAcres + " - "
                //                + fd.WorkedAcres + " = "
                //                + fd.WorkedAreaRemainAcres +  " | "

                //                + fd.ActualAreaWorkedAcres + " = "
                //                + fd.ActualRemainAcres + "  "
                //                + fd.ActualOverlapPercent + " | "

                //                + fd.TimeTillFinished + "  "
                //                + fd.WorkRateAcres;
                //        else
                //            fieldData = "Applied: "
                //                + fd.WorkedAcres + "  Actual: "
                //                + fd.ActualAreaWorkedAcres + " *"
                //                + fd.ActualOverlapPercent + "   "
                //                + fd.WorkRateAcres;
                //    }
                //}
                //else
                //{
                //    fieldData = string.Empty;
                //}
                */

                if (isJobStarted)
                {
                    switch (currentFieldTextCounter)
                    {
                        case 0:
                            lblCurrentField.Text = gStr.gsField + ": " + displayFieldName;
                            break;

                        case 1:

                            lblCurrentField.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                            break;

                        case 2:
                            if (bnd.bndList.Count > 0)
                            {
                                if (isMetric)
                                {
                                    lblCurrentField.Text = fd.AreaBoundaryLessInnersHectares
                                        + "  App: " + fd.WorkedHectares
                                        + "  Actual: " + fd.ActualAreaWorkedHectares
                                        + "  " + fd.WorkedAreaRemainPercentage
                                        + "  " + fd.WorkRateHectares;

                                }
                                else
                                {
                                    lblCurrentField.Text = fd.AreaBoundaryLessInnersAcres
                                        + "  App: " + fd.WorkedAcres
                                        + "  Actual: " + fd.ActualAreaWorkedAcres
                                        + "  " + fd.WorkedAreaRemainPercentage
                                        + "  " + fd.WorkRateAcres;
                                }
                            }
                            else
                            {
                                if (isMetric)
                                {
                                    lblCurrentField.Text = "App: "
                                + fd.WorkedHectares + " Actual: "
                                + fd.ActualAreaWorkedHectares + "  "
                                + fd.ActualOverlapPercent + "   "
                                + fd.WorkRateHectares;
                                }
                                else
                                {
                                    lblCurrentField.Text = fieldData + "App: "
                                + fd.WorkedAcres + "  Actual: "
                                + fd.ActualAreaWorkedAcres + " *"
                                + fd.ActualOverlapPercent + "   "
                                + fd.WorkRateAcres;
                                }
                            }
                            break;

                        case 3:
                            if (trk.idx > -1)
                                lblCurrentField.Text = "Line: " + trk.gArr[trk.idx].name;
                            else
                                lblCurrentField.Text = "Line: " + gStr.gsNoGuidanceLines;
                            break;

                        case 4:
                            lblCurrentField.Text = "";
                            break;


                        default:
                            break;
                    }

                    if (tram.displayMode == 0) 
                        tram.isRightManualOn = tram.isLeftManualOn = false;
                }
                else
                {
                    switch (currentFieldTextCounter)
                    {
                        case 0:
                            lblCurrentField.Text = (tool.width * m2FtOrM).ToString("N2") + unitsFtM + " - " + vehicleFileName;
                            break;

                        case 1:
                            lblCurrentField.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss ");
                            break;

                        case 2:
                            lblCurrentField.Text = "Lat: " + pn.latitude.ToString("N7") + "   Lon: " + pn.longitude.ToString("N7");
                            break;

                        case 3:
                            lblCurrentField.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss ");                            
                            break;

                        case 4:
                            lblCurrentField.Text = "";
                            break;

                        default:
                            break;
                    }
                }

                if (isPauseFieldTextCounter)
                {
                    lblCurrentField.Text = "\u23F8" + " " + lblCurrentField.Text;
                }
                else
                {
                    lblCurrentField.Text = "\u25B6" + " " + lblCurrentField.Text;
                }
            }

            /////////////////////////////////////////////////////////   2 second  ////////////////////////////////////////
            //every 2 second update status
            if (twoSecondCounter >= 2)
            {
                //reset the counter
                twoSecondCounter = 0;

                //hide the Nav panel in 6  secs
                if (panelNavigation.Visible)
                {
                    if (navPanelCounter-- <= 0) panelNavigation.Visible = false;
                    lblHz.Text = gpsHz.ToString("N1") + " ~ " + (frameTime.ToString("N1")) + " " + FixQuality;
                }

                //fix
                if (timerSim.Enabled && pn.fixQuality++ > 5) pn.fixQuality = 2;

                //save nmea log file
                if (isLogNMEA) FileSaveNMEA();

            }//end every 2 seconds

            //every second update all status ///////////////////////////   1 1 1 1 1 1 ////////////////////////////
            if (oneSecondCounter >= 4)
            {
                //reset the counter
                oneSecondCounter = 0;

                //counter used for saving field in background - is actually 30 second
                fileSaveCounter++;

                //general counters
                twoSecondCounter++;
                fourSecondCounter++;

                //keeps autoTrack from changing too fast
                trk.autoTrack3SecTimer++;

                lblFix.Text = FixQuality + "Age: " + pn.age.ToString("N1");

                switch (pn.fixQuality)
                {
                    case 4:
                        btnGPSData.BackColor = Color.PaleGreen;
                        break;
                    case 5:
                        btnGPSData.BackColor = Color.Orange;
                        break;
                    case 2:
                        btnGPSData.BackColor = Color.Yellow;
                        break;                               
                    default:
                        btnGPSData.BackColor = Color.Red;
                        break;
                }

                //statusbar flash red undefined headland
                if (timerSim.Enabled)
                {
                    if (mc.isOutOfBounds && panelSim.BackColor == Color.Transparent
                        || !mc.isOutOfBounds && panelSim.BackColor == Color.Tomato)
                    {
                        if (!mc.isOutOfBounds)
                        {
                            panelSim.BackColor = Color.Transparent;
                        }
                        else
                        {
                            panelSim.BackColor = Color.Tomato;
                        }
                    }
                }

                if (flp1.Visible)
                {
                    if (trackMethodPanelCounter-- < 1) flp1.Visible = false;
                }
            }

            //every half of a second update all status  ////////////////    0.5  0.5   0.5    0.5    /////////////////
            if (oneHalfSecondCounter >= 2)
            {
                //reset the counter
                oneHalfSecondCounter = 0;

                isFlashOnOff = !isFlashOnOff;

                //the ratemap trigger
                worldGrid.isRateTrigger = true;

                //the main formgps window
                if (isMetric)  //metric or imperial
                {
                    //status strip values
                    distanceToolBtn.Text = fd.DistanceUserMeters + "\r\n" + fd.WorkedUserHectares;

                }
                else  //Imperial Measurements
                {
                    //acres on the master section soft control and sections
                    //status strip values
                    distanceToolBtn.Text = fd.DistanceUserFeet + "\r\n" + fd.WorkedUserAcres;
                }

                //Make sure it is off when it should
                if ((!ct.isContourBtnOn && trk.idx == -1 && isBtnAutoSteerOn)
                    ) btnAutoSteer.PerformClick();

                //the main formgps window
                if (isMetric)  //metric or imperial
                {
                    lblSpeed.Text = SpeedKPH;
                    //btnContour.Text = XTE; //cross track error

                }
                else  //Imperial Measurements
                {
                    lblSpeed.Text = SpeedMPH;
                    //btnContour.Text = InchXTE; //cross track error
                }

            } //end every 1/2 second

            //every fourth second update  ///////////////////////////   Fourth  ////////////////////////////
            {
                //reset the counter
                oneHalfSecondCounter++;
                oneSecondCounter++;
                makeUTurnCounter++;

                btnAutoSteerConfig.Text = SetSteerAngle + "\r\n" + ActualSteerAngle;

                secondsSinceStart = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;
            }

        }//wait till timer fires again.         

        public void LoadSettings()
        {            
            CheckSettingsNotNull();

            //metric settings
            isMetric = Settings.Default.setMenu_isMetric;
            
            //field menu
            boundariesToolStripMenuItem.Visible = Properties.Settings.Default.setFeatures.isBoundaryOn;
            headlandToolStripMenuItem.Visible = Properties.Settings.Default.setFeatures.isHeadlandOn;
            headlandBuildToolStripMenuItem.Visible = Properties.Settings.Default.setFeatures.isHeadlandOn;
            tramLinesMenuField.Visible = Properties.Settings.Default.setFeatures.isTramOn;
            recordedPathStripMenu.Visible = Properties.Settings.Default.setFeatures.isRecPathOn;


            //tools menu
            SmoothABtoolStripMenu.Visible = Properties.Settings.Default.setFeatures.isABSmoothOn;
            deleteContourPathsToolStripMenuItem.Visible = Properties.Settings.Default.setFeatures.isHideContourOn;
            webcamToolStrip.Visible = Properties.Settings.Default.setFeatures.isWebCamOn;
            offsetFixToolStrip.Visible = Properties.Settings.Default.setFeatures.isOffsetFixOn;

            //left side
            btnStartAgIO.Visible = Properties.Settings.Default.setFeatures.isAgIOOn;

            //OGL control
            isUTurnOn = Properties.Settings.Default.setFeatures.isUTurnOn;
            isLateralOn = Properties.Settings.Default.setFeatures.isLateralOn;
            isNudgeOn = Properties.Settings.Default.setFeatures.isABLineOn;

            if (isMetric)
            {
                inchOrCm2m = 0.01;
                m2InchOrCm = 100.0;

                m2FtOrM = 1.0;
                ftOrMtoM = 1.0;

                inOrCm2Cm = 1.0;
                cm2CmOrIn = 1.0;

                unitsFtM = " m";
                unitsInCm = " cm";
                unitsInCmNS = "cm";
            }
            else
            {
                inchOrCm2m = glm.in2m;
                m2InchOrCm = glm.m2in;

                m2FtOrM = glm.m2ft;
                ftOrMtoM = glm.ft2m;

                inOrCm2Cm = 2.54;
                cm2CmOrIn = 0.394;

                unitsInCm = " in";
                unitsInCmNS = "in";
                unitsFtM = " ft";
            }

            udpWatchLimit = Properties.Settings.Default.SetGPS_udpWatchMsec;
            pn.headingTrueDualOffset = Properties.Settings.Default.setGPS_dualHeadingOffset;
            dualReverseDetectionDistance = Properties.Settings.Default.setGPS_dualReverseDetectionDistance;

            frameDayColor = Properties.Settings.Default.setDisplay_colorDayFrame.CheckColorFor255();
            frameNightColor = Properties.Settings.Default.setDisplay_colorNightFrame.CheckColorFor255();
            sectionColorDay = Properties.Settings.Default.setDisplay_colorSectionsDay.CheckColorFor255();
            fieldColorDay = Properties.Settings.Default.setDisplay_colorFieldDay.CheckColorFor255();
            fieldColorNight = Properties.Settings.Default.setDisplay_colorFieldNight.CheckColorFor255();

            Properties.Settings.Default.setDisplay_colorDayFrame = frameDayColor;
            Properties.Settings.Default.setDisplay_colorNightFrame = frameNightColor;
            Properties.Settings.Default.setDisplay_colorSectionsDay = sectionColorDay;
            Properties.Settings.Default.setDisplay_colorFieldDay = fieldColorDay;
            Properties.Settings.Default.setDisplay_colorFieldNight = fieldColorNight;
            Properties.Settings.Default.Save();

            //load up colors
            textColorDay = Settings.Default.setDisplay_colorTextDay.CheckColorFor255();
            textColorNight = Settings.Default.setDisplay_colorTextNight.CheckColorFor255();

            //load the string of custom colors
            string[] words = Properties.Settings.Default.setDisplay_customColors.Split(',');
            for (int i = 0; i < 16; i++)
            {
                Color test;
                customColorsList[i] = int.Parse(words[i], CultureInfo.InvariantCulture);
                test = Color.FromArgb(customColorsList[i]).CheckColorFor255();
                int iCol = (test.A << 24) | (test.R << 16) | (test.G << 8) | test.B;
                customColorsList[i] = iCol;
            }

            Properties.Settings.Default.setDisplay_customColors = "";
            for (int i = 0; i < 15; i++)
                Properties.Settings.Default.setDisplay_customColors += customColorsList[i].ToString() + ",";
            Properties.Settings.Default.setDisplay_customColors += customColorsList[15].ToString();

            Properties.Settings.Default.Save();


            isTextureOn = Settings.Default.setDisplay_isTextureOn;
            isLogElevation = Settings.Default.setDisplay_isLogElevation;

            isGridOn = Settings.Default.setMenu_isGridOn;
            isBrightnessOn = Settings.Default.setDisplay_isBrightnessOn;

            isCompassOn = Settings.Default.setMenu_isCompassOn;
            isSpeedoOn = Settings.Default.setMenu_isSpeedoOn;
            isSideGuideLines = Settings.Default.setMenu_isSideGuideLines;
            isSvennArrowOn = Settings.Default.setDisplay_isSvennArrowOn;

            //isLogNMEA = Settings.Default.setMenu_isLogNMEA;
            isPureDisplayOn = Settings.Default.setMenu_isPureOn;

            isAutoStartAgIO = Settings.Default.setDisplay_isAutoStartAgIO;

            panelNavigation.Location = new System.Drawing.Point(90, 100);
            panelDrag.Location = new System.Drawing.Point(87, 268);

            vehicleOpacity = ((double)(Properties.Settings.Default.setDisplay_vehicleOpacity) * 0.01);
            vehicleOpacityByte = (byte)(255 * ((double)(Properties.Settings.Default.setDisplay_vehicleOpacity) * 0.01));
            isVehicleImage = Properties.Settings.Default.setDisplay_isVehicleImage;

            string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            //grab the current vehicle filename - make sure it exists
            vehicleFileName = Settings.Default.setVehicle_vehicleName;

            simulatorOnToolStripMenuItem.Checked = Settings.Default.setMenu_isSimulatorOn;
            if (simulatorOnToolStripMenuItem.Checked)
            {
                panelSim.Visible = true;
                timerSim.Enabled = true;
            }
            else
            {
                panelSim.Visible = false;
                timerSim.Enabled = false;
            }

            if (timerSim.Enabled) gpsHz = 10;

            //set the flag mark button to red dot
            btnFlag.Image = Properties.Resources.FlagRed;

            vehicleColor = Settings.Default.setDisplay_colorVehicle.CheckColorFor255();

            isLightbarOn = Settings.Default.setMenu_isLightbarOn;

            //set up grid and lightbar

            isKeyboardOn = Settings.Default.setDisplay_isKeyboardOn;

            if (Properties.Settings.Default.setAS_isAutoSteerAutoOn) btnAutoSteer.Text = "R";
            else btnAutoSteer.Text = "M";

            if (bnd.isHeadlandOn) btnHeadlandOnOff.Image = Properties.Resources.HeadlandOn;
            else btnHeadlandOnOff.Image = Properties.Resources.HeadlandOff;

            //btnChangeMappingColor.BackColor = sectionColorDay;
            btnChangeMappingColor.Text = Application.ProductVersion.ToString(CultureInfo.InvariantCulture);

            if (Properties.Settings.Default.setDisplay_isStartFullScreen)
            {
                this.WindowState = FormWindowState.Maximized;
                isFullScreen = true;
            }
            else
            {
                isFullScreen = false;
            }

            //is rtk on?
            isRTK_AlarmOn = Properties.Settings.Default.setGPS_isRTK;
            isRTK_KillAutosteer = Properties.Settings.Default.setGPS_isRTK_KillAutoSteer;

            pn.ageAlarm = Properties.Settings.Default.setGPS_ageAlarm;

            isConstantContourOn = Properties.Settings.Default.setAS_isConstantContourOn;
            isSteerInReverse = Properties.Settings.Default.setAS_isSteerInReverse;

            guidanceLookAheadTime = Properties.Settings.Default.setAS_guidanceLookAheadTime;

            gyd.sideHillCompFactor = Properties.Settings.Default.setAS_sideHillComp;

            ahrs = new CAHRS();

            fd.UpdateFieldBoundaryGUIAreas();

            btnSection1Man.Visible = false;
            btnSection2Man.Visible = false;
            btnSection3Man.Visible = false;
            btnSection4Man.Visible = false;
            btnSection5Man.Visible = false;
            btnSection6Man.Visible = false;
            btnSection7Man.Visible = false;
            btnSection8Man.Visible = false;
            btnSection9Man.Visible = false;
            btnSection10Man.Visible = false;
            btnSection11Man.Visible = false;
            btnSection12Man.Visible = false;
            btnSection13Man.Visible = false;
            btnSection14Man.Visible = false;
            btnSection15Man.Visible = false;
            btnSection16Man.Visible = false;

            btnZone1.Visible = false;
            btnZone2.Visible = false;
            btnZone3.Visible = false;
            btnZone4.Visible = false;
            btnZone5.Visible = false;
            btnZone6.Visible = false;
            btnZone7.Visible = false;
            btnZone8.Visible = false;

            if (tool.isSectionsNotZones)
            {
                //Set width of section and positions for each section
                SectionSetPosition();

                //Calculate total width and each section width
                SectionCalcWidths();
                LineUpIndividualSectionBtns();
            }
            else
            {
                SectionCalcMulti();
                LineUpAllZoneButtons();
            }

            DisableYouTurnButtons();

            //which heading source is being used
            headingFromSource = Settings.Default.setGPS_headingFromWhichSource;

            //workswitch stuff
            mc.isRemoteWorkSystemOn = Settings.Default.setF_isRemoteWorkSystemOn;

            mc.isWorkSwitchActiveLow = Settings.Default.setF_isWorkSwitchActiveLow;
            mc.isWorkSwitchManualSections = Settings.Default.setF_isWorkSwitchManualSections;
            mc.isWorkSwitchEnabled = Settings.Default.setF_isWorkSwitchEnabled;

            mc.isSteerWorkSwitchEnabled = Settings.Default.setF_isSteerWorkSwitchEnabled;
            mc.isSteerWorkSwitchManualSections = Settings.Default.setF_isSteerWorkSwitchManualSections;

            minHeadingStepDist = Settings.Default.setF_minHeadingStepDistance;
            gpsMinimumStepDistance = Settings.Default.setGPS_minimumStepLimit;

            fd.workedAreaTotalUser = Settings.Default.setF_UserTotalArea;

            yt.uTurnSmoothing = Settings.Default.setAS_uTurnSmoothing;

            tool.halfWidth = (tool.width - tool.overlap) / 2.0;
            tool.contourWidth = (tool.width - tool.overlap) / 3.0;

            //load the lightbar resolution
            lightbarCmPerPixel = 2;

            isStanleyUsed = Properties.Settings.Default.setVehicle_isStanleyUsed;

            //main window first
            if (Settings.Default.setWindow_Maximized)
            {
                WindowState = FormWindowState.Normal;
                Location = Settings.Default.setWindow_Location;
                Size = Settings.Default.setWindow_Size;
            }
            else if (Settings.Default.setWindow_Minimized)
            {
                //WindowState = FormWindowState.Minimized;
                Location = Settings.Default.setWindow_Location;
                Size = Settings.Default.setWindow_Size;
            }
            else
            {
                Location = Settings.Default.setWindow_Location;
                Size = Settings.Default.setWindow_Size;
            }

            //night mode
            isDay = Properties.Settings.Default.setDisplay_isDayMode;
            isDay = !isDay;
            SwapDayNightMode();

            //load uturn properties
            yt = new CYouTurn(this);

            lblNumCu.Visible = false;
            lblNumCu.Text = "";

            words = Properties.Settings.Default.setDisplay_buttonOrder.Split(',');
            buttonOrder?.Clear();

            for (int i = 0; i < words.Length; i++)
            {
                buttonOrder.Add(int.Parse(words[i], CultureInfo.InvariantCulture));
            }

            bnd.isSectionControlledByHeadland = Properties.Settings.Default.setHeadland_isSectionControlled;
            if (bnd.isSectionControlledByHeadland) cboxIsSectionControlled.Image = Properties.Resources.HeadlandSectionOn;
            else cboxIsSectionControlled.Image = Properties.Resources.HeadlandSectionOff;

            //right side build
            PanelBuildRightMenu();

            PanelsAndOGLSize();
            PanelUpdateRightAndBottom();

            camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
            SetZoom();

            lblGuidanceLine.BringToFront();
        }

        public void PanelUpdateRightAndBottom()
        {
            if (isJobStarted)
            {
                int tracksTotal = 0, tracksVisible = 0;
                bool isHdl = false;

                bool isBnd = bnd.bndList.Count > 0;
                if (!isBnd) isHdl = isBnd;
                else isHdl = bnd.bndList[0].hdLine.Count > 0;

                bool istram = (tram.tramList.Count + tram.tramBndOuterArr.Count) > 0;

                for (int i = 0; i < trk.gArr.Count; i++)
                {
                    tracksTotal++;
                    if (trk.gArr[i].isVisible) tracksVisible++;
                }

                btnContourLock.Visible = ct.isContourBtnOn;

                if (trk.idx > -1 || ct.isContourBtnOn)
                    btnAutoSteer.Enabled = true;
                else
                {
                    if (isBtnAutoSteerOn) btnAutoSteer.PerformClick();
                    btnAutoSteer.Enabled = false;
                }

                btnAutoYouTurn.Visible = trk.idx > -1 && !ct.isContourBtnOn && isBnd;
                btnCycleLines.Visible = tracksVisible > 1 && trk.idx > -1 && !ct.isContourBtnOn;
                btnCycleLinesBk.Visible = tracksVisible > 1 && trk.idx > -1 && !ct.isContourBtnOn;

                cboxpRowWidth.Visible = trk.idx > -1;
                btnYouSkipEnable.Visible = trk.idx > -1;

                btnSnapToPivot.Visible = trk.idx > -1 && isNudgeOn;
                btnAdjLeft.Visible = trk.idx > -1 && isNudgeOn;
                btnAdjRight.Visible = trk.idx > -1 && isNudgeOn;

                btnTramDisplayMode.Visible = istram;
                btnHeadlandOnOff.Visible = isHdl;
                btnHydLift.Visible = isHdl;
                cboxIsSectionControlled.Visible = isHdl;

                //btnResetToolHeading.Visible = this.Width > 1190;

                btnAutoTrack.Visible = tracksVisible > 1 && trk.idx > -1 && !ct.isContourBtnOn;

                if (trk.idx > -1 && trk.gArr.Count > 0 && !ct.isContourBtnOn)
                {
                    lblNumCu.Visible = true;
                    lblNumCu.Text = (trk.idx+1).ToString() + "/" + trk.gArr.Count.ToString();
                }
                else
                {
                    lblNumCu.Visible = false;
                    lblNumCu.Text = "";
                }

                PanelSizeRightAndBottom();
            }

            if (worldGrid.isRateMap)
            {
                //lblRed.Visible = lblGrn.Visible = lblBlu.Visible = false;
                //if (worldGrid.numRateChannels > 0) lblRed.Visible = true;
                //if (worldGrid.numRateChannels > 1) lblGrn.Visible = true;
                //if (worldGrid.numRateChannels > 2) lblBlu.Visible = true;
                //lblRed.Visible = true;
                //pbarRate.Visible = true;
            }
            else
            {
                //lblRed.Visible = lblGrn.Visible = lblBlu.Visible = false;
                //lblRed.Visible = false;
                //pbarRate.Visible = false;
            }
        }

        public void PanelBuildRightMenu()
        {
            panelRight.Controls.Clear();

            for (int i = 0; i < buttonOrder.Count; i++)
            {
                switch (buttonOrder[i])
                {
                    case 0:
                        panelRight.Controls.Add(btnAutoSteer);
                        break;

                    case 1:
                        panelRight.Controls.Add(btnAutoYouTurn);
                        break;

                    case 2:
                        panelRight.Controls.Add(btnSectionMasterAuto);
                        break;

                    case 3:
                        panelRight.Controls.Add(btnSectionMasterManual);
                        break;

                    case 4:
                        panelRight.Controls.Add(btnAutoTrack);
                        break;

                    case 5:
                        panelRight.Controls.Add(btnCycleLinesBk);
                        break;

                    case 6:
                        panelRight.Controls.Add(btnCycleLines);
                        break;

                    case 7:
                        panelRight.Controls.Add(btnContour);
                        panelRight.Controls.Add(btnContourLock);
                        break;

                    default:
                        break;
                }
            }

            panelRight.Controls.Add(lblNumCu);
        }

        public void PanelSizeRightAndBottom()
        {
            btnResetToolHeading.Visible = false;
            int viz = 0;
            for (int i = 0; i < panelRight.Controls.Count; i++)
            {
                if (panelRight.Controls[i].Visible && panelRight.Controls[i] is Button) viz++;
            }

            if (viz == 0) return;

            int sizer = (Height - 140) / (viz);
            if (sizer > 120) { sizer = 120; }

            for (int i = 0; i < panelRight.Controls.Count; i++)
            {
                if (panelRight.Controls[i].Visible && panelRight.Controls[i] is Button)
                {
                    panelRight.Controls[i].Height = sizer;
                }
            }

            if (panelBottom.Visible)
            {
                viz = 0;
                for (int i = 0; i < panelBottom.Controls.Count; i++)
                {
                    if (panelBottom.Controls[i].Visible && panelBottom.Controls[i] is Button)
                        viz++;
                    if (panelBottom.Controls[i].Visible && panelBottom.Controls[i] is CheckBox)
                        viz++;
                }

                if (viz == 0) return;
                if (viz > 9 && Width < 1190)
                {
                    btnResetToolHeading.Visible = false;
                }
                else
                {
                    btnResetToolHeading.Visible = true;
                    viz++;
                }

                sizer = (Width - 185) / (viz);
                if (sizer > 150) { sizer = 150; }

                for (int i = 0; i < panelBottom.Controls.Count; i++)
                {
                    if (panelBottom.Controls[i].Visible && panelBottom.Controls[i] is Button)
                        panelBottom.Controls[i].Width = sizer;
                    if (panelBottom.Controls[i].Visible && panelBottom.Controls[i] is CheckBox)
                        panelBottom.Controls[i].Width = sizer;
                }

            }

            flp1.Top = this.Height - 230;
            flp1.Left = this.Width - 120 - flp1.Width;
        }

        private void PanelsAndOGLSize()
        {
            if (!isJobStarted)
            {
                panelBottom.Visible = false;
                panelRight.Visible = false;

                oglMain.Left = 80;
                oglMain.Width = this.Width - statusStripLeft.Width - 22; //22
                oglMain.Height = this.Height - 60;
            }
            else
            {
                if (isPanelBottomHidden)
                {
                    panelBottom.Visible = false;
                    panelLeft.Visible = false;
                    oglMain.Left = 20;
                    oglMain.Width = this.Width - 98; //22
                    oglMain.Height = this.Height - 62;
                }
                else
                {
                    panelBottom.Visible = true;
                    panelRight.Visible = true;
                    panelLeft.Visible = true;
                    oglMain.Left = 80;
                    oglMain.Width = this.Width - statusStripLeft.Width - 92; //22
                    oglMain.Height = this.Height - 118;
                }
            }

            PanelSizeRightAndBottom();

            if (tool.isSectionsNotZones)
            {
                LineUpIndividualSectionBtns();
            }
            else
            {
                LineUpAllZoneButtons();
            }
        }

        private void ZoomByMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (camera.zoomValue <= 20) camera.zoomValue += camera.zoomValue * 0.06;
                else camera.zoomValue += camera.zoomValue * 0.02;
                if (camera.zoomValue > 120) camera.zoomValue = 120;
                camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
                SetZoom();
            }
            else
            {
                if (camera.zoomValue <= 20)
                { if ((camera.zoomValue -= camera.zoomValue * 0.06) < 4.0) camera.zoomValue = 4.0; }
                else { if ((camera.zoomValue -= camera.zoomValue * 0.02) < 4.0) camera.zoomValue = 4.0; }

                camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
                SetZoom();
            }
        }

        public void SwapDayNightMode()
        {
            isDay = !isDay;
            if (isDay)
            {
                btnDayNightMode.Image = Properties.Resources.WindowNightMode;

                this.BackColor = frameDayColor;
                foreach (Control c in this.Controls)
                {
                    //if (c is Label || c is Button)
                    {
                        c.ForeColor = textColorDay;
                    }
                }

                foreach (Control c in panelRight.Controls)
                {
                    //if (c is Label || c is Button)
                    {
                        c.ForeColor = textColorDay;
                    }
                }

                foreach (Control c in panelNavigation.Controls)
                {
                    //if (c is Label || c is Button)
                    {
                        c.ForeColor = textColorDay;
                    }
                }
                foreach (Control c in panelControlBox.Controls)
                {
                    //if (c is Label || c is Button)
                    {
                        c.ForeColor = textColorDay;
                    }
                }
            }
            else //nightmode
            {
                btnDayNightMode.Image = Properties.Resources.WindowDayMode;
                this.BackColor = frameNightColor;

                foreach (Control c in this.Controls)
                {
                    {
                        c.ForeColor = textColorNight;
                    }
                }

                foreach (Control c in panelRight.Controls)
                {
                    //if (c is Label || c is Button)
                    {
                        c.ForeColor = textColorNight;
                    }
                }

                foreach (Control c in panelNavigation.Controls)
                {
                    //if (c is Label || c is Button)
                    {
                        c.ForeColor = textColorNight;
                    }
                }

                foreach (Control c in panelControlBox.Controls)
                {
                    //if (c is Label || c is Button)
                    {
                        c.ForeColor = textColorNight;
                    }
                }
            }

            if (tool.isSectionsNotZones)
            {
                LineUpIndividualSectionBtns();
            }
            else
            {
                LineUpAllZoneButtons();
            }

            Properties.Settings.Default.setDisplay_isDayMode = isDay;
            Properties.Settings.Default.Save();
        }

        public void SaveFormGPSWindowSettings()
        {
            //save window settings
            if (WindowState == FormWindowState.Maximized)
            {
                Settings.Default.setWindow_Location = RestoreBounds.Location;
                Settings.Default.setWindow_Size = RestoreBounds.Size;
                Settings.Default.setWindow_Maximized = false;
                Settings.Default.setWindow_Minimized = false;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                Settings.Default.setWindow_Location = Location;
                Settings.Default.setWindow_Size = Size;
                Settings.Default.setWindow_Maximized = false;
                Settings.Default.setWindow_Minimized = false;
            }
            else
            {
                Settings.Default.setWindow_Location = RestoreBounds.Location;
                Settings.Default.setWindow_Size = RestoreBounds.Size;
                Settings.Default.setWindow_Maximized = false;
                Settings.Default.setWindow_Minimized = true;
            }

            Settings.Default.setDisplay_camPitch = camera.camPitch;
            Properties.Settings.Default.setDisplay_camZoom = camera.zoomValue;

            Settings.Default.setF_UserTotalArea = fd.workedAreaTotalUser;

            //Settings.Default.setDisplay_panelSnapLocation = panelSnap.Location;
            Settings.Default.setDisplay_panelSimLocation = panelSim.Location;

            Settings.Default.Save();
        }

        public string FindDirection(double heading)
        {
            if (heading < 0) heading += glm.twoPI;

            heading = glm.toDegrees(heading);

            if (heading > 337.5 || heading < 22.5)
            {
                return (" " +  gStr.gsNorth + " ");
            }
            if (heading > 22.5 && heading < 67.5)
            {
                return (" " +  gStr.gsN_East + " ");
            }
            if (heading > 67.5 && heading < 111.5)
            {
                return (" " +  gStr.gsEast + " ");
            }
            if (heading > 111.5 && heading < 157.5)
            {
                return (" " +  gStr.gsS_East + " ");
            }
            if (heading > 157.5 && heading < 202.5)
            {
                return (" " +  gStr.gsSouth + " ");
            }
            if (heading > 202.5 && heading < 247.5)
            {
                return (" " +  gStr.gsS_West + " ");
            }
            if (heading > 247.5 && heading < 292.5)
            {
                return (" " +  gStr.gsWest + " ");
            }
            if (heading > 292.5 && heading < 337.5)
            {
                return (" " +  gStr.gsN_West + " ");
            }
            return (" ?? ");
        }

        //Mouse Clicks 
        private void oglMain_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                int centerX = oglMain.Width / 2;
                int centerY = oglMain.Height / 2;

                //0 at bottom for opengl, 0 at top for windows, so invert Y value
                Point point = oglMain.PointToClient(Cursor.Position);

                if (isJobStarted)
                {
                    if (isBtnAutoSteerOn || yt.isYouTurnBtnOn)
                    {
                        //uturn and swap uturn direction
                        if (point.Y < 90 && point.Y > 30 && (trk.idx > -1))
                        {

                            int middle = oglMain.Width / 2 + oglMain.Width / 5;
                            if (point.X > middle - 80 && point.X < middle + 80)
                            {
                                SwapDirection();
                                yt.turnTooCloseTrigger = false;
                                yt.isTurnCreationTooClose = false;
                                return;
                            }

                            //k turn or u turn
                            middle += 140;
                            if (point.X > middle - 25 && point.X < middle + 25)
                            {
                                yt.uTurnStyle++;
                                if (yt.uTurnStyle > 1) yt.uTurnStyle = 0;
                                yt.ResetCreatedYouTurn();

                                Properties.Settings.Default.set_uTurnStyle = yt.uTurnStyle;
                                Properties.Settings.Default.Save();

                                return;
                            }

                            //manual uturn triggering
                            middle = oglMain.Width / 2 - oglMain.Width / 4;
                            if (point.X > middle - 140 && point.X < middle && isUTurnOn)
                            {
                                if (yt.isYouTurnTriggered)
                                {
                                    yt.ResetYouTurn();
                                }
                                else
                                {
                                    if (vehicle.functionSpeedLimit > avgSpeed)
                                    {
                                        yt.isYouTurnTriggered = true;
                                        yt.BuildManualYouTurn(false, true);
                                    }
                                    else
                                    {
                                        SpeedLimitExceeded();
                                    }
                                    return;
                                }
                            }

                            if (point.X > middle && point.X < middle + 140 && isUTurnOn)
                            {
                                if (yt.isYouTurnTriggered)
                                {
                                    yt.ResetYouTurn();
                                }
                                else
                                {
                                    if (vehicle.functionSpeedLimit > avgSpeed)
                                    {
                                        yt.isYouTurnTriggered = true;
                                        yt.BuildManualYouTurn(true, true);
                                    }
                                    else
                                    {
                                        SpeedLimitExceeded();
                                    }

                                    return;
                                }
                            }
                        }

                        //lateral
                        if (point.Y < 150 && point.Y > 90 && (trk.idx > -1))
                        {
                            int middle = oglMain.Width / 2 - oglMain.Width / 4;
                            if (point.X > middle - 140 && point.X < middle && isLateralOn)
                            {
                                if (vehicle.functionSpeedLimit > avgSpeed)
                                {
                                    yt.BuildManualYouLateral(false);
                                }
                                else
                                {
                                    SpeedLimitExceeded();
                                }

                                return;
                            }

                            if (point.X > middle && point.X < middle + 140 && isLateralOn)
                            {
                                if (vehicle.functionSpeedLimit > avgSpeed)
                                {
                                    yt.BuildManualYouLateral(true);
                                }
                                else
                                {
                                    SpeedLimitExceeded();
                                }

                                return;
                            }
                        }
                    }

                    //pan and hide menus
                    if (point.X > 30 && point.X < 60)
                    {
                        if (point.Y > 50 && point.Y < 80)
                        {
                            isPanFormVisible = true;
                            Form f = Application.OpenForms["FormPan"];

                            if (f != null)
                            {
                                f.Focus();
                                return;
                            }

                            Form form = new FormPan(this);
                            form.Show(this);

                            form.Top = this.Top + 90;
                            form.Left = this.Left + 120;
                        }

                        if (isJobStarted)
                        {
                            if (point.Y > oglMain.Height - 60 && point.Y < oglMain.Height - 30)
                            {
                                isPanelBottomHidden = !isPanelBottomHidden;
                                PanelsAndOGLSize();
                                return;
                            }
                        }
                    }

                    //tram override
                    if (point.Y > 68 && point.Y < 120)
                    {
                        if (point.X > centerX - 100 && point.X < centerX - 40)
                        {
                            tram.isLeftManualOn = !tram.isLeftManualOn;
                        }
                        if (point.X > centerX + 40 && point.X < centerX + 100)
                        {
                            tram.isRightManualOn = !tram.isRightManualOn;
                        }
                    }
                }

                //prevent flag selection if flag form is up
                Form fc = Application.OpenForms["Flags"];
                if (fc != null)
                {
                    fc.Focus();
                    return;
                }

                //zoom buttons
                if (point.X > oglMain.Width - 80)
                {
                    //---
                    if (point.Y < 210 && point.Y > 120)
                    {
                        if (camera.zoomValue <= 20) camera.zoomValue += camera.zoomValue * 0.2;
                        else camera.zoomValue += camera.zoomValue * 0.1;
                        if (camera.zoomValue > 180) camera.zoomValue = 180;
                        camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
                        SetZoom();
                        return;
                    }

                    //++
                    if (point.Y < 120 && point.Y > 30)
                    {
                        if (camera.zoomValue <= 20)
                        { if ((camera.zoomValue -= camera.zoomValue * 0.2) < 4.0) camera.zoomValue = 4.0; }
                        else { if ((camera.zoomValue -= camera.zoomValue * 0.1) < 4.0) camera.zoomValue = 4.0; }

                        camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
                        SetZoom();
                        return;
                    }
                }

                //vehicle direcvtion reset
                if (point.X > centerX - 40 && point.X < centerX + 40
                    && point.Y > centerY - 60 && point.Y < centerY + 60)
                {
                    if (headingFromSource == "Dual") return;

                    Array.Clear(stepFixPts, 0, stepFixPts.Length);
                    isFirstHeadingSet = false;
                    isReverse = false;
                    TimedMessageBox(2000, "Reset Direction", "Drive Forward > 1.5 kmh");
                    return;
                }

                mouseX = point.X;
                mouseY = oglMain.Height - point.Y;
                leftMouseDownOnOpenGL = true;
            }
        }
        private void SpeedLimitExceeded()
        {
            if (isMetric)
            {
                TimedMessageBox(2000, gStr.gsTooFast, gStr.gsSlowDownBelow + " " 
                    + vehicle.functionSpeedLimit.ToString("N0") + " "+ gStr.gsKMH);
            }
            else
            {
                TimedMessageBox(2000, gStr.gsTooFast, gStr.gsSlowDownBelow + " "
                    + (vehicle.functionSpeedLimit* 0.621371).ToString("N1") + " " + gStr.gsMPH);
           }
        }

        public void SwapDirection()
        {
            if (!yt.isYouTurnTriggered)
            {
                yt.isYouTurnRight = !yt.isYouTurnRight;
                yt.ResetCreatedYouTurn();
            }
            else if (yt.isYouTurnBtnOn)
                btnAutoYouTurn.PerformClick();
        }

        //Function to delete flag
        public void DeleteSelectedFlag()
        {
            //delete selected flag and set selected to none
            flagPts.RemoveAt(flagNumberPicked - 1);
            flagNumberPicked = 0;

            // re-sort the id's based on how many flags left
            int flagCnt = flagPts.Count;
            if (flagCnt > 0)
            {
                for (int i = 0; i < flagCnt; i++) flagPts[i].ID = i + 1;
            }
        }
        public void EnableYouTurnButtons()
        {
            yt.ResetYouTurn();
            yt.isYouTurnBtnOn = false;
            btnAutoYouTurn.Image = Properties.Resources.YouTurnNo;
        }
        public void DisableYouTurnButtons()
        {
            yt.isYouTurnBtnOn = false;
            btnAutoYouTurn.Image = Properties.Resources.YouTurnNo;
            yt.ResetYouTurn();
        }

        private void ShowNoGPSWarning()
        {
            //update main window
            sentenceCounter = 300;
            oglMain.MakeCurrent();
            oglMain.Refresh();
        }

        #region Properties // ---------------------------------------------------------------------

        public string Latitude { get { return Convert.ToString(Math.Round(pn.latitude, 7)); } }
        public string Longitude { get { return Convert.ToString(Math.Round(pn.longitude, 7)); } }
        public string SatsTracked { get { return Convert.ToString(pn.satellitesTracked); } }
        public string HDOP { get { return Convert.ToString(pn.hdop); } }
        public string Heading { get { return Convert.ToString(Math.Round(glm.toDegrees(fixHeading), 1)) + "\u00B0"; } }
        public string GPSHeading { get { return (Math.Round(glm.toDegrees(gpsHeading), 1)) + "\u00B0"; } }
        public string FixQuality
        {
            get
            {
                if (pn.fixQuality == 0) return "Invalid: ";
                else if (pn.fixQuality == 1) return "GPS single: ";
                else if (pn.fixQuality == 2) return "DGPS: ";
                else if (pn.fixQuality == 3) return "PPS: ";
                else if (pn.fixQuality == 4) return "RTK fix: ";
                else if (pn.fixQuality == 5) return "RTK Float: ";
                else if (pn.fixQuality == 6) return "Estimate: ";
                else if (pn.fixQuality == 7) return "Man IP: ";
                else if (pn.fixQuality == 8) return "Sim: ";
                else return "Unknown: ";
            }
        }
        public string GyroInDegrees
        {
            get
            {
                if (ahrs.imuHeading != 99999)
                    return Math.Round(ahrs.imuHeading, 1) + "\u00B0";
                else return "-";
            }
        }
        public string RollInDegrees
        {
            get
            {
                if (ahrs.imuRoll != 88888)
                    return Math.Round((ahrs.imuRoll), 1) + "\u00B0";
                else return "-";
            }
        }
        public string SetSteerAngle { get { return ((double)(guidanceLineSteerAngle) * 0.01).ToString("N1"); } }
        public string ActualSteerAngle { get { return ((mc.actualSteerAngleDegrees) ).ToString("N1") ; } }

        //Metric and Imperial Properties
        public string SpeedMPH
        {
            get
            {
                if (avgSpeed > 2)
                    return (avgSpeed * 0.62137).ToString("N1");
                else
                    return(avgSpeed * 0.62137).ToString("N2");
            }
        }
        public string SpeedKPH
        {
            get
            {
                if (avgSpeed > 2)
                    return (avgSpeed).ToString("N1");
                else
                    return (avgSpeed).ToString("N2");
            }
        }

        public string FixOffset { get { return (pn.fixOffset.easting.ToString("N2") + ", " + pn.fixOffset.northing.ToString("N2")); } }
        public string FixOffsetInch { get { return ((pn.fixOffset.easting*glm.m2in).ToString("N0")+ ", " + (pn.fixOffset.northing*glm.m2in).ToString("N0")); } }

        public string Altitude { get { return Convert.ToString(Math.Round(pn.altitude,2)); } }
        public string AltitudeFeet { get { return Convert.ToString((Math.Round((pn.altitude * 3.28084),1))); } }
        public string DistPivotM
        {
            get
            {
                if (distancePivotToTurnLine > 0 )
                    return ((int)(distancePivotToTurnLine)) + " m";
                else return "--";
            }
        }
        public string DistPivotFt
        {
            get
            {
                if (distancePivotToTurnLine > 0 ) return (((int)(glm.m2ft * (distancePivotToTurnLine))) + " ft");
                else return "--";
            }
        }

        #endregion properties 

        //Load Bitmaps brand
        public Bitmap GetTractorBrand(TBrand brand)
        {
            Bitmap bitmap;
            if (brand == TBrand.Case)
                bitmap = Resources.z_TractorCase;
            else if (brand == TBrand.Claas)
                bitmap = Resources.z_TractorClaas;
            else if (brand == TBrand.Deutz)
                bitmap = Resources.z_TractorDeutz;
            else if (brand == TBrand.Fendt)
                bitmap = Resources.z_TractorFendt;
            else if (brand == TBrand.JDeere)
                bitmap = Resources.z_TractorJDeere;
            else if (brand == TBrand.Kubota)
                bitmap = Resources.z_TractorKubota;
            else if (brand == TBrand.Massey)
                bitmap = Resources.z_TractorMassey;
            else if (brand == TBrand.NewHolland)
                bitmap = Resources.z_TractorNH;
            else if (brand == TBrand.Same)
                bitmap = Resources.z_TractorSame;
            else if (brand == TBrand.Steyr)
                bitmap = Resources.z_TractorSteyr;
            else if (brand == TBrand.Ursus)
                bitmap = Resources.z_TractorUrsus;
            else if (brand == TBrand.Valtra)
                bitmap = Resources.z_TractorValtra;
            else
                bitmap = Resources.z_TractorAoG;

            return bitmap;
        }

        public Bitmap GetHarvesterBrand(HBrand brandH)
        {
            Bitmap harvesterbitmap;
            if (brandH == HBrand.Case)
                harvesterbitmap = Resources.z_HarvesterCase;
            else if (brandH == HBrand.Claas)
                harvesterbitmap = Resources.z_HarvesterClaas;
            else if (brandH == HBrand.JDeere)
                harvesterbitmap = Resources.z_HarvesterJD;
            else if (brandH == HBrand.NewHolland)
                harvesterbitmap = Resources.z_HarvesterNH;
            else
                harvesterbitmap = Resources.z_HarvesterAoG;

            return harvesterbitmap;
        }

        public Bitmap Get4WDBrandFront(WDBrand brandWDF)
        {
            Bitmap bitmap4WDFront;
            if (brandWDF == WDBrand.Case)
                bitmap4WDFront = Resources.z_4WDFrontCase;
            else if (brandWDF == WDBrand.Challenger)
                bitmap4WDFront = Resources.z_4WDFrontChallenger;
            else if (brandWDF == WDBrand.JDeere)
                bitmap4WDFront = Resources.z_4WDFrontJDeere;
            else if (brandWDF == WDBrand.NewHolland)
                bitmap4WDFront = Resources.z_4WDFrontNH;
            else if (brandWDF == WDBrand.Holder)
                bitmap4WDFront = Resources.z_4WDFrontHolder;
            else
                bitmap4WDFront = Resources.z_4WDFrontAoG;

            return bitmap4WDFront;
        }
        
        public Bitmap Get4WDBrandRear(WDBrand brandWDR)
        {
            Bitmap bitmap4WDRear;
            if (brandWDR == WDBrand.Case)
                bitmap4WDRear = Resources.z_4WDRearCase;
            else if (brandWDR == WDBrand.Challenger)
                bitmap4WDRear = Resources.z_4WDRearChallenger;
            else if (brandWDR == WDBrand.JDeere)
                bitmap4WDRear = Resources.z_4WDRearJDeere;
            else if (brandWDR == WDBrand.NewHolland)
                bitmap4WDRear = Resources.z_4WDRearNH;
            else if (brandWDR == WDBrand.Holder)
                bitmap4WDRear = Resources.z_4WDRearHolder;
            else
                bitmap4WDRear = Resources.z_4WDRearAoG;

            return bitmap4WDRear;
        }

    }//end class
}//end namespace