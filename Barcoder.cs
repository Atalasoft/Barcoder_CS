using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Atalasoft.Imaging.WinControls;
using Atalasoft.Barcoding.Reading;
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging;
using Atalasoft.Imaging.ImageProcessing.Document;

namespace BarcodeDemo
{
	/// <summary>
	/// Demonstration of Atalasoft barcode recognition.
	/// </summary>
	/// 
	public class Barcoder : System.Windows.Forms.Form
	{
        AtalaImage _tmpImg = null;

		// To follow the process of recognition of a barcode, recognizeButton_Click
		// and recognizeBarcodes are the main methods that should be examined.
		// recognizeButton_Click handles nearly all the user-interface feedback
		// leaving the work of recognition to recognizeBarcodes, which is written
		// as much as possible to be separate from the UI and the specific
		// implementation.
		//
		// Various options are set in the callback methods.

		private void recognizeButton_Click(object sender, System.EventArgs e)
		{
			// Respond to a recognize request - this button is disabled if it's not
			// appropriate to do a recognize (options that don't make sense,
			// no file loaded)
			BarCode[] results = null;

			// swap in a wait cursor
			System.Windows.Forms.Cursor savedCursor = this.Cursor;
			this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

			DateTime start, end;
			TimeSpan elapsed;

			// time the process
			start = DateTime.Now;

            if (workspaceViewer.Image == null)
            {
                MessageBox.Show("An image needs to be loaded first.");
                return;
            }
            using (BarCodeReader readEngine = new BarCodeReader(workspaceViewer.Image, this.useAutomaticThresholding_CheckBox.Checked))
            {
                results = recognizeBarcodes(readEngine, options);
            }
	
			end = DateTime.Now;
			elapsed = end-start;

			this.Cursor = savedCursor;

			statusBox.AppendText(results.Length + " total barcode" +
				(results.Length == 1 ? "" : "s") + " found.\r\n");
			
			if (results != null && results.Length > 0) 
			{
				statusBox.AppendText("Found " + results.Length +
					" barcode" + (results.Length > 1 ? "s" : "") + ":\r\n");
				for (int i = 0; i < results.Length; i++) 
				{
					statusBox.AppendText("      Result #" + (i+1) + "\r\n");
                    statusBox.AppendText("           Direction: " + results[i].ReadDirection.ToString() + "\r\n");
                    statusBox.AppendText("           Symbology: " + symbologyToString(results[i].Symbology, mySymbologyMap) + "\r\n");
					statusBox.AppendText("           Text read: " + results[i].DataString + "\r\n");
				}
			}
			statusBox.AppendText("Total time: " + System.String.Format("{0:0.000}", elapsed.TotalSeconds) +
				" seconds.\r\n");

			finalResults = results;
			workspaceViewer.Invalidate();
		}

		// Read a set of barcodes from an image.
		// 
        private BarCode[] recognizeBarcodes(BarCodeReader reader, ReadOpts optionsIn) 
		{

            Atalasoft.Barcoding.Reading.BarCode[] results = null;
            Atalasoft.Barcoding.Reading.ReadOpts options = new Atalasoft.Barcoding.Reading.ReadOpts(optionsIn);

			if (options.Symbology == 0) 
			{
				return null;
			}

			try 
			{
				results = reader.ReadBars(options);
			}
			catch (ArgumentOutOfRangeException ex) 
			{
				statusBox.AppendText("Range error in options: " + ex.Message + "\r\n");
			}
			catch (System.Exception ex) 
			{
				statusBox.AppendText("General error: " + ex.Message + "\r\n");
			}
			
			return results;
		}

		// private class for mapping internal symbology names into
		// the user interface
		private class SymbologyMap 
		{
            public SymbologyMap(string name, Symbologies sym) 
			{
				UIName = name;
				symbology = sym;
			}
			// ToString is vital - it allows this object to live transparently
			// in a ListBox and have its UIName displayed.
			public override string ToString() { return UIName; }
			public string UIName;
            public Symbologies symbology;
		}

		// private class for mapping internal scan directions into
		// the user interface
		private class ScanDirectionMap 
		{
            public ScanDirectionMap(string name, Directions dir)
			{
				UIName = name;
				direction = dir;
			}
			// ToString is vital - it allows this object to live transparently
			// in a ListBox and have its UIName displayed.
			public override string ToString() { return UIName; }
			public string UIName;
            public Directions direction;
		}

// members used for barcode recognition
		// maps from internal enumerations to UIStrings
		private SymbologyMap[] mySymbologyMap;
		private ScanDirectionMap[] directionMap;

        private BarCode[] finalResults = null;
        private ReadOpts options;
		private bool imageLoaded = false;

