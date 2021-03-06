﻿/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FluidSys;

namespace FluidUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary> 
    public partial class MainWindow : Window {

        ConfigMgr cfg = new ConfigMgr();
        Window aboutWindow = new Window();

        int mouseDownLoc;
        public int internalRefBpm = 120;

        private bool mouseDownOverBPM = false;
        private bool isEditorMode = false;

        public enum EditorMode {
            Editing, Tuning,
        }

        public EditorMode EditMode {
            get {
                if (isEditorMode) return EditorMode.Tuning;
                return EditorMode.Editing;
            }
            set {
                try {
                    if (value == EditorMode.Tuning) {
                        menuContainer.Visibility = System.Windows.Visibility.Visible;
                        noteRoll.Margin = new Thickness(0, 75, menuContainer.Width, 0);
                        isEditorMode = true;
                    }
                    else {
                        menuContainer.Visibility = System.Windows.Visibility.Hidden;
                        noteRoll.Margin = new Thickness(0, 75, 0, 0);
                        isEditorMode = false;
                    }

                    //noteRoll.RollEditMode = EditMode;
                }
                catch (Exception) { }
            }
        }


        public MainWindow() {
            InitializeComponent();

            //make shift rest system needs rest path to be specified:
            noteRoll.RestPath = System.AppDomain.CurrentDomain.BaseDirectory;

            // Read settings files for sample bank and resynth engine:
            System.IO.StreamReader sr;

            try {
                noteRoll.MasterSampleBank = cfg.DefaultSamplebank;
                samplebankButton.Content = noteRoll.MasterSampleBank.Substring(noteRoll.MasterSampleBank.LastIndexOf("\\") + 1);

                if (System.IO.File.Exists(FluidSys.FluidSys.SettingsDir + "\\resynthengine")) {
                    sr = new System.IO.StreamReader(FluidSys.FluidSys.SettingsDir + "\\resynthengine");
                    noteRoll.ResynthEngine = sr.ReadToEnd();
                    sr.Close();

                    engineButton.Content = noteRoll.ResynthEngine.Substring(noteRoll.ResynthEngine.LastIndexOf("\\") + 1);
                }
            }
            catch (Exception ex) { }

            noteRoll.NoteSelected += noteRoll_NoteSelected;
            noteRoll.DefNoteSize = NoteRoll.Snapping.Quarter;

            fluidMenu.prefEvent += FluidMenu_prefEvent;

            initIconButtons();
        }

        private void initIconButtons() {
            pauseBtn.SetIcon(5);
            playBtn.SetIcon(7);
            stopBtn.SetIcon(10);
            snapBtn.SetIcon(8);
            pauseBtn.IconScale = playBtn.IconScale = stopBtn.IconScale = snapBtn.IconScale = 0.8;
        }

        private void FluidMenu_prefEvent() {
            aboutWindow = new Window();
            AboutControl aboutbox = new AboutControl();
            aboutWindow.Content = aboutbox;
            aboutWindow.Opacity = 0.9;
            aboutWindow.ResizeMode = ResizeMode.NoResize;
            aboutWindow.SizeToContent = SizeToContent.WidthAndHeight;
            aboutWindow.WindowStyle = WindowStyle.None;
            aboutWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            aboutWindow.ShowDialog();
        }

        void noteRoll_NoteSelected(Note workingNote) {
            dockedMenu.WorkingNote = workingNote;
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e) {
            InputForm inForm = new InputForm();
            System.Windows.Forms.DialogResult dr = inForm.ShowDialog("Set Sample Bank");
            if (dr == System.Windows.Forms.DialogResult.Yes) noteRoll.MasterSampleBank = inForm.Value;

            samplebankButton.Content = noteRoll.MasterSampleBank.Substring(noteRoll.MasterSampleBank.LastIndexOf("\\") + 1);

            // Write setting to disk
            cfg.DefaultSamplebank = noteRoll.MasterSampleBank;
            projCfgMenu.Visibility = Visibility.Hidden;
        }

        private void Label_MouseDown_1(object sender, MouseButtonEventArgs e) {
            noteRoll.Play();
            //try {
                
            //}
            //catch (Exception ex) {
            //    System.Windows.Forms.MessageBox.Show("Error occured when rendering. \r\n\r\n" + 
            //    "Make sure your sample bank and resampler paths are set and valid. Also check that the " +
            //    "sample bank you chose is compatible with FVSS. If this error persists, please restart FluidUI." + 
            //    "\r\n\r\nDetails: " + ex.Message);
            //}
        }

        private void Label_MouseDown_2(object sender, MouseButtonEventArgs e) {
           InputForm inForm = new InputForm();
            System.Windows.Forms.DialogResult dr = inForm.ShowDialog("Set Resynthesizer");
            if (dr == System.Windows.Forms.DialogResult.Yes) {
                if (System.IO.File.Exists(inForm.Value) && inForm.Value.IndexOf(".exe") != -1) 
                    noteRoll.ResynthEngine = inForm.Value;
                else System.Windows.Forms.MessageBox.Show("The path " +  inForm.Value + " is not a valid executable file.");
            }

            engineButton.Content = noteRoll.ResynthEngine.Substring(noteRoll.ResynthEngine.LastIndexOf("\\") + 1);

            // Write setting to disk
            System.IO.StreamWriter sw = new System.IO.StreamWriter(FluidSys.FluidSys.SettingsDir + "\\resynthengine", false);
            sw.Write(inForm.Value);
            sw.Close();

            projCfgMenu.Visibility = Visibility.Hidden;
        }

        private void BpmLabel_MouseDown(object sender, MouseButtonEventArgs e) {
            mouseDownLoc = (int)Mouse.GetPosition(BpmLabel).Y;
            mouseDownOverBPM = true;
        }

        private void BpmLabel_MouseUp(object sender, MouseButtonEventArgs e) {
            noteRoll.GlobalBPM = internalRefBpm;
            mouseDownOverBPM = false;
        }

        private void BpmLabel_MouseMove(object sender, MouseEventArgs e) {
            if (mouseDownOverBPM) {
                internalRefBpm += (mouseDownLoc - (int)Mouse.GetPosition(BpmLabel).Y) / 3;
                BpmLabel.Content = internalRefBpm.ToString();  
            }
        }

        private void BpmLabel_MouseLeave(object sender, MouseEventArgs e) {
            BpmLabel_MouseUp(sender, null);
        }

        private void fluidMenuButton_Click(object sender, MouseButtonEventArgs e) {
            if (fluidMenu.IsVisible) {
                fluidMenu.Visibility = System.Windows.Visibility.Hidden;
                noteRoll.Effect = null;
            }
            else {
                System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect();
                blur.Radius = 15;

                fluidMenu.Visibility = System.Windows.Visibility.Visible;
                noteRoll.Effect = blur;
            }
        } 
        
        private void showSnappingLight(NoteRoll.Snapping snapping) {
            // Hide all the snapping lights (little circles to the left of the snapping menu items)
            _4snappingLight.Visibility = _8snappingLight.Visibility = _16snappingLight.Visibility =
                _32snappingLight.Visibility = noSnappingLight.Visibility = System.Windows.Visibility.Hidden;

            // Show the desired snapping light 
            if (snapping == NoteRoll.Snapping.Quarter) _4snappingLight.Visibility = System.Windows.Visibility.Visible;
            if (snapping == NoteRoll.Snapping.Eighth) _8snappingLight.Visibility = System.Windows.Visibility.Visible;
            if (snapping == NoteRoll.Snapping.Sixteenth) _16snappingLight.Visibility = System.Windows.Visibility.Visible;
            if (snapping == NoteRoll.Snapping.Thirty_Second) _32snappingLight.Visibility = System.Windows.Visibility.Visible;
            
            //hide snapping menu
            snappingMenu.Visibility = System.Windows.Visibility.Hidden;
        }
        
        private void fluidMenu_MouseLeave(object sender, MouseEventArgs e) {
            fluidMenu.Visibility = System.Windows.Visibility.Hidden;
            noteRoll.Effect = null;
        }

        private void _4snapping_MouseUp(object sender, MouseButtonEventArgs e) {
            noteRoll.NoteSnapping = NoteRoll.Snapping.Quarter;
            showSnappingLight(NoteRoll.Snapping.Quarter);
        }

        private void _8snapping_MouseUp(object sender, MouseButtonEventArgs e) {
            noteRoll.NoteSnapping = NoteRoll.Snapping.Eighth;
            showSnappingLight(NoteRoll.Snapping.Eighth);
        }

        private void _16snapping_MouseUp(object sender, MouseButtonEventArgs e) {
            noteRoll.NoteSnapping = NoteRoll.Snapping.Sixteenth;
            showSnappingLight(NoteRoll.Snapping.Sixteenth);
        }

        private void _32snapping_MouseUp(object sender, MouseButtonEventArgs e) {
            noteRoll.NoteSnapping = NoteRoll.Snapping.Thirty_Second;
            showSnappingLight(NoteRoll.Snapping.Thirty_Second);
        }

        private void noSnapping_MouseUp(object sender, MouseButtonEventArgs e) {
            noteRoll.NoteSnapping = NoteRoll.Snapping.None;
            showSnappingLight(NoteRoll.Snapping.None);
        }

        private void snappingLabel_MouseDown(object sender, MouseButtonEventArgs e) {
           // Show Snapping menu. I'm using MouseDown to open the menu and MouseUp for the menu items
            // so you can use the menu in one click (click down on snapping button, move mouse over item,
            // release mouse to activate item).
            snappingMenu.Visibility = System.Windows.Visibility.Visible;
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e) {
            snappingMenu.Visibility = System.Windows.Visibility.Hidden;
        }

        private void fluidMenu_newEvent() {
            fluidMenu.Visibility = System.Windows.Visibility.Hidden;
        }

        private void fluidMenu_saveEvent() {
            fluidMenu.Visibility = System.Windows.Visibility.Hidden;

            if (noteRoll.IsNewProject) fluidMenu_saveAsEvent();
            else noteRoll.Save("");
        }

        private void fluidMenu_saveAsEvent() {
            fluidMenu.Visibility = System.Windows.Visibility.Hidden;

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            System.Windows.Forms.DialogResult dr;

            sfd.Filter = "FVSS Project | *.fvsp";
            sfd.AutoUpgradeEnabled = false;

            dr = sfd.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK) noteRoll.Save(sfd.FileName);
        }

        private void fluidMenu_exportEvent() {
            fluidMenu.Visibility = System.Windows.Visibility.Hidden;

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            System.Windows.Forms.DialogResult dr;

            sfd.Filter = "WavMOD Render Output | *.wav";
            sfd.AutoUpgradeEnabled = false;

            dr = sfd.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK) noteRoll.ExportWav(sfd.FileName, true);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (modeComboBox.SelectedIndex == 1) EditMode = EditorMode.Tuning;
            else EditMode = EditorMode.Editing;
        }

        private void wavtoolButton_MouseDown(object sender, MouseButtonEventArgs e) {
            InputForm inForm = new InputForm();
            System.Windows.Forms.DialogResult dr = inForm.ShowDialog("Set WavTool");
            if (dr == System.Windows.Forms.DialogResult.Yes) {
                if (System.IO.File.Exists(inForm.Value) && inForm.Value.IndexOf(".exe") != -1)
                    noteRoll.WavTool = inForm.Value;
                else System.Windows.Forms.MessageBox.Show("The path " + inForm.Value + " is not a valid executable file.");
            }

            wavtoolButton.Content = noteRoll.WavTool.Substring(noteRoll.WavTool.LastIndexOf("\\") + 1);
            projCfgMenu.Visibility = Visibility.Hidden;
        }

        private void pencilLabel_MouseDown(object sender, MouseButtonEventArgs e) {
            noteRoll.editorTool = NoteRoll.EditorTool.Pencil;
        }

        private void brushLabel_MouseDown(object sender, MouseButtonEventArgs e) {
            noteRoll.editorTool = NoteRoll.EditorTool.Brush;
        }

        private void projCfgMenu_MouseLeave(object sender, MouseEventArgs e) {
            projCfgMenu.Visibility = Visibility.Hidden;
        }

        private void Label_MouseDown_3(object sender, MouseButtonEventArgs e) {
            projCfgMenu.Visibility = Visibility.Visible;
        }

        private void zoominbtn_MouseDown(object sender, MouseButtonEventArgs e) {
            noteRoll.ZoomIn();
        }

        private void zoomoutbtn_MouseDown(object sender, MouseButtonEventArgs e) {
            noteRoll.ZoomOut();
        }

        private void undoLabel_MouseDown(object sender, MouseButtonEventArgs e) {
            noteRoll.Undo();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            // ctrl shortcuts
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Z) {
                noteRoll.Undo();
            }
        }
    }
}