        // members used for the UI
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox shrinkToFit;
		private System.Windows.Forms.CheckBox showBoundingRects;
        private System.Windows.Forms.CheckBox showBoundingBoxes;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Button openButton;
        private Button recognizeButton;
        private CheckedListBox barcodeSymbologyList;
        private Button btnSelectAllSymbologies;
        private Button btnClearSymbologies;
        private GroupBox groupBox1;
        private CheckedListBox scanDirectionList;
        private Button btnSelectAllDirections;
        private Button btnClearAllDirections;
        private GroupBox groupBox2;
        private TrackBar scanInterval;
        private Label scanIntervalLabel;
        private GroupBox groupBox3;
        private TrackBar expectedBarCodes;
        private Label expectedBarcodesLabel;
        private GroupBox groupBox4;
        private Button aboutBtn;
        private CheckBox useAutomaticThresholding_CheckBox;
        private TextBox statusBox;
        private WorkspaceViewer workspaceViewer;
        private RadioButton btnMorphoErode;
        private GroupBox groupBox5;
        private RadioButton btnMorphoDilate;
        private RadioButton btnMorphoNone;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Barcoder()
		{
			// build the maps first since they get used by the UI
			// so make sure that they're constructed before the UI
			// gets built.
            mySymbologyMap = new SymbologyMap[] {
                    new SymbologyMap("Aztec", Symbologies.Aztec),
                    new SymbologyMap("Australia Post", Symbologies.AustraliaPost),
                    new SymbologyMap("Codabar", Symbologies.Codabar),
                    new SymbologyMap("Code 11", Symbologies.Code11),
                    new SymbologyMap("Code 128", Symbologies.Code128),
                    new SymbologyMap("Code 32", Symbologies.Code32),
                    new SymbologyMap("Code 39", Symbologies.Code39),
                    new SymbologyMap("Code 93", Symbologies.Code93),
                    new SymbologyMap("Data Matrix", Symbologies.Datamatrix),
                    new SymbologyMap("Ean 13", Symbologies.Ean13),
                    new SymbologyMap("Ean 8", Symbologies.Ean8),
                    new SymbologyMap("I 2 of 5", Symbologies.I2of5),
                    new SymbologyMap("Intelligent Mail", Symbologies.IntelligentMail),
                    new SymbologyMap("ITF-14", Symbologies.Itf14),
                    new SymbologyMap("Micro QR Code", Symbologies.MicroQr),
                    new SymbologyMap("Patch", Symbologies.Patch),
                    new SymbologyMap("PDF 417", Symbologies.Pdf417),
                    new SymbologyMap("Planet", Symbologies.Planet),
                    new SymbologyMap("Plus 2", Symbologies.Plus2),
                    new SymbologyMap("Plus 5", Symbologies.Plus5),
                    new SymbologyMap("Postnet", Symbologies.Postnet),
                    new SymbologyMap("QR", Symbologies.Qr),
                    new SymbologyMap("Royal Mail +4 State Customer Code", Symbologies.Rm4scc),
                    new SymbologyMap("RSS-14", Symbologies.Rss14),
                    new SymbologyMap("RSS Limited", Symbologies.RssLimited),
                    new SymbologyMap("Telepen", Symbologies.Telepen),
                    new SymbologyMap("UPC A", Symbologies.Upca),
                    new SymbologyMap("UPC E", Symbologies.Upce),
                    };

			directionMap = new ScanDirectionMap[] {
				new ScanDirectionMap("Left to Right", Directions.East),
				new ScanDirectionMap("Right to Left", Directions.West),
				new ScanDirectionMap("Top to Bottom", Directions.South),
				new ScanDirectionMap("Bottom to Top", Directions.North),
				new ScanDirectionMap("Bottom Left to Top Right", Directions.NorthEast),
				new ScanDirectionMap("Top Left to Bottom Right", Directions.SouthEast),
				new ScanDirectionMap("Top Right to Bottom Left", Directions.SouthWest),
				new ScanDirectionMap("Bottom Right to Top Left", Directions.NorthWest),
			};

			// Ensure we'll open the image with one of our licensed decoders
			AtalaDemos.HelperMethods.PopulateDecoders(RegisteredDecoders.Decoders);
            //

            // Sets the default Pixel Format Changer to use Thresholding.
            // This may cause problems for people with PhotoFree or PhotoPro licenses.
            AtalaImage.PixelFormatChanger = new DocumentPixelFormatChanger(new AtalaPixelFormatChanger());
			
            InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Barcoder));
            this.label1 = new System.Windows.Forms.Label();
            this.shrinkToFit = new System.Windows.Forms.CheckBox();
            this.showBoundingRects = new System.Windows.Forms.CheckBox();
            this.showBoundingBoxes = new System.Windows.Forms.CheckBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.openButton = new System.Windows.Forms.Button();
            this.recognizeButton = new System.Windows.Forms.Button();
            this.barcodeSymbologyList = new System.Windows.Forms.CheckedListBox();
            this.btnSelectAllSymbologies = new System.Windows.Forms.Button();
            this.btnClearSymbologies = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.scanDirectionList = new System.Windows.Forms.CheckedListBox();
            this.btnSelectAllDirections = new System.Windows.Forms.Button();
            this.btnClearAllDirections = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.scanInterval = new System.Windows.Forms.TrackBar();
            this.scanIntervalLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.expectedBarCodes = new System.Windows.Forms.TrackBar();
            this.expectedBarcodesLabel = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.aboutBtn = new System.Windows.Forms.Button();
            this.useAutomaticThresholding_CheckBox = new System.Windows.Forms.CheckBox();
            this.statusBox = new System.Windows.Forms.TextBox();
            this.workspaceViewer = new Atalasoft.Imaging.WinControls.WorkspaceViewer();
            this.btnMorphoErode = new System.Windows.Forms.RadioButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnMorphoDilate = new System.Windows.Forms.RadioButton();
            this.btnMorphoNone = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scanInterval)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.expectedBarCodes)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Arial Black", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Orange;
            this.label1.Location = new System.Drawing.Point(12, -3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(476, 27);
            this.label1.TabIndex = 1;
            this.label1.Text = "Atalasoft Barcode Reader Demo";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.UseMnemonic = false;
            // 
            // shrinkToFit
            // 
            this.shrinkToFit.Location = new System.Drawing.Point(20, 25);
            this.shrinkToFit.Name = "shrinkToFit";
            this.shrinkToFit.Size = new System.Drawing.Size(120, 18);
            this.shrinkToFit.TabIndex = 11;
            this.shrinkToFit.Text = "Shrink Image to Fit";
            this.shrinkToFit.CheckedChanged += new System.EventHandler(this.shrinkToFit_CheckedChanged);
            // 
            // showBoundingRects
            // 
            this.showBoundingRects.Checked = true;
            this.showBoundingRects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBoundingRects.Location = new System.Drawing.Point(148, 25);
            this.showBoundingRects.Name = "showBoundingRects";
            this.showBoundingRects.Size = new System.Drawing.Size(168, 18);
            this.showBoundingRects.TabIndex = 12;
            this.showBoundingRects.Text = "Show Bounding Rectangles";
            this.showBoundingRects.CheckedChanged += new System.EventHandler(this.showBoundingRects_CheckedChanged);
            // 
            // showBoundingBoxes
            // 
            this.showBoundingBoxes.Checked = true;
            this.showBoundingBoxes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBoundingBoxes.Location = new System.Drawing.Point(324, 25);
            this.showBoundingBoxes.Name = "showBoundingBoxes";
            this.showBoundingBoxes.Size = new System.Drawing.Size(144, 18);
            this.showBoundingBoxes.TabIndex = 13;
            this.showBoundingBoxes.Text = "Show Bounding Boxes";
            this.showBoundingBoxes.CheckedChanged += new System.EventHandler(this.showBoundingBoxes_CheckedChanged);
            // 
            // openButton
            // 
            this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.openButton.Location = new System.Drawing.Point(22, 285);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(96, 23);
            this.openButton.TabIndex = 2;
            this.openButton.Text = "Open Image...";
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // recognizeButton
            // 
            this.recognizeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.recognizeButton.Location = new System.Drawing.Point(134, 285);
            this.recognizeButton.Name = "recognizeButton";
            this.recognizeButton.Size = new System.Drawing.Size(80, 24);
            this.recognizeButton.TabIndex = 4;
            this.recognizeButton.Text = "Recognize...";
            this.recognizeButton.Click += new System.EventHandler(this.recognizeButton_Click);
            // 
            // barcodeSymbologyList
            // 
            this.barcodeSymbologyList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.barcodeSymbologyList.CheckOnClick = true;
            this.barcodeSymbologyList.Location = new System.Drawing.Point(8, 16);
            this.barcodeSymbologyList.Name = "barcodeSymbologyList";
            this.barcodeSymbologyList.Size = new System.Drawing.Size(174, 64);
            this.barcodeSymbologyList.TabIndex = 6;
            this.barcodeSymbologyList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.barcodeSymbologyList_ItemCheck);
            // 
            // btnSelectAllSymbologies
            // 
            this.btnSelectAllSymbologies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectAllSymbologies.Location = new System.Drawing.Point(188, 19);
            this.btnSelectAllSymbologies.Name = "btnSelectAllSymbologies";
            this.btnSelectAllSymbologies.Size = new System.Drawing.Size(64, 32);
            this.btnSelectAllSymbologies.TabIndex = 7;
            this.btnSelectAllSymbologies.Text = "Select All";
            this.btnSelectAllSymbologies.UseVisualStyleBackColor = true;
            this.btnSelectAllSymbologies.Click += new System.EventHandler(this.btnSelectAllSymbologies_Click);
            // 
            // btnClearSymbologies
            // 
            this.btnClearSymbologies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearSymbologies.Location = new System.Drawing.Point(188, 57);
            this.btnClearSymbologies.Name = "btnClearSymbologies";
            this.btnClearSymbologies.Size = new System.Drawing.Size(64, 23);
            this.btnClearSymbologies.TabIndex = 8;
            this.btnClearSymbologies.Text = "Clear All";
            this.btnClearSymbologies.UseVisualStyleBackColor = true;
            this.btnClearSymbologies.Click += new System.EventHandler(this.btnClearSymbologies_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnClearSymbologies);
            this.groupBox1.Controls.Add(this.btnSelectAllSymbologies);
            this.groupBox1.Controls.Add(this.barcodeSymbologyList);
            this.groupBox1.Location = new System.Drawing.Point(222, 285);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 88);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Barcode Types";
            // 
            // scanDirectionList
            // 
            this.scanDirectionList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.scanDirectionList.CheckOnClick = true;
            this.scanDirectionList.Location = new System.Drawing.Point(8, 16);
            this.scanDirectionList.Name = "scanDirectionList";
            this.scanDirectionList.Size = new System.Drawing.Size(174, 64);
            this.scanDirectionList.TabIndex = 9;
            this.scanDirectionList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.scanDirectionList_ItemCheck);
            // 
            // btnSelectAllDirections
            // 
            this.btnSelectAllDirections.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectAllDirections.Location = new System.Drawing.Point(188, 16);
            this.btnSelectAllDirections.Name = "btnSelectAllDirections";
            this.btnSelectAllDirections.Size = new System.Drawing.Size(64, 35);
            this.btnSelectAllDirections.TabIndex = 9;
            this.btnSelectAllDirections.Text = "Select All";
            this.btnSelectAllDirections.UseVisualStyleBackColor = true;
            this.btnSelectAllDirections.Click += new System.EventHandler(this.btnSelectAllDirections_Click);
            // 
            // btnClearAllDirections
            // 
            this.btnClearAllDirections.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearAllDirections.Location = new System.Drawing.Point(188, 57);
            this.btnClearAllDirections.Name = "btnClearAllDirections";
            this.btnClearAllDirections.Size = new System.Drawing.Size(64, 23);
            this.btnClearAllDirections.TabIndex = 10;
            this.btnClearAllDirections.Text = "Clear All";
            this.btnClearAllDirections.UseVisualStyleBackColor = true;
            this.btnClearAllDirections.Click += new System.EventHandler(this.btnClearAllDirections_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnClearAllDirections);
            this.groupBox2.Controls.Add(this.btnSelectAllDirections);
            this.groupBox2.Controls.Add(this.scanDirectionList);
            this.groupBox2.Location = new System.Drawing.Point(222, 373);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(260, 88);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Scan Directions";
            // 
            // scanInterval
            // 
            this.scanInterval.AutoSize = false;
            this.scanInterval.Location = new System.Drawing.Point(8, 40);
            this.scanInterval.Maximum = 20;
            this.scanInterval.Minimum = 1;
            this.scanInterval.Name = "scanInterval";
            this.scanInterval.Size = new System.Drawing.Size(176, 20);
            this.scanInterval.TabIndex = 0;
            this.scanInterval.TickFrequency = 5;
            this.scanInterval.Value = 5;
            this.scanInterval.ValueChanged += new System.EventHandler(this.scanInterval_ValueChanged);
            // 
            // scanIntervalLabel
            // 
            this.scanIntervalLabel.Location = new System.Drawing.Point(88, 16);
            this.scanIntervalLabel.Name = "scanIntervalLabel";
            this.scanIntervalLabel.Size = new System.Drawing.Size(24, 16);
            this.scanIntervalLabel.TabIndex = 1;
            this.scanIntervalLabel.Text = "5";
            this.scanIntervalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.scanIntervalLabel.UseMnemonic = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.scanIntervalLabel);
            this.groupBox3.Controls.Add(this.scanInterval);
            this.groupBox3.Location = new System.Drawing.Point(22, 317);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(192, 64);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Scan Interval";
            // 
            // expectedBarCodes
            // 
            this.expectedBarCodes.AutoSize = false;
            this.expectedBarCodes.Location = new System.Drawing.Point(8, 40);
            this.expectedBarCodes.Minimum = 1;
            this.expectedBarCodes.Name = "expectedBarCodes";
            this.expectedBarCodes.Size = new System.Drawing.Size(176, 20);
            this.expectedBarCodes.TabIndex = 1;
            this.expectedBarCodes.TickFrequency = 3;
            this.expectedBarCodes.Value = 1;
            this.expectedBarCodes.ValueChanged += new System.EventHandler(this.expectedBarCodes_ValueChanged);
            // 
            // expectedBarcodesLabel
            // 
            this.expectedBarcodesLabel.Location = new System.Drawing.Point(88, 16);
            this.expectedBarcodesLabel.Name = "expectedBarcodesLabel";
            this.expectedBarcodesLabel.Size = new System.Drawing.Size(24, 16);
            this.expectedBarcodesLabel.TabIndex = 2;
            this.expectedBarcodesLabel.Text = "1";
            this.expectedBarcodesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.expectedBarcodesLabel.UseMnemonic = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox4.Controls.Add(this.expectedBarcodesLabel);
            this.groupBox4.Controls.Add(this.expectedBarCodes);
            this.groupBox4.Location = new System.Drawing.Point(22, 397);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(192, 64);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Number of Expected Barcodes";
            // 
            // aboutBtn
            // 
            this.aboutBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.aboutBtn.Location = new System.Drawing.Point(404, 602);
            this.aboutBtn.Name = "aboutBtn";
            this.aboutBtn.Size = new System.Drawing.Size(88, 24);
            this.aboutBtn.TabIndex = 14;
            this.aboutBtn.Text = "About ...";
            this.aboutBtn.Click += new System.EventHandler(this.aboutBtn_Click);
            // 
            // useAutomaticThresholding_CheckBox
            // 
            this.useAutomaticThresholding_CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.useAutomaticThresholding_CheckBox.AutoSize = true;
            this.useAutomaticThresholding_CheckBox.Checked = true;
            this.useAutomaticThresholding_CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useAutomaticThresholding_CheckBox.Location = new System.Drawing.Point(18, 607);
            this.useAutomaticThresholding_CheckBox.Name = "useAutomaticThresholding_CheckBox";
            this.useAutomaticThresholding_CheckBox.Size = new System.Drawing.Size(346, 17);
            this.useAutomaticThresholding_CheckBox.TabIndex = 15;
            this.useAutomaticThresholding_CheckBox.Text = "Enable Automatic Thresholding (Try Toggling If Codes Not Reading)";
            this.useAutomaticThresholding_CheckBox.UseVisualStyleBackColor = true;
            // 
            // statusBox
            // 
            this.statusBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.statusBox.Location = new System.Drawing.Point(14, 469);
            this.statusBox.Multiline = true;
            this.statusBox.Name = "statusBox";
            this.statusBox.ReadOnly = true;
            this.statusBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.statusBox.Size = new System.Drawing.Size(468, 72);
            this.statusBox.TabIndex = 5;
            this.statusBox.DoubleClick += new System.EventHandler(this.statusBox_DoubleClick);
            // 
            // workspaceViewer
            // 
            this.workspaceViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.workspaceViewer.AntialiasDisplay = Atalasoft.Imaging.WinControls.AntialiasDisplayMode.ScaleToGray;
            this.workspaceViewer.DisplayProfile = null;
            this.workspaceViewer.Location = new System.Drawing.Point(30, 49);
            this.workspaceViewer.Magnifier.BackColor = System.Drawing.Color.White;
            this.workspaceViewer.Magnifier.BorderColor = System.Drawing.Color.Black;
            this.workspaceViewer.Magnifier.Size = new System.Drawing.Size(100, 100);
            this.workspaceViewer.Name = "workspaceViewer";
            this.workspaceViewer.OutputProfile = null;
            this.workspaceViewer.Selection = null;
            this.workspaceViewer.Size = new System.Drawing.Size(444, 228);
            this.workspaceViewer.TabIndex = 0;
            this.workspaceViewer.Text = "The Image";
            this.workspaceViewer.Paint += new System.Windows.Forms.PaintEventHandler(this.workspaceViewer_Paint);
            // 
            // btnMorphoErode
            // 
            this.btnMorphoErode.AutoSize = true;
            this.btnMorphoErode.Location = new System.Drawing.Point(283, 14);
            this.btnMorphoErode.Name = "btnMorphoErode";
            this.btnMorphoErode.Size = new System.Drawing.Size(53, 17);
            this.btnMorphoErode.TabIndex = 2;
            this.btnMorphoErode.Text = "Erode";
            this.btnMorphoErode.UseVisualStyleBackColor = true;
            this.btnMorphoErode.CheckedChanged += new System.EventHandler(this.btnMorphoErode_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.btnMorphoErode);
            this.groupBox5.Controls.Add(this.btnMorphoDilate);
            this.groupBox5.Controls.Add(this.btnMorphoNone);
            this.groupBox5.Location = new System.Drawing.Point(12, 560);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(352, 36);
            this.groupBox5.TabIndex = 17;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Morphology Command to Apply";
            // 
            // btnMorphoDilate
            // 
            this.btnMorphoDilate.AutoSize = true;
            this.btnMorphoDilate.Location = new System.Drawing.Point(150, 14);
            this.btnMorphoDilate.Name = "btnMorphoDilate";
            this.btnMorphoDilate.Size = new System.Drawing.Size(52, 17);
            this.btnMorphoDilate.TabIndex = 1;
            this.btnMorphoDilate.Text = "Dilate";
            this.btnMorphoDilate.UseVisualStyleBackColor = true;
            this.btnMorphoDilate.CheckedChanged += new System.EventHandler(this.btnMorphoDilate_CheckedChanged);
            // 
            // btnMorphoNone
            // 
            this.btnMorphoNone.AutoSize = true;
            this.btnMorphoNone.Checked = true;
            this.btnMorphoNone.Location = new System.Drawing.Point(6, 14);
            this.btnMorphoNone.Name = "btnMorphoNone";
            this.btnMorphoNone.Size = new System.Drawing.Size(51, 17);
            this.btnMorphoNone.TabIndex = 0;
            this.btnMorphoNone.TabStop = true;
            this.btnMorphoNone.Text = "None";
            this.btnMorphoNone.UseVisualStyleBackColor = true;
            this.btnMorphoNone.CheckedChanged += new System.EventHandler(this.btnMorphoNone_CheckedChanged);
            // 
            // Barcoder
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(508, 632);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.useAutomaticThresholding_CheckBox);
            this.Controls.Add(this.aboutBtn);
            this.Controls.Add(this.showBoundingBoxes);
            this.Controls.Add(this.showBoundingRects);
            this.Controls.Add(this.statusBox);
            this.Controls.Add(this.workspaceViewer);
            this.Controls.Add(this.shrinkToFit);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.recognizeButton);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Barcoder";
            this.Text = "Atalasoft Barcode Reader Demo";
            this.Load += new System.EventHandler(this.Barcoder_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scanInterval)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.expectedBarCodes)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Barcoder());
		}

		private void Barcoder_Load(object sender, System.EventArgs e)
		{
			recognizeButton.Enabled = false;

			// set a few reasonable default options
            options = new ReadOpts();
            options.Symbology = Symbologies.Code39;
            options.Direction = Directions.East;
			// counter-intuitive - these defaults get pulled from the UI instead
			// of being pushed into the UI
			options.ScanInterval = scanInterval.Value;
			options.ScanBarsToRead = expectedBarCodes.Value;

			// map the options into the UI
			mapBarcodeSymbologies(barcodeSymbologyList, options, mySymbologyMap);
			mapScanDirections(scanDirectionList, options, directionMap);
			statusBox.AppendText("Application loaded.  Click \"Open Image\" to load an image.\r\n");

			// This should point to the "DotImage 7.0\Images\Barcodes" folder.
			this.openFileDialog1.FileName = System.IO.Path.GetFullPath(@"..\..\Images\Barcodes\Code39Scanned.gif");
			if (!System.IO.File.Exists(this.openFileDialog1.FileName))
			{
				this.openFileDialog1.FileName = System.IO.Path.GetFullPath(@"..\..\..\..\..\Images\Barcodes\Code39Scanned.gif");
				if (!System.IO.File.Exists(this.openFileDialog1.FileName))	
					this.openFileDialog1.FileName = "";
			}
		}

        private void mapScanDirections(System.Windows.Forms.CheckedListBox listBox, ReadOpts theOptions, ScanDirectionMap[] map)
		{
			// put each scan direction into the check box,
			// checking it as needed.
			for (int i=0; i < map.Length; i++) 
			{
				listBox.Items.Add(map[i]);
				if ((theOptions.Direction & map[i].direction) != 0) 
				{
					listBox.SetItemChecked(i, true);
				}
			}
		}

        private void btnSelectAllDirections_Click(object sender, EventArgs e)
        {
            CheckUncheckAllListItems(scanDirectionList, true);
        }

        private void btnClearAllDirections_Click(object sender, EventArgs e)
        {
            CheckUncheckAllListItems(scanDirectionList, false);
        }

        private void mapBarcodeSymbologies(System.Windows.Forms.CheckedListBox listBox, ReadOpts theOptions, SymbologyMap[] map)
		{
			// put each symbology into the check box,
			// checking it as needed.
            BarCodeReader reader = new BarCodeReader(); // we will use this to check supported symbologies
			for (int i=0; i < map.Length; i++) 
			{
                if (reader.IsSymbologySupported(map[i].symbology))
                {
                    listBox.Items.Add(map[i]);
                    if ((theOptions.Symbology & map[i].symbology) != 0)
                    {
                        // if any symbology didn't get added, then i is not the correct index so just update the last
                        listBox.SetItemChecked(listBox.Items.Count - 1, true);
                    }
                }
			}
		}

        /// <summary>
        /// Generic for checking/unchecking all options in a Checked List Box
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="status"></param>
        private void CheckUncheckAllListItems(System.Windows.Forms.CheckedListBox listBox, bool status)
        {
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                listBox.SetItemChecked(i, status);
            }
        }

        /// <summary>
        /// Check all sybmology items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectAllSymbologies_Click(object sender, EventArgs e)
        {
            CheckUncheckAllListItems(barcodeSymbologyList, true);
        }

        /// <summary>
        /// Uncheck all symbology items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearSymbologies_Click(object sender, EventArgs e)
        {
            CheckUncheckAllListItems(barcodeSymbologyList, false);
        }

		// Look up the string value of a given symbology
        private string symbologyToString(Symbologies sym, SymbologyMap[] map)
		{
			for (int i=0; i < map.Length; i++) 
			{
				if (sym == map[i].symbology) 
				{
					return map[i].UIName;
				}
			}
			return "Unknown";
		}

		// Respond to a file open
		private void openButton_Click(object sender, System.EventArgs e)
		{
			// try to locate images folder
			string imagesFolder = Application.ExecutablePath;

			// we assume we are running under the DotImage install folder
			int pos = imagesFolder.IndexOf("DotImage ");
			if (pos != -1)
			{
				imagesFolder = imagesFolder.Substring(0,imagesFolder.IndexOf(@"\",pos)) + @"\Images\Barcodes";
			}

			//use this folder as starting point			
			this.openFileDialog1.InitialDirectory = imagesFolder;

			// Filter on the available, licensed decoders
			this.openFileDialog1.Filter = AtalaDemos.HelperMethods.CreateDialogFilter(true);
            //

			if (this.openFileDialog1.ShowDialog(this) != DialogResult.OK) 
			{
				return;
			}

            try
            {
                this._tmpImg = new AtalaImage(openFileDialog1.FileName);

                // Loads the image into the viewer - applying the desired morphology if needed
                UpdateMorphology();
                //workspaceViewer.Open(openFileDialog1.FileName);
            }
            catch
            {
                MessageBox.Show("Unable to load file " + openFileDialog1.FileName + ".");
                imageLoaded = false;
                return;
            }
			imageLoaded = true;

			// check if its OK start a recognize at this point
			validateRecognize(0, 0);
			finalResults = null;
		}

		// user has de/selected a barcode Symbology set
		private void barcodeSymbologyList_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			int index = e.Index;
			CheckState cs = e.NewValue;
			SymbologyMap map = (SymbologyMap)barcodeSymbologyList.Items[index];
			if (map != null) 
			{
				if (cs == CheckState.Checked) 
				{
					options.Symbology |= map.symbology;
				}
				else 
				{
					options.Symbology &= ~map.symbology;
				}
				// this callback semantics are off in the sense that the controls
				// count of checked items isn't updated until after all
				// callbacks have been hit, which means that we can't tell
				// how many are checked without adding a delta in.
				// We'll let Validate handle the delta.
				validateRecognize(cs == CheckState.Checked ? 1 : -1, 0);
			}
		}

		// user has de/selected a scan direction
		private void scanDirectionList_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			int index = e.Index;
			CheckState cs = e.NewValue;
			ScanDirectionMap map = (ScanDirectionMap)scanDirectionList.Items[index];
			if (map != null) 
			{
				if (cs == CheckState.Checked) 
				{
					options.Direction |= map.direction;
				}
				else 
				{
					options.Direction &= ~map.direction;
				}
				// this callback semantics are off in the sense that the controls
				// count of checked items isn't updated until after all
				// callbacks have been hit, which means that we can't tell
				// how many are checked without adding a delta in.
				// We'll let Validate handle the delta.
				validateRecognize(0, cs == CheckState.Checked ? 1 : -1);
			}
		}

		// Make sure that it is OK to proceed with recognition
		private void validateRecognize(int symbologyDeltaOnChangedCheck, int scanDirectionDeltaOnChangedCheck)
		{
			// To be ready for recognition, an image must be loaded,
			// a symbology must be selected and a scan direction
			// must be selected.
			recognizeButton.Enabled = imageLoaded &&
				barcodeSymbologyList.CheckedItems.Count + symbologyDeltaOnChangedCheck != 0 &&
				scanDirectionList.CheckedItems.Count + scanDirectionDeltaOnChangedCheck != 0; 
		}

		private void scanInterval_ValueChanged(object sender, System.EventArgs e)
		{
			// give feedback on the current value of the control
			scanIntervalLabel.Text = scanInterval.Value.ToString();
			if (options != null)
				options.ScanInterval = scanInterval.Value;
		}

		private void expectedBarCodes_ValueChanged(object sender, System.EventArgs e)
		{
			// give feedback on the current value of the control
			expectedBarcodesLabel.Text = expectedBarCodes.Value.ToString();
			options.ScanBarsToRead = expectedBarCodes.Value;
		}

		private void workspaceViewer_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// paint the frames on top of the existing image

			// no results, nothing to do.
			if (finalResults == null) 
			{
				return;
			}

			drawResultsPolygons(e.Graphics, finalResults);
		}

        private void drawResultsPolygons(System.Drawing.Graphics g, BarCode[] results) 
		{
			System.Drawing.Pen penBlue = new Pen(System.Drawing.Color.Blue, 4);
			System.Drawing.Pen penOrange = new Pen(System.Drawing.Color.Orange, 1);
			double zoom = workspaceViewer.Zoom;
			for (int i=0; i < results.Length; i++) 
			{
				// handle the bounding rectangles
				if (showBoundingRects.Checked) 
				{
					System.Drawing.Rectangle r = results[i].BoundingRect;
					// the bounding rects that come back can have negative values in
					// some cases.
					if (r.Width < 0) 
					{
						r.X += r.Width;
						r.Width = -r.Width;
					}
					if (r.Height < 0) 
					{
						r.Y += r.Height;
						r.Height = -r.Height;
					}
					// scale and offset the bounding rect.
					r.X = (int)(r.X * zoom);
					r.Y = (int)(r.Y * zoom);
					r.Width = (int)(r.Width * zoom);
					r.Height = (int)(r.Height * zoom);
					r.Offset(workspaceViewer.ImagePosition);
					g.DrawRectangle(penBlue, r);
				}
				
				// handle the bounding boxes (quadrilaterals)
				if (showBoundingBoxes.Checked) 
				{
					System.Drawing.Point p1, p2, p3, p4;
					p1 = scaleAndOffset(results[i].StartEdgePoints[0], zoom, workspaceViewer.ImagePosition);
					p2 = scaleAndOffset(results[i].StartEdgePoints[1], zoom, workspaceViewer.ImagePosition);
					p3 = scaleAndOffset(results[i].EndEdgePoints[1], zoom, workspaceViewer.ImagePosition);
					p4 = scaleAndOffset(results[i].EndEdgePoints[0], zoom, workspaceViewer.ImagePosition);
					g.DrawLine(penOrange, p1, p2);
					g.DrawLine(penOrange, p2, p3);
					g.DrawLine(penOrange, p3, p4);
					g.DrawLine(penOrange, p4, p1);
				}
			}
			penBlue.Dispose();
			penOrange.Dispose();
		}

		private Point scaleAndOffset(Point source, double zoom, Point offset)
		{
			// scale and offset a point from image space into view space
			Point dest = new Point(source.X, source.Y);
			dest.X = (int)(dest.X * zoom);
			dest.Y = (int)(dest.Y * zoom);
			dest.Offset(offset.X, offset.Y);

			return dest;
		}

		private void shrinkToFit_CheckedChanged(object sender, System.EventArgs e)
		{
			if (shrinkToFit.Checked) 
			{
				workspaceViewer.AutoZoom = AutoZoomMode.BestFitShrinkOnly;
			}
			else 
			{
				workspaceViewer.AutoZoom = AutoZoomMode.None;
				workspaceViewer.Zoom = 1.0;
			}
		}

		private void showBoundingBoxes_CheckedChanged(object sender, System.EventArgs e)
		{
			if (finalResults != null) 
			{
				workspaceViewer.Invalidate();
			}
		}

		private void showBoundingRects_CheckedChanged(object sender, System.EventArgs e)
		{
			if (finalResults != null) 
			{
				workspaceViewer.Invalidate();
			}		
		}

		private void aboutBtn_Click(object sender, System.EventArgs e)
		{
			AtalaDemos.AboutBox.About aboutBox = new AtalaDemos.AboutBox.About("About Atalasoft DotImage Barcode Reader Demo",
				"DotImage Barcode Reader Demo");
			aboutBox.Description = @"The Barcode Reader Demo demonstrates how to read a barcode from an image.  This demo should be used to gain a basic understanding of how the DotImage Barcode recognition functions.  The demo allows you to set options, such as barcode types, scan directions, scan interval and the number of expected barcodes.  If you are having trouble recognizing a barcode, this demo may help to see why.  Requires DotImage and DotImage BarcodeReader.";
			aboutBox.ShowDialog();
		}

        private void statusBox_DoubleClick(object sender, EventArgs e)
        {
            ResultForm resForm = new ResultForm(statusBox.Text);
            resForm.ShowDialog();
        }


        #region Morphology Selection
        private void btnMorphoNone_CheckedChanged(object sender, EventArgs e)
        {
            if (btnMorphoNone.Checked)
            {
                UpdateMorphology("None");
            }
        }

        private void btnMorphoDilate_CheckedChanged(object sender, EventArgs e)
        {
            if (btnMorphoDilate.Checked)
            {
                UpdateMorphology("Dilate");
            }
        }

        private void btnMorphoErode_CheckedChanged(object sender, EventArgs e)
        {
            if (btnMorphoErode.Checked)
            {
                UpdateMorphology("Erode");
            }
        }


        private void UpdateMorphology()
        {
            // call the default
            UpdateMorphology("None");
            this.btnMorphoNone.Checked = true;
            
        }

        private void UpdateMorphology(string mode)
        {
            // load fresh COPY of the image
            if (this._tmpImg != null)
            {
                workspaceViewer.Image = (AtalaImage)this._tmpImg.Clone();

                switch (mode)
                {
                    case "Dilate":
                        workspaceViewer.ApplyCommand(new MorphoDocumentCommand() { Mode = MorphoDocumentMode.Dilation, ApplyToAnyPixelFormat = true });
                        break;
                    case "Erode":
                        workspaceViewer.ApplyCommand(new MorphoDocumentCommand() { Mode = MorphoDocumentMode.Erosion, ApplyToAnyPixelFormat = true });
                        break;
                    case "None":
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please select a file to open.");
            }
        }
        #endregion Morphology Selection
    }
}
